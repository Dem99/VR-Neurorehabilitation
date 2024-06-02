using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Localization.Settings;

using NeuroRehab.Core;
using NeuroRehab.ScriptObjects;
using NeuroRehab.Interfaces;

namespace NeuroRehab.Managers {

	/// <summary>
	/// Contains various settings - RoleSettings, AvatarSettings, AudioSettings, GeneralSettings
	/// Script execution order: SettingsManager -> XRSettingsManager
	/// </summary>
	public class SettingsManager : MonoBehaviour, ISaveable {
		private readonly CustomDebug customDebug = new("SETTINGS");
		private static bool isAppInitialized = false;

		[Header("Scriptable objects")]
		[SerializeField] private LanguageSettings m_languageSettings;
		[SerializeField] private GeneralSettings m_generalSettings;
		[SerializeField] private RoleSettings m_roleSettings;
		[SerializeField] private AvatarSettings m_avatarSettings;
		[SerializeField] private NetworkSettings m_networkSettings;
		[SerializeField] private CustomGraphicsSettings m_graphicsSettings;
		[SerializeField] private CustomAudioSettings m_audioSettings;
		[SerializeField] private OffsetSettings m_offsetSettings;

		[Header("Dependencies")]
		[SerializeField] private LogLevel globalLogLevel = LogLevel.ALL;
		[SerializeField] private string saveFileName = "SaveConfig.dat";

		/// <summary>
		/// Settings version string is used to invalidate settings file when reading it, it can be virtually anything.
		/// Can be used to reset settings to default values. When pushing new version to git with changes to structure it's good to change version, so that other devs will also update settings file when running the game.
		/// </summary>
		private string settingsVersion = "1m";

		void Awake() {
			{
				if (isAppInitialized) {
					Destroy(gameObject);
					return;
				}
				isAppInitialized = true;
				DontDestroyOnLoad(gameObject);
			}
			// fast solution, would be better to rework it to use custom logger, so that we can control better what is / isn't logged
			// https://gamedevbeginner.com/how-to-use-debug-log-in-unity-without-affecting-performance/
			Debug.unityLogger.logEnabled = globalLogLevel != LogLevel.NONE;
			CustomDebug.GlobalLogLevel = globalLogLevel;

			m_networkSettings.IpAddress = "localhost";

			m_graphicsSettings.SetResolution(Screen.width, Screen.height);

			try {
				LoadSettings();
			} catch (System.Exception e) {
				Debug.Log(e);
			}
			StartCoroutine(InitLanguage());

			m_graphicsSettings.RenderScale = m_graphicsSettings.RenderScale;
		}

		void Start() {
			m_graphicsSettings.RefreshFrameRate();
		}

		void OnApplicationQuit() {
			SaveSettings();
		}

		private void SaveSettings() {
			SaveData sd = new();
			PopulateSaveData(sd);

			if (FileManager.WriteToFile(saveFileName, sd.ToJson())) {
				customDebug.LogOk("Succesfully saved Settings");
			}
		}

		private void LoadSettings() {
			if (FileManager.LoadFromFile(saveFileName, out string json)) {
				SaveData sd = new();
				sd.LoadFromJson(json);

				if (!sd.SettingsVersion.Equals(settingsVersion)) {
					customDebug.LogWarning("Invalid Settings Version, settings will not be applied!!");
				} else {
					LoadFromSaveData(sd);
					customDebug.LogOk("Settings sucesfully initialized");
				}
			}
		}

		public void PopulateSaveData(SaveData saveData) {
			saveData.AudioSettings = new(m_audioSettings.UIVolume, m_audioSettings.MasterVolume, m_audioSettings.MusicVolume);
			saveData.AvatarSettings = new(m_avatarSettings.IsFemale, m_avatarSettings.AvatarNumber, m_avatarSettings.SizeMultiplier, m_avatarSettings.OffsetDistance);
			saveData.RoleSettings = new(m_roleSettings.CharacterRole);
			saveData.OffsetSettings = new(m_offsetSettings.MiniMenuInitialized, m_offsetSettings.StaticMenuInitialized, m_offsetSettings.MiniMenuTransformOffset, m_offsetSettings.StaticMenuTransformOffset);
			saveData.GraphicsSettings = new(m_graphicsSettings.RenderScale, m_graphicsSettings.Exposure, m_graphicsSettings.Tonemapping, m_graphicsSettings.Fullscreen, m_graphicsSettings.ScreenWidth, m_graphicsSettings.ScreenHeight, m_graphicsSettings.VSync, m_graphicsSettings.FrameRate);
			saveData.GeneralSettings = new(m_generalSettings.MeasureFps, m_generalSettings.WriteFpsToFile, m_generalSettings.ReticleColor, m_generalSettings.ReticleScale, m_generalSettings.ReticleStyle, m_generalSettings.CursorStyle);
			saveData.IpAddress = m_networkSettings.IpAddress;
			saveData.WebsocketPort = m_networkSettings.WebsocketPort;
			saveData.RestPort = m_networkSettings.RestPort;
			saveData.SettingsVersion = settingsVersion;
			saveData.RoomID = m_networkSettings.GetRoomId();
			saveData.Language = m_languageSettings.GetLanguage();
			saveData.Servers = m_networkSettings.ServersList;
		}

		public void LoadFromSaveData(SaveData saveData) {
			m_avatarSettings.Init(saveData.AvatarSettings.IsFemale, saveData.AvatarSettings.AvatarNumber, saveData.AvatarSettings.SizeMultiplier, saveData.AvatarSettings.OffsetDistance);
			m_roleSettings.Init(saveData.RoleSettings.CharacterRole);
			m_offsetSettings.Init(saveData.OffsetSettings.MiniMenusOffsetInitialized, saveData.OffsetSettings.StaticMenusOffsetInitialized, saveData.OffsetSettings.MiniMenuTransformOffset, saveData.OffsetSettings.StaticMenuTransformOffset);
			m_graphicsSettings.Init(saveData.GraphicsSettings.RenderScale, saveData.GraphicsSettings.Exposure, saveData.GraphicsSettings.Tonemapping, saveData.GraphicsSettings.Fullscreen, saveData.GraphicsSettings.ScreenWidth, saveData.GraphicsSettings.ScreenHeight, saveData.GraphicsSettings.VSync, saveData.GraphicsSettings.FrameRate);
			m_generalSettings.Init(saveData.GeneralSettings.MeasureFps, saveData.GeneralSettings.WriteFpsToFile, saveData.GeneralSettings.ReticleColor, saveData.GeneralSettings.ReticleScale, saveData.GeneralSettings.ReticleStyle, saveData.GeneralSettings.CursorStyle);
			m_networkSettings.Init(saveData.IpAddress, saveData.WebsocketPort, saveData.RestPort, saveData.RoomID, saveData.Servers);
			settingsVersion = saveData.SettingsVersion;

			m_audioSettings.Init(saveData.AudioSettings.UIVolume, saveData.AudioSettings.MasterVolume,saveData.AudioSettings.MusicVolume);

			StartCoroutine(m_languageSettings.SetLanguage(saveData.Language));
		}

		private IEnumerator InitLanguage() {
			// Wait for the localization system to initialize, loading Locales, preloading, etc.
			yield return LocalizationSettings.InitializationOperation;

			int localesCount = LocalizationSettings.AvailableLocales.Locales.Count;
			for (int i = 0; i < localesCount; i++) {
				if (LocalizationSettings.AvailableLocales.Locales[i].Equals(LocalizationSettings.SelectedLocale)) {
					m_languageSettings.SetLanguage((LanguageEnum)i);
					break;
				}
			}
		}
	}
}