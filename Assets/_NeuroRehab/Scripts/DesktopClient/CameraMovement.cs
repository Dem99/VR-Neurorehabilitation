using UnityEngine;
using UnityEngine.InputSystem;

namespace NeuroRehab.Desktop {
	public class CameraMovement : MonoBehaviour {

		[SerializeField] private InputActionReference mouseX;
		[SerializeField] private InputActionReference mouseY;

		[SerializeField] private MouseManager mouseManager;

		[SerializeField] private Transform player;

		[SerializeField] private float verticalRotation = 0f;
		[SerializeField] private float mouseSensitivityX = 50f;
		[SerializeField] private float mouseSensitivityY = 50f;

		[SerializeField] private float cameraFieldOfView = 60f;


		private void OnEnable() {
			if (TryGetComponent(out Camera camera)) {
				camera.fieldOfView = cameraFieldOfView;
			}
		}

		void Update() {
			if (mouseManager.ActiveTriggers > 0) {
				return;
			}

			float _mouseX = mouseX.action.ReadValue<float>() * mouseSensitivityX * Time.deltaTime;
			float _mouseY = mouseY.action.ReadValue<float>() * mouseSensitivityY * Time.deltaTime;

			verticalRotation -= _mouseY;
			verticalRotation = Mathf.Clamp(verticalRotation, -60f, 60f);

			transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
			player.Rotate(Vector3.up * _mouseX);
		}
	}
}