using UnityEngine;
using UnityEngine.InputSystem;

using NeuroRehab.Interfaces;

namespace NeuroRehab.Desktop {
	public class MouseClick : MonoBehaviour {
		[SerializeField] private InputActionReference mouseClick;

		[SerializeField] private Camera mainCamera;

		[SerializeField] private LayerMask layersToIgnore;

		[SerializeField][Range(0.1f, 50f)] private float rayLength = 10f;

		private void Awake() {
			if (mainCamera == null) {
				mainCamera = Camera.main;
			}
		}

		private void OnEnable() {
			mouseClick.action.performed += MouseTargetClicked;
		}

		private void OnDisable() {
			mouseClick.action.performed -= MouseTargetClicked;
		}

		/// <summary>
		/// Triggers MouseCLick event on object that has correct component.
		/// </summary>
		/// <param name="obj"></param>
		private void MouseTargetClicked(InputAction.CallbackContext obj) {
			Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

			if (Physics.Raycast(ray, out RaycastHit hit, rayLength, layerMask: ~layersToIgnore)) {
				if (hit.collider != null && hit.collider.gameObject.TryGetComponent(out IMouseClickable mouseClickable)) {
					mouseClickable.OnMouseClicked();
				}
			}
		}
	}
}