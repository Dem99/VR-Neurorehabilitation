using Mirror;
using Unity.XR.MockHMD;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using System.Collections;
using System.Collections.Generic;

using NeuroRehab.Core;
using NeuroRehab.ScriptObjects;

namespace NeuroRehab.Managers {

	/// <summary>
	/// Similar to SettingsManager, but specific for only XR settings. Used for turning XR On/Off. Attempts to use OpenXR first, and then starts MockHMD.
	/// Script execution order: SettingsManager -> XRSettingsManager
	/// </summary>
	public class XRSettingsManager : MonoBehaviour {
		private readonly CustomDebug customDebug = new("XR_MANAGER");
		private static bool isAppInitialized = false;

		[Header("Scriptable objects")]
		[SerializeField] private CustomXRSettings m_xrSettings;
		[SerializeField] private GeneralSettings m_generalSettings;
		[SerializeField] private CustomGraphicsSettings m_graphicsSettings;

		[Header("Dependencies")]
		[SerializeField, Scene] private string offlineSceneName;

		[SerializeField] private GameObject XRRig;
		[SerializeField] private GameObject desktopRig;

		[SerializeField] private GameObject xrDeviceSimulator;

		private SimpleObjectListManager simpleObjectList;

		private XRLoader activeLoader;
		private bool foundHMD = false;
		private bool xrInitialized;

		private SceneHelper sceneHelper;

		void Awake() {
			{
				if (isAppInitialized) {
					Destroy(gameObject);
					return;
				}
				isAppInitialized = true;
				DontDestroyOnLoad(gameObject);
			}

#if UNITY_ANDROID
			m_xrSettings.XrActive = true;
#endif

#if UNITY_EDITOR
			if (m_xrSettings.XrActive) {
				StartCoroutine(StartXR(UsedXRLoader.OPEN_XR));
			}
#endif

#if UNITY_SERVER
			StopXR();
#endif
		}

		private void Start() {
			sceneHelper = new(SceneManager.GetActiveScene());
			simpleObjectList = sceneHelper.FindRootObject<SimpleObjectListManager>();

			{ // this has to be in Start, otherwise build on android (Oculus) breaks and we get issue with eternal black screen
				SceneManager.activeSceneChanged += ChangedActiveScene;

				InitializeXRSettings();
			}
		}

		private void OnEnable() {
			m_xrSettings.OnXRActiveChange += HandleXRStateChange;
		}

		private void OnDisable() {
			m_xrSettings.OnXRActiveChange -= HandleXRStateChange;
			SceneManager.activeSceneChanged -= ChangedActiveScene;
		}

		void OnApplicationQuit() {
			StopXR();
		}

		private void ChangedActiveScene(Scene current, Scene next) {
			sceneHelper = new(SceneManager.GetActiveScene());
			if (next.buildIndex == SceneUtility.GetBuildIndexByScenePath(offlineSceneName)) {
				InitializeXRSettings();
			}
		}

		private void HandleXRStateChange() {
			if (m_xrSettings.XrActive) {
				StartCoroutine(StartXR(UsedXRLoader.OPEN_XR));
			} else {
				StopXR();
			}
		}

		private void InitializeXRSettings() {
			InitHMD();
			InitObjectReferences();

			if ((XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager.isInitializationComplete) || xrInitialized || activeLoader != null) {
				m_xrSettings.XrActive = true;
			} else {
				m_xrSettings.XrActive = false;
			}
			customDebug.Log($"XR {(!m_xrSettings.XrActive ? "not " : "")}running");

			InitRigs();
			InitXRObjects();
		}

		private IEnumerator StartXR(UsedXRLoader xrLoader) {
			if (xrInitialized) {
				customDebug.Log("XR is already initialized.");
				yield break;
			}

			XRLoader loader = SetupLoader(xrLoader);
			if (loader == null) {
				customDebug.LogError("Error resolving loader");
				m_xrSettings.XrActive = false;
				yield break;
			}

			if (activeLoader == null) {
				try {
					xrInitialized = loader.Initialize();
				} catch {
					xrInitialized = false;
				}
				if (!xrInitialized) {
					customDebug.LogWarning($"Initializing XR loader ({loader.name}) failed. Check log for details.");
					if (xrLoader == UsedXRLoader.OPEN_XR) {
						customDebug.LogWarning($"Failed to start {xrLoader} loader. Attempting to launch Mock HMD loader");
						loader.Deinitialize();

						yield return StartCoroutine(StartXR(UsedXRLoader.MOCK));

						yield break;
					}
				} else {
					customDebug.Log($"Starting XR {loader.name}...");
					loader.Start();
					activeLoader = loader;
				}
			}
			m_xrSettings.XrActive = true;

			InitRigs();
			InitHMD();

			InitXRObjects();
		}

		private void StopXR() {
			bool stoppedXR = StopXRNative();
			stoppedXR = stoppedXR || StopXRManually();

			if (stoppedXR) {
				customDebug.Log("Stopped XR...");
				m_graphicsSettings.RefreshFrameRate();

				InitRigs();
				InitXRObjects();
			}
		}

