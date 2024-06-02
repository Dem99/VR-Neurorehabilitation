using UnityEngine;

using NeuroRehab.ScriptObjects;

namespace NeuroRehab.UI {
	public class XRStateButton : MonoBehaviour {
		[SerializeField] private CustomXRSettings xrSettings;

		[SerializeField] private bool xrActive = false;

		[SerializeField] private ActiveBarButton activeBar;

		void OnEnable() {
			xrSettings.OnXRActiveChange += ChangeActiveState;
		}

		void OnDisable() {
			xrSettings.OnXRActiveChange -= ChangeActiveState;
		}

		private void Start() {
			ChangeActiveState();
		}

		private void ChangeActiveState() {
			if (xrActive == xrSettings.XrActive) {
				activeBar.ActivateBar();
			}
		}
	}
}