using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

using NeuroRehab.ScriptObjects;
using NeuroRehab.Interfaces;
using NeuroRehab.Core;

namespace NeuroRehab.Desktop {
	/// <summary>
	/// Component that handles Drag and Drop on Desktop.
	/// </summary>
	public class DragAndDrop : MonoBehaviour {
		[SerializeField] private MenuHelperSO menuHelper;
		[SerializeField] private InputActionReference mouseClick;

		[SerializeField][Range(0.01f, 100f)] private float mousePhysicsDragSpeed = 10f;
		[SerializeField][Range(0.01f, 10f)] private float mouseDragSpeed = 0.1f;

		[SerializeField] private Camera mainCamera;

		[SerializeField] private LayerMask layersToIgnore;
		[SerializeField] private SingleUnityLayer targetLayer;

		[SerializeField][Range(0.1f, 50f)] private float rayLength = 10f;

		private WaitForFixedUpdate waitForFixedUpdate = new();

		private Vector3 velocity = Vector3.zero;

		private void Awake() {
			if (mainCamera == null) {
				mainCamera = Camera.current;
			}
		}

		private void OnEnable() {
			mouseClick.action.performed += MousePressed;
		}

		private void OnDisable() {
			mouseClick.action.performed -= MousePressed;
		}

		// https://www.youtube.com/watch?v=HfqRKy5oFDQ
		private void MousePressed(InputAction.CallbackContext obj) {
			if (menuHelper.IsMenuShowing) {
				return;
			}

			Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

			if (Physics.Raycast(ray, out RaycastHit hit, rayLength, layerMask: ~layersToIgnore)) {
				if (hit.collider != null && (hit.collider.gameObject.CompareTag("Draggable") || hit.collider.gameObject.layer == targetLayer.LayerIndex)) {
					StartCoroutine(DragUpdate(hit.collider.gameObject));
				}
			}
		}

		private IEnumerator DragUpdate(GameObject draggedObject) {
			Quaternion initRotation = draggedObject.transform.rotation;

			IPickable[] pickables = draggedObject.GetComponents<IPickable>();
			foreach (IPickable pickable in pickables) {
				pickable.OnPickUp();
			}
			IHover[] hoverables = draggedObject.GetComponents<IHover>();
			foreach (IHover hoverable in hoverables) {
				hoverable.OnMouseEnter();
			}

			float initDistance = Vector3.Distance(draggedObject.transform.position, mainCamera.transform.position);
			draggedObject.TryGetComponent(out Rigidbody rigidbody);

			while (mouseClick.action.ReadValue<float>() != 0) {
				if (menuHelper.IsMenuShowing) {
					break;
				}

				Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
				if (rigidbody != null) {
					Vector3 direction = ray.GetPoint(initDistance) - draggedObject.transform.position;
					rigidbody.velocity = direction * mousePhysicsDragSpeed;
					draggedObject.transform.rotation = initRotation;
					yield return waitForFixedUpdate;
				} else {
					draggedObject.transform.position = Vector3.SmoothDamp(draggedObject.transform.position, ray.GetPoint(initDistance), ref velocity, mouseDragSpeed);
					yield return waitForFixedUpdate;
				}
			}

			foreach (IPickable pickable in pickables) {
				pickable.OnRelease();
			}
			foreach (IHover hoverable in hoverables) {
				hoverable.OnMouseExit();
			}
		}
	}
}