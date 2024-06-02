using UnityEngine;

using NeuroRehab.Interfaces;

namespace NeuroRehab.Utility {
	public abstract class HoverableBase : MonoBehaviour, IHover {
		public abstract void OnMouseEnter();

		public abstract void OnMouseExit();
	}
}