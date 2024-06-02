using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace NeuroRehab.Utility {
	public class HoverTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
		[SerializeField] private List<HoverableBase> hoverables;

		public void OnPointerEnter(PointerEventData eventData) {
			foreach (HoverableBase hoverable in hoverables) {
				hoverable.OnMouseEnter();
			}
		}

		public void OnPointerExit(PointerEventData eventData) {
			foreach (HoverableBase hoverable in hoverables) {
				hoverable.OnMouseExit();
			}
		}
	}
}