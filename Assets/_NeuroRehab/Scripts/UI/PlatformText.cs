using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.XR;

using NeuroRehab.ScriptObjects;

namespace NeuroRehab.UI {
	/// <summary>
	/// Component used for status text message - message containing information about device used, XR status etc
	/// </summary>
	public class PlatformText : MonoBehaviour {

		[SerializeField] private CustomXRSettings xrSettings;
		[SerializeField] private LocalizeStringEvent localizeStringEvent;

		public string platform;
		public string hmdType;
		public string device;

		void Start() {
			InitStatusText();
		}

		private void OnEnable() {
			xrSettings.OnHmdChange += InitStatusText;
		}

		private void OnDisable() {
			xrSettings.OnHmdChange -= InitStatusText;
		}

		private void InitStatusText() {
			if(!gameObject.scene.isLoaded) return;

			// GetComponent<TMP_Text>().text = $"Platform: '{Application.platform}', HMD type: '{XRSettingsManager.Instance.hmdType}', Device: '{XRSettings.loadedDeviceName}'";
			platform = Application.platform.ToString();
			hmdType = xrSettings.HmdType.ToString();
			device = XRSettings.loadedDeviceName;

			localizeStringEvent.RefreshString();
		}
	}
}