using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using NeuroRehab.ScriptObjects;
using NeuroRehab.Core;

namespace NeuroRehab.UI.Managers.Menu.Settings {
	public class GraphicsSettingsMenuManager : MonoBehaviour {
		[Header("Scriptable objects")]
		[SerializeField] private GeneralSettings generalSettings;
		[SerializeField] private CustomGraphicsSettings graphicsSettings;

		[Header("Dependencies")]
		[SerializeField] private Toggle measureFpsToggle;
		[SerializeField] private Toggle writeFpsToggle;

		[Header("Graphic quality objects")]
		[SerializeField] private TMP_Text renderScaleTextValue;
		[SerializeField] private Slider renderScaleSlider;

		[SerializeField] private Toggle tonemappingToggle;

		[SerializeField] private TMP_Text brightnessTextValue;
		[SerializeField] private Slider brightnessSlider;

		[Header("Window resolution")]
		[SerializeField] private TMP_Dropdown resolutionsDropdown;
		[SerializeField] private Toggle fullscreenToggle;
		// --------------------------------------------------------------
		private Resolution[] allResolutions;
		private List<ResolutionHelper> supportedResolutions = new();

		private void Awake() {
			allResolutions = Screen.resolutions;
			System.Array.Reverse(allResolutions);

			supportedResolutions.Clear();

			foreach (Resolution resolution in allResolutions) {
				supportedResolutions.Add(new(resolution.width, resolution.height));
			}
			supportedResolutions = supportedResolutions.Distinct().ToList();
		}

		void Start() {
			SetResolutionOptions();

			measureFpsToggle.onValueChanged.AddListener(ToggleFps);
			writeFpsToggle.onValueChanged.AddListener(ToggleFpsWrite);
			measureFpsToggle.isOn = generalSettings.MeasureFps;
			writeFpsToggle.isOn = generalSettings.WriteFpsToFile;

			tonemappingToggle.onValueChanged.AddListener(ToggleTonemapping);
			tonemappingToggle.isOn = graphicsSettings.Tonemapping;

			brightnessSlider.onValueChanged.AddListener(BrightnessSliderHandler);
			brightnessSlider.value = graphicsSettings.Exposure * 100;

			renderScaleTextValue.text = $"{graphicsSettings.RenderScale}";
			renderScaleSlider.onValueChanged.AddListener(RenderScaleSliderHandler);
			renderScaleSlider.value = graphicsSettings.RenderScale * 10;

			fullscreenToggle.onValueChanged.AddListener(ToggleFullscreen);
			fullscreenToggle.isOn = graphicsSettings.Fullscreen;

			resolutionsDropdown.onValueChanged.AddListener(ResolutionDropdownHandler);
		}

		public void ToggleFps(bool value) {
			generalSettings.MeasureFps = value;
		}

		public void ToggleFpsWrite(bool value) {
			generalSettings.WriteFpsToFile = value;

			if (value) {
				generalSettings.MeasureFps = true;
				measureFpsToggle.isOn = true;
			}
		}

		public void ToggleTonemapping(bool value) {
			graphicsSettings.Tonemapping = value;
		}

		private void RenderScaleSliderHandler(float value) {
			graphicsSettings.RenderScale = value / 10;
			renderScaleTextValue.text = $"{value / 10}";
		}

		private void BrightnessSliderHandler(float value) {
			graphicsSettings.Exposure = value / 100;
			brightnessTextValue.text = $"{value} %";
		}

		private void ToggleFullscreen(bool value) {
			graphicsSettings.Fullscreen = value;
		}

		private void ResolutionDropdownHandler(int value) {
			graphicsSettings.SetResolution(supportedResolutions[value].width, supportedResolutions[value].height);
		}

		private void SetResolutionOptions() {
			resolutionsDropdown.ClearOptions();
			int indexOfRes = -1;

			List<string> options = new();
			foreach (ResolutionHelper res in supportedResolutions) {
				options.Add($"{res.width}x{res.height}");
				if (res.width == graphicsSettings.ScreenWidth && res.height == graphicsSettings.ScreenHeight) {
					indexOfRes = supportedResolutions.IndexOf(res);
				}
			}
			if (indexOfRes == -1) { // if not found in supported list, we set highest possible res
				graphicsSettings.SetResolution(supportedResolutions[0].width, supportedResolutions[0].height);
				indexOfRes = 0;
			}

			resolutionsDropdown.AddOptions(options);
			resolutionsDropdown.value = indexOfRes;
			resolutionsDropdown.RefreshShownValue();
		}
	}
}