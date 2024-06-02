using System;
using UnityEngine;

namespace NeuroRehab.ScriptObjects {

	[CreateAssetMenu(menuName = "ScriptObjects/MenuHelper")]
	public class MenuHelperSO : ScriptableObject {
		private bool isMenuShowing;
		private Camera renderingCamera;
		private GameObject mainMenu;

		public event Action OnCameraChange;

		public event Action<string> OnMarkerTrigger;
		public event Action OnMarkersReset;

		public bool IsMenuShowing { get => isMenuShowing; set => isMenuShowing = value; }
		public Camera RenderingCamera {
			get => renderingCamera; set {
				renderingCamera = value;
				OnCameraChange?.Invoke();
			}
		}

		public GameObject MainMenu { get => mainMenu; set => mainMenu = value; }

		public void Init() {
			isMenuShowing = false;
		}

		public void TriggerMarker(string marker) {
			OnMarkerTrigger?.Invoke(marker);
		}

		internal void ResetMarkers() {
			OnMarkersReset?.Invoke();
		}
	}
}