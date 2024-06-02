using System.Collections.Generic;
using UnityEngine;

namespace NeuroRehab.UI.Managers.Menu.Tab {
	public class TabGroup : MonoBehaviour {

		[SerializeField] private List<TabGroupButton> tabButtons = new();
		[SerializeField] private Sprite tabIdle;
		[SerializeField] private Sprite tabHover;
		[SerializeField] private Sprite tabActive;

		[SerializeField] private List<GameObject> tabObjects = new();

		private TabGroupButton selectedTabButton;

		public void RegisterButton(TabGroupButton tabButton, bool isActive) {
			tabButtons.Add(tabButton);
			tabObjects.Add(tabButton.tabMenu);

			if (isActive) {
				OnTabSelected(tabButton);
			} else {
				if (tabButton.tabMenu.TryGetComponent(out Canvas canvas) && tabButton.tabMenu.TryGetComponent(out CanvasGroup canvasGroup) ) {
					canvas.enabled = false;
					canvasGroup.interactable = false;
					canvasGroup.blocksRaycasts = false;
				} else {
					tabButton.tabMenu.SetActive(false);
				}
			}
			ResetTabs();
		}

		public void OnTabEnter(TabGroupButton tabButton) {
			ResetTabs();
			if (selectedTabButton != null && !selectedTabButton.Equals(tabButton)) {
				tabButton.background.sprite = tabHover;
			}
		}

		public void OnTabExit(TabGroupButton tabButton) {
			ResetTabs();
		}

		public void OnTabSelected(TabGroupButton tabButton) {
			selectedTabButton = tabButton;

			ResetTabs();
			tabButton.background.sprite = tabActive;

			foreach (GameObject tabObject in tabObjects) {
				if (tabObject.Equals(tabButton.tabMenu)) {
					if (tabObject.TryGetComponent(out Canvas canvas) && tabObject.TryGetComponent(out CanvasGroup canvasGroup) ) {
						canvas.enabled = true;
						canvasGroup.interactable = true;
						canvasGroup.blocksRaycasts = true;
					} else {
						tabObject.SetActive(true);
					}
				} else {
					if (tabObject.TryGetComponent(out Canvas canvas) && tabObject.TryGetComponent(out CanvasGroup canvasGroup) ) {
						canvas.enabled = false;
						canvasGroup.interactable = false;
						canvasGroup.blocksRaycasts = false;
					} else {
						tabObject.SetActive(false);
					}
				}
			}
		}

		private void ResetTabs() {
			foreach (var item in tabButtons) {
				if (selectedTabButton != null && item.Equals(selectedTabButton)) {
					continue;
				}
				item.background.sprite = tabIdle;
			}
		}
	}
}