using UnityEngine;

namespace NeuroRehab.Utility {
	/// <summary>
	/// Marks object that can be mirrored and provides function for doing so
	/// </summary>
	public class ObjectMirrorable : MonoBehaviour {
		public void MirrorObject() {
			transform.RotateAround(transform.position, transform.right, 180);
		}
	}
}