		private bool StopXRNative() {
			if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager.isInitializationComplete) {
				XRGeneralSettings.Instance.Manager.StopSubsystems();
				XRGeneralSettings.Instance.Manager.DeinitializeLoader();
				return true;
			}
			return false;
		}

		private bool StopXRManually() {
			if (xrInitialized || activeLoader != null) {
				activeLoader.Stop();
				activeLoader.Deinitialize();
				xrInitialized = false;
				activeLoader = null;
				return true;
			}
			return false;
		}

		private void InitXRObjects() {
			if (!gameObject.scene.isLoaded) {
				return;
			}

			if (xrDeviceSimulator == null) {
				InitObjectReferences();
			}

			if (m_xrSettings.XrActive) {
				xrDeviceSimulator.SetActive(m_xrSettings.HmdType == HMDType.Mock);
			} else {
				m_graphicsSettings.RefreshScreenResolution();

				xrDeviceSimulator.SetActive(false);
			}

			InitMockXRSettings();
		}

		private void InitMockXRSettings() {
			if (m_xrSettings.HmdType == HMDType.Mock) {
				XRSettings.gameViewRenderMode = GameViewRenderMode.LeftEye;

				XRSettings.eyeTextureResolutionScale = 1.5f;
			}
		}

		private void InitRigs() {
			if (!gameObject.scene.isLoaded) {
				return;
			}

			if (m_xrSettings.XrActive) {
				desktopRig.SetActive(false);
				XRRig.SetActive(true);
			} else {
				XRRig.SetActive(false);
				desktopRig.SetActive(true);
			}
		}

		/// <summary>
		/// We have to setup objects this way because when user disconnects we would lose references to objects
		/// </summary>
		private void InitObjectReferences() {
			if (xrDeviceSimulator != null && XRRig != null && desktopRig != null) {
				return;
			}

			if (simpleObjectList == null) {
				simpleObjectList = sceneHelper.FindRootObject<SimpleObjectListManager>();
			}
			// we could use simpleObjectListManager.objectList.Find(...), but it's more time consuming (cycling multiple times through array)
			foreach (SimpleObjectListMember item in simpleObjectList.objectList) {
				switch (item.name) {
					case "XR Device Simulator":
						xrDeviceSimulator = item.gameObject;
						break;
					case "XRRig":
						XRRig = item.gameObject;
						break;
					case "DesktopRig":
						desktopRig = item.gameObject;
						break;
					default: break;
				}
			}
		}

		/// <summary>
		/// Initializes HMD type based on either HMD discovered or platform used
		/// </summary>
		private void InitHMD() {
			ResolveHMD();
			ResolvePlatform();
		}

		private void ResolveHMD() {
			m_xrSettings.HmdType = HMDType.Other;

			if (XRSettings.loadedDeviceName == null || XRSettings.loadedDeviceName.Trim().Equals("")) {
				customDebug.Log("No HMD discovered");
				m_xrSettings.HmdType = HMDType.NotFound;
				foundHMD = false;
			} else {
				foundHMD = true;
				customDebug.Log("HMD discovered: " + XRSettings.loadedDeviceName);

				m_xrSettings.HmdType = HMDType.Other;
				if (XRSettings.loadedDeviceName.Equals("MockHMD Display")) {
					// if MockHMD, we make the game view render only using one eye
					XRSettings.gameViewRenderMode = GameViewRenderMode.LeftEye;

					m_xrSettings.HmdType = HMDType.Mock;
				}
			}
		}

		private void ResolvePlatform() {
			if (!foundHMD && (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)) {
				m_xrSettings.HmdType = HMDType.Mock;
			} else if (Application.platform == RuntimePlatform.Android) {
				m_xrSettings.HmdType = HMDType.Android;
			} else if (Application.platform == RuntimePlatform.WindowsServer || Application.platform == RuntimePlatform.LinuxServer) {
				m_xrSettings.HmdType = HMDType.NotFound;
			}
		}

		/// <summary>
		/// Swaps laoders, so that we have only one active loader. Refer to https://forum.unity.com/threads/xr-management-controlling-the-used-xrloader-manually.1019677/ for more details
		/// </summary>
		/// <param name="useOpenXRLoader"></param>
		private XRLoader SetupLoader(UsedXRLoader usedXRLoader) {
			IReadOnlyList<XRLoader> loaders = XRGeneralSettings.Instance.Manager.activeLoaders;
			//bool loaderRemoved = false;
			foreach (XRLoader loader in loaders) {
				if ((usedXRLoader == UsedXRLoader.OPEN_XR && loader.GetType() == typeof(OpenXRLoader)) ||
				(usedXRLoader == UsedXRLoader.MOCK && loader.GetType() == typeof(MockHMDLoader))) {
					return loader;
				}
			}
			return null;
		}

	}
}