using UnityEngine;

namespace NeuroRehab.Utility.Positioning {
	public class FollowObject : MonoBehaviour {
		[SerializeField] private bool followPosition = true;

		[SerializeField] private bool followPositionX = true;
		[SerializeField] private bool followPositionY = true;
		[SerializeField] private bool followPositionZ = true;

		[SerializeField] private bool followRotation = false;
		[SerializeField] private bool followRotationX = false;
		[SerializeField] private bool followRotationY = false;
		[SerializeField] private bool followRotationZ = false;

		[SerializeField] private GameObject objectToFollow;

		[SerializeField] private Vector3 positionDiff = new();

		[SerializeField] private Vector3 rotationDiff = new();

		// Update is called once per frame
		void LateUpdate() {
			if (followRotation) {
				float rotX = (objectToFollow.transform.rotation.eulerAngles.x + rotationDiff.x) * (followRotationX ? 1 : 0);
				float rotY = (objectToFollow.transform.rotation.eulerAngles.y + rotationDiff.y) * (followRotationY ? 1 : 0);
				float rotZ = (objectToFollow.transform.rotation.eulerAngles.z + rotationDiff.z) * (followRotationZ ? 1 : 0);
				transform.rotation = Quaternion.Euler(rotX, rotY, rotZ);
			}
			if (followPosition) {
				float posX = (objectToFollow.transform.position.x + positionDiff.x) * (followPositionX ? 1 : 0);
				float posY = (objectToFollow.transform.position.y + positionDiff.y) * (followPositionY ? 1 : 0);
				float posZ = (objectToFollow.transform.position.z + positionDiff.z) * (followPositionZ ? 1 : 0);
				transform.position += new Vector3(posX, posY, posZ);
			}
		}
	}
}