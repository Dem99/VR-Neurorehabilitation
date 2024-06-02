using UnityEngine;

namespace NeuroRehab.Utility.Positioning {
	public class FreezeObjectPosition : MonoBehaviour {
		private Vector3 initialPosition;

		void OnEnable() {
			initialPosition = transform.position;
		}

		void LateUpdate() {
			transform.position = initialPosition;
		}
	}
}