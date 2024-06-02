using UnityEngine;

namespace NeuroRehab.ScriptObjects {
	[CreateAssetMenu(menuName = "ScriptObjects/Settings/Offset")]
	public class OffsetSettings : ScriptableObject {
		[SerializeField] private bool miniMenuInitialized = false;
		[SerializeField] private bool staticMenuInitialized = false;
		[SerializeField] private Vector3 miniMenuTransformOffset;
		[SerializeField] private Vector3 staticMenuTransformOffset;
		public bool MiniMenuInitialized { get => miniMenuInitialized; set => miniMenuInitialized = value; }
		public bool StaticMenuInitialized { get => staticMenuInitialized; set => staticMenuInitialized = value; }
		public Vector3 MiniMenuTransformOffset { get => miniMenuTransformOffset; set => miniMenuTransformOffset = value; }
		public Vector3 StaticMenuTransformOffset { get => staticMenuTransformOffset; set => staticMenuTransformOffset = value; }


		public void Init(bool miniMenuInitialized, bool staticMenuInitialized, Vector3 miniMenuTransformOffset, Vector3 staticMenuTransformOffset) {
			this.miniMenuInitialized = miniMenuInitialized;
			this.staticMenuInitialized = staticMenuInitialized;
			this.miniMenuTransformOffset = miniMenuTransformOffset;
			this.staticMenuTransformOffset = staticMenuTransformOffset;
		}
	}
}