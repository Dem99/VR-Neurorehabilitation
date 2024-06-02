using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections;

using NeuroRehab.Core;

namespace NeuroRehab.ScriptObjects {

	[CreateAssetMenu(menuName = "ScriptObjects/Settings/Language")]
	public class LanguageSettings : ScriptableObject {
		[SerializeField, NonReorderable] private LanguageOption[] m_languageOptions;
		[SerializeField] private LanguageEnum m_language;

		public LanguageOption[] GetLanguageOptions() {
			return m_languageOptions;
		}

		public LanguageEnum GetLanguage() {
			return m_language;
		}

		public IEnumerator SetLanguage(LanguageEnum _language) {
			// Wait for the localization system to initialize, loading Locales, preloading, etc.
			yield return LocalizationSettings.InitializationOperation;

			m_language = _language;
			LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[(int) m_language];
		}
	}
}