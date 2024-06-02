using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NeuroRehab.UI.Managers.Menu.Tab {
	public class TabGroupButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler {

		[SerializeField] private TabGroup tabGroup;

		public Image background;

		public GameObject tabMenu;

		public bool isActive;

		void Start() {
			background = gameObject.GetComponent<Image>();
			tabGroup.RegisterButton(this, isActive);
		}

		public void OnPointerEnter(PointerEventData eventData) {
			tabGroup.OnTabEnter(this);
		}

		public void OnPointerClick(PointerEventData eventData) {
			tabGroup.OnTabSelected(this);
		}

		public void OnPointerExit(PointerEventData eventData) {
			tabGroup.OnTabExit(this);
		}
	}
}