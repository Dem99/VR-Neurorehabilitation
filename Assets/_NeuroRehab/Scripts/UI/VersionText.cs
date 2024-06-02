using UnityEngine;
using UnityEngine.Localization.Components;

namespace NeuroRehab.UI {
	public class VersionText : MonoBehaviour {
		public string applicationVersion;
		[SerializeField] private LocalizeStringEvent localizeStringEvent;

		private void Start() {
			applicationVersion = Application.version;

			localizeStringEvent.RefreshString();
		}
	}
}