using UnityEngine;

namespace NeuroRehab.Utility {
	[RequireComponent(typeof(Outline))]
	public class PointerOutline : HoverableBase {
		[Header("Dependencies")]
		[SerializeField] private Outline outline;

		private Color initColor;
		private Color transparentColor = new Color(0, 0, 0, 0);

		private void Awake() {
			initColor = outline.OutlineColor;
			outline.OutlineColor = transparentColor;
		}

		public override void OnMouseEnter() {
			outline.OutlineColor = initColor;
		}

		public override void OnMouseExit() {
			outline.OutlineColor = transparentColor;
		}

	}
}