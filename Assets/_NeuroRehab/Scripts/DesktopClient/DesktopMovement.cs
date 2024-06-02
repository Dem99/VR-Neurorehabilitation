using UnityEngine;
using UnityEngine.InputSystem;
using AYellowpaper;

using NeuroRehab.Interfaces;
using NeuroRehab.Core;

namespace NeuroRehab.Desktop {
	public class DesktopMovement : MonoBehaviour {
		private readonly CustomDebug customDebug = new ("DesktopMovement");
		[SerializeField] private InputActionReference move;

		[SerializeField] private InterfaceReference<IAvatarChanger> avatarChanger;

		[SerializeField] private CharacterController body;
		[SerializeField] private float moveSpeed = 7f;

		private void Awake() {
			if (avatarChanger == null) {
				throw new System.Exception($"Wrong object for 'avatarChanger' on object: '{gameObject.name}'");
			}
		}

		void Start() {
			float sizeMulti = avatarChanger.Value.GetSizeMulti();
			body.height *= sizeMulti;
			body.center *= sizeMulti;
		}

		void Update() {
			Vector2 tempMove = move.action.ReadValue<Vector2>();

			Vector3 sidewayMovement = transform.right * tempMove.x;
			Vector3 forwardMovement = transform.forward * tempMove.y;
			Vector3 movement = sidewayMovement + forwardMovement;

			movement = Vector3.ClampMagnitude(movement, 1f); // double move speed fix

			Vector3 movementVelocity = moveSpeed * Time.deltaTime * movement;
			body.Move(movementVelocity);
		}
	}
}