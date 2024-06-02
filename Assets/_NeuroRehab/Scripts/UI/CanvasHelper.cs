using UnityEngine;

using NeuroRehab.ScriptObjects;

namespace NeuroRehab.UI {

	public class CanvasHelper : MonoBehaviour {
		[SerializeField] private MenuHelperSO menuHelper;
		[SerializeField] private Canvas canvas;
		[SerializeField] private bool isMainMenu = false;

		private void Awake() {
			if (isMainMenu) {
				menuHelper.MainMenu = gameObject;
			}
		}

		private void Start() {
			ResetCanvasCamera();
		}

		private void OnEnable() {
			menuHelper.OnCameraChange += ResetCanvasCamera;
		}

		private void OnDisable() {
			menuHelper.OnCameraChange -= ResetCanvasCamera;
		}

		private void ResetCanvasCamera() {
			canvas.enabled = false;

			canvas.renderMode = RenderMode.WorldSpace;
			canvas.worldCamera = menuHelper.RenderingCamera;

			canvas.enabled = true;
		}
	}
}