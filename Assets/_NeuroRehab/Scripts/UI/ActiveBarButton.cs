using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeuroRehab.UI {
	public class ActiveBarButton : MonoBehaviour {
		[SerializeField] private List<Image> activeBars = new();

		private void Start() {
			transform.GetComponent<Button>().onClick.AddListener(ActivateBar);
		}

		public void ActivateBar() {
			foreach (Image activeBar in activeBars) {
				if (activeBar.transform.parent == transform) {
					activeBar.enabled = true;
				} else {
					activeBar.enabled = false;
				}
			}
		}
	}
}