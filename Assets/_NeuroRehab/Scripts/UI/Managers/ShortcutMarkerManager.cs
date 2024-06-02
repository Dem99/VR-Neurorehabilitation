using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;

using NeuroRehab.ScriptObjects;

namespace NeuroRehab.UI.Managers {
	public class ShortcutMarkerManager : MonoBehaviour {

		[Header("Scriptable objects")]
		[SerializeField] private MenuHelperSO menuHelper;

		[Header("Dependencies")]
		[SerializeField] private GameObject shortcutMarkerPrefab;

		private readonly List<string> shortcutMarkerItems = new();
		private readonly List<ShortcutMarker> spawnedShortcutMarkers = new();

		private void OnEnable() {
			LocalizationSettings.SelectedLocaleChanged += UpdateMarkers;

			menuHelper.OnMarkerTrigger += TriggerMarker;
			menuHelper.OnMarkersReset += ResetMarkers;
		}

		private void OnDisable() {
			LocalizationSettings.SelectedLocaleChanged -= UpdateMarkers;

			menuHelper.OnMarkerTrigger -= TriggerMarker;
			menuHelper.OnMarkersReset -= ResetMarkers;
		}

		private void UpdateMarkers(Locale locale) {
			foreach (ShortcutMarker item in spawnedShortcutMarkers) {
				item.spawnedShortcutMarker.GetComponentInChildren<TMP_Text>().text = item.localizedString.GetLocalizedString();
			}
		}

		private void AddMarker(string text) {
			if (shortcutMarkerItems.Contains(text)) {
				return;
			}
			shortcutMarkerItems.Add(text);
			InitShortcutMarkers();
		}

		private void RemoveMarker(string text) {
			if (!shortcutMarkerItems.Contains(text)) {
				return;
			}
			shortcutMarkerItems.Remove(text);
			InitShortcutMarkers();
		}

		private void TriggerMarker(string text) {
			if (shortcutMarkerItems.Contains(text)) {
				shortcutMarkerItems.Remove(text);
			} else {
				shortcutMarkerItems.Add(text);
			}
			InitShortcutMarkers();
		}

		private void ResetMarkers() {
			shortcutMarkerItems.Clear();
			InitShortcutMarkers();
		}

		private void InitShortcutMarkers() {
			foreach (ShortcutMarker item in spawnedShortcutMarkers) {
				Destroy(item.spawnedShortcutMarker);
			}
			spawnedShortcutMarkers.Clear();

			int markerCount = shortcutMarkerItems.Count;
			for (int i = 0; i < markerCount; i++) {
				GameObject newShortcutMarker = Instantiate(shortcutMarkerPrefab, transform);
				LocalizedString locString = new("UI", $"{shortcutMarkerItems[i].ToLower()}_marker");

				ShortcutMarker newMarker = new(newShortcutMarker, locString);
				newShortcutMarker.GetComponentInChildren<TMP_Text>().text = locString.GetLocalizedString();

				spawnedShortcutMarkers.Add(newMarker);
			}
		}
	}
}
