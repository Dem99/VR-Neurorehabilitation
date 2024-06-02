using UnityEngine;
using UnityEngine.InputSystem;

using NeuroRehab.ScriptObjects;

namespace NeuroRehab.Desktop {

	/// <summary>
	/// Class used to manage mouse triggers and mouse visibility. Used when there are multiple events that cause mouse to become visible (such as Grab + Menu)
	/// </summary>
	public class MouseManager : MonoBehaviour {
		[Header("Scriptable objects")]
		[SerializeField] private MenuHelperSO menuHelper;

		[Header("Dependencies")]
		[SerializeField] private InputActionReference[] mouseVisibilityTriggers;

		private int triggerCount = 0;
		private int activeTriggers = 0;

		public int ActiveTriggers { get => activeTriggers; }

		private void Awake() {
			activeTriggers = 0;
			Cursor.lockState = CursorLockMode.Locked;
		}

		private void OnEnable() {
			Cursor.lockState = CursorLockMode.Locked;

			foreach (InputActionReference item in mouseVisibilityTriggers) {
				item.action.performed += TriggerVisibility;
			}
			triggerCount = mouseVisibilityTriggers.Length;

			activeTriggers = 0;
			InitCursor();
		}

		private void OnDisable() {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			foreach (InputActionReference item in mouseVisibilityTriggers) {
				item.action.performed -= TriggerVisibility;
			}
		}

		private void TriggerVisibility(InputAction.CallbackContext obj) {
			for (int i = 0; i < triggerCount; i++) {
				if (mouseVisibilityTriggers[i].action == obj.action) {
					activeTriggers ^= (int)Mathf.Pow(2, i); // exclusive OR

					menuHelper.TriggerMarker(obj.action.name);
					break;
				}
			}
			InitCursor();
		}

		private void InitCursor() {
			if (activeTriggers > 0) {
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			} else {
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;

				menuHelper.ResetMarkers();
			}
		}
	}
}