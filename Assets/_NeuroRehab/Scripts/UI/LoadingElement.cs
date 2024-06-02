using UnityEngine;
using Mirror;

namespace NeuroRehab.UI {

	[RequireComponent(typeof(CanvasGroup))]
	public class LoadingElement : MonoBehaviour {
		[SerializeField] private CanvasGroup canvasGroup;
		[SerializeField] private GameObject loadingBarObject;

		[SerializeField] private bool isButton;
		[SerializeField] private bool isLoadingBar;

		private void OnEnable() {
			DisableInteractions();
		}

		private void OnDisable() {
			EnableInteractions();
		}

		private void FixedUpdate() {
			if (!NetworkClient.active) {
				enabled = false;
			}
		}

		private void DisableInteractions() {
			if (isLoadingBar) {
				canvasGroup.alpha = 1;
				loadingBarObject.SetActive(true);
			} else {
				canvasGroup.interactable = false;
				canvasGroup.blocksRaycasts = false;
			}
		}

		private void EnableInteractions() {
			if (isLoadingBar) {
				canvasGroup.alpha = 0;
				loadingBarObject.SetActive(false);
			} else {
				canvasGroup.interactable = true;
				canvasGroup.blocksRaycasts = true;
			}
		}

	}

}