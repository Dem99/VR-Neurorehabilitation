using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

using NeuroRehab.Core;
using NeuroRehab.UI.Localization;
using NeuroRehab.ScriptObjects;

namespace NeuroRehab.UI.Managers.Menu.Settings {
	public class GeneralSettingsMenuManager : MonoBehaviour {
		[SerializeField] private GeneralSettings generalSettings;
		[SerializeField] LanguageSettings m_languageSettings;

		[Header("General settings objects")]
		[SerializeField] private TMP_Text reticleScaleTextValue;
		[SerializeField] private Slider reticleScaleSlider;
		[SerializeField] private TMP_Dropdown reticleStyleDropdown;

		[SerializeField] private TMP_Dropdown languageDropdown;

		[SerializeField] private TMP_Dropdown cursorStyleDropdown;

		void Start() {
			reticleScaleSlider.onValueChanged.AddListener(ReticleScaleSliderHandler);
			reticleScaleSlider.value = generalSettings.ReticleScale * 10;
			reticleScaleTextValue.text = $"{generalSettings.ReticleScale * 100} %";

			reticleStyleDropdown.GetComponent<LocalizedDropdown>().UpdateLocalizedOptions(generalSettings.GetReticles());
			reticleStyleDropdown.value = reticleStyleDropdown.options.FindIndex(option =>
				option.text.Equals(generalSettings.GetReticleOption().text.GetLocalizedString()));
			reticleStyleDropdown.RefreshShownValue();

			cursorStyleDropdown.GetComponent<LocalizedDropdown>().UpdateLocalizedOptions(generalSettings.GetCursors());
			cursorStyleDropdown.value = cursorStyleDropdown.options.FindIndex(option =>
				option.text.Equals(generalSettings.GetCursorOption().text.GetLocalizedString()));
			cursorStyleDropdown.RefreshShownValue();

			reticleStyleDropdown.onValueChanged.AddListener(delegate {
				ReticleStyleHandler(reticleStyleDropdown);
			});

			cursorStyleDropdown.onValueChanged.AddListener(delegate {
				CursorStyleHandler(cursorStyleDropdown);
			});

			StartCoroutine(InitLanguageDropdown());
			languageDropdown.onValueChanged.AddListener(delegate {
				LanguageHandler(languageDropdown);
			});
		}

		private void ReticleScaleSliderHandler(float value) {
			generalSettings.ReticleScale = value / 10;
			reticleScaleTextValue.text = $"{value * 10} %";
		}

		private void ReticleStyleHandler(TMP_Dropdown dropdown) {
			foreach (var item in generalSettings.ReticleOptions) {
				if (item.Value.text.GetLocalizedString().Equals(dropdown.options[dropdown.value].text)) {
					generalSettings.ReticleStyle = item.Key;
					break;
				}
			}
		}

		private void CursorStyleHandler(TMP_Dropdown dropdown) {
			foreach (var item in generalSettings.CursorOptions) {
				if (item.Value.text.GetLocalizedString().Equals(dropdown.options[dropdown.value].text)) {
					generalSettings.CursorStyle = item.Key;
					break;
				}
			}
		}

		private void LanguageHandler(TMP_Dropdown dropdown) {
			StartCoroutine(m_languageSettings.SetLanguage((LanguageEnum) dropdown.value));
		}

		private IEnumerator InitLanguageDropdown() {
			yield return LocalizationSettings.InitializationOperation;

			languageDropdown.ClearOptions();
			foreach (LanguageOption item in m_languageSettings.GetLanguageOptions()) {
				languageDropdown.options.Add(new TMP_Dropdown.OptionData(item.languageReadable, item.sprite));
			}
			languageDropdown.value = (int) m_languageSettings.GetLanguage();
			languageDropdown.RefreshShownValue();
		}
	}
}