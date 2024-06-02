using System.Collections.Generic;
using UnityEngine;

namespace NeuroRehab.UI.Managers.Menu.Mini {
	public class MiniMenuVisibilityManager : MonoBehaviour {

		private List<MiniMenuManager> miniMenuManagers = new();
		[SerializeField] private List<bool> isMenuShowingList = new();
		private int miniMenuCount = 0;

		private void OnEnable() {
			foreach (MiniMenuManager miniMenu in miniMenuManagers) {
				miniMenu.MenuHolderCanvas.enabled = false;
			}
			for (int i = 0; i < isMenuShowingList.Count; i++) {
				isMenuShowingList[i] = false;
			}
		}

		private void OnDisable() {
			foreach (MiniMenuManager miniMenu in miniMenuManagers) {
				if (miniMenu.MenuHolderCanvas != null) {
					miniMenu.MenuHolderCanvas.enabled = false;
				}
			}
		}

		public void RegisterMiniMenuManager(MiniMenuManager _miniMenuManager) {
			miniMenuManagers.Add(_miniMenuManager);

			isMenuShowingList.Add(false);
			miniMenuCount++;
		}

		public bool IsMenuShowing() {
			for (int i = 0; i < miniMenuCount; i++) {
				if (isMenuShowingList[i]) {
					return true;
				}
			}

			return false;
		}

		// checks if IS menu showing AND was triggered by other MiniMenuManager
		public bool IsMenuShowing(MiniMenuManager _miniMenuManager) {
			int index = miniMenuManagers.IndexOf(_miniMenuManager);

			for (int i = 0; i < miniMenuCount; i++) {
				if (i == index) {
					continue;
				}
				if (isMenuShowingList[i]) {
					return true;
				}
			}

			return false;
		}

		// triggers menu for specific MenuManager
		public bool TriggerMenu(MiniMenuManager _miniMenuManager) {
			int index = miniMenuManagers.IndexOf(_miniMenuManager);

			isMenuShowingList[index] = !isMenuShowingList[index];

			return isMenuShowingList[index];
		}

		public void SetMenuStatus(MiniMenuManager _miniMenuManager, bool _value) {
			int index = miniMenuManagers.IndexOf(_miniMenuManager);

			isMenuShowingList[index] = _value;
		}

		public bool GetMenuStatus(MiniMenuManager _miniMenuManager) {
			return isMenuShowingList[miniMenuManagers.IndexOf(_miniMenuManager)];
		}
	}
}