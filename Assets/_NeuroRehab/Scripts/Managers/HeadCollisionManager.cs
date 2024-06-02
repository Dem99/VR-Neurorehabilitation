using UnityEngine;

using NeuroRehab.Core;

namespace NeuroRehab.Managers {
	/// <summary>
	/// Contains events for HeadCollider when using XR origin. When Collision happened pushes whole user away from the point of 'impact'.
	/// </summary>
	public class HeadCollisionManager : MonoBehaviour {

		[SerializeField] private GameObject XRRig;

		[SerializeField] private CharacterController XRRigCharacterController;

		[SerializeField] private SingleUnityLayer groundLayer;
		[SerializeField] private SingleUnityLayer controllerLayer;
		[SerializeField] private SingleUnityLayer avatarLayer;

		[SerializeField] private float collisionOffset = 0.1f;

		void Awake() {
			Physics.IgnoreLayerCollision(transform.gameObject.layer, transform.gameObject.layer);
			Physics.IgnoreLayerCollision(transform.gameObject.layer, groundLayer.LayerIndex);
			Physics.IgnoreLayerCollision(transform.gameObject.layer, controllerLayer.LayerIndex);
			Physics.IgnoreLayerCollision(transform.gameObject.layer, avatarLayer.LayerIndex);

			if (XRRigCharacterController == null) {
				XRRigCharacterController = XRRig.GetComponent<CharacterController>();
			}
		}

		/// <summary>
		/// Checks collision of object and if head is colliding attempts to move it out of object
		/// </summary>
		/// <param name="collision"></param>
		private void OnCollisionEnter(Collision collision) {
			// customDebug.Log("Collision detected: " + collision.transform.name);

			Vector3 headPosition = transform.TransformPoint(Vector3.zero);

			// customDebug.Log(headPosition);
			foreach (ContactPoint contact in collision.contacts) {
				// customDebug.Log(contact.point);
				float diffX = Mathf.Round((headPosition.x - contact.point.x) * 100) / 100;
				// customDebug.Log(diffX);
				if (diffX != 0f) {
					if (Mathf.Abs(diffX) < collisionOffset) {
						if (diffX < 0f) {
							diffX = -collisionOffset;
						} else {
							diffX = collisionOffset;
						}
					}

					XRRig.transform.position += new Vector3(diffX, 0f, 0f);
					XRRigCharacterController.center += new Vector3(diffX, 0f, 0f);
					break;
				}
				float diffZ = Mathf.Round((headPosition.z - contact.point.z) * 100) / 100;
				// customDebug.Log(diffZ);
				if (diffZ != 0f) {
					if (Mathf.Abs(diffZ) < collisionOffset) {
							if (diffZ < 0f) {
								diffZ = -collisionOffset;
							} else {
								diffZ = collisionOffset;
							}
					}

					XRRig.transform.position += new Vector3(0f, 0f, diffZ);
					XRRigCharacterController.center += new Vector3(0f, 0f, diffZ);
					break;
				}
			}
		}
	}
}