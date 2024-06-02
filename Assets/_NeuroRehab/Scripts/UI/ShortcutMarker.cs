using UnityEngine;
using UnityEngine.Localization;

namespace NeuroRehab.UI {
	public class ShortcutMarker {
		public GameObject spawnedShortcutMarker;
		public LocalizedString localizedString;

		public ShortcutMarker(GameObject _spawnedShortcutMarker, LocalizedString _localizedString) {
			spawnedShortcutMarker = _spawnedShortcutMarker;
			localizedString = _localizedString;
		}
	}
}