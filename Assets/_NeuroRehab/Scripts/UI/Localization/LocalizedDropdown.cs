using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;
using System.Collections.Generic;

using NeuroRehab.Utility;
using NeuroRehab.Core;

// Based on https://forum.unity.com/threads/localizing-ui-dropdown-options.896951/
namespace NeuroRehab.UI.Localization {
	public class LocalizedDropdown : MonoBehaviour {
		[SerializeField] private TMP_Dropdown dropdown;
		[SerializeField] private List<LocalizedDropdownOption> localizedOptions;

		private Locale currentLocale = null;

		private void Awake() {
			if (!dropdown) {
				dropdown = GetComponent<TMP_Dropdown>();
			}
		}

		private void OnEnable() {
			UpdateDropdownOptions(LocalizationSettings.SelectedLocale);

			LocalizationSettings.SelectedLocaleChanged += UpdateDropdownOptions;
		}

		private void OnDisable() {
			LocalizationSettings.SelectedLocaleChanged -= UpdateDropdownOptions;
		}

		public void UpdateLocalizedOptions(List<LocalizedDropdownOption> options) {
			localizedOptions = options;
			UpdateDropdownOptions(LocalizationSettings.SelectedLocale, true);
		}

		public void AddLocalizedOption(LocalizedDropdownOption option) {
			localizedOptions.Add(option);
			UpdateDropdownOptions(LocalizationSettings.SelectedLocale, true);
		}

		private void UpdateDropdownOptions(Locale locale) {
			UpdateDropdownOptions(locale, false);
		}

		private void UpdateDropdownOptions(Locale locale, bool force) {
			if (!force && currentLocale == locale) {
				return;
			}
			currentLocale = locale;

			int selectedOptionIndex = dropdown.value;
			dropdown.ClearOptions();
			// Updating all options
			foreach (LocalizedDropdownOption option in localizedOptions) {
				TMP_Dropdown.OptionData newOption = new();

				// Update text
				if (!option.text.IsEmpty) {
					string localizedTextHandle = option.text.GetLocalizedString(locale);
					newOption.text = localizedTextHandle;
				}
				// Simple Sprite (aka fallback)
				if (option.simpleSprite) {
					newOption.image = ConvertToSprite.ConvertTextureToSprite(option.simpleSprite);
				}
				// Update sprite
				if (!option.sprite.IsEmpty) {
					var localizedSpriteHandle = option.sprite.LoadAssetAsync();
					localizedSpriteHandle.Completed += (handle) => {
						newOption.image = localizedSpriteHandle.Result;
					};
				}

				dropdown.options.Add(newOption);
			}

			dropdown.value = selectedOptionIndex;
			dropdown.RefreshShownValue();
		}
	}
}