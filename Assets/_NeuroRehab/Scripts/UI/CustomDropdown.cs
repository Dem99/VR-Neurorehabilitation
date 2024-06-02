using TMPro;

namespace NeuroRehab.UI {
	public class CustomDropdown : TMP_Dropdown {
		protected override void DoStateTransition(SelectionState state, bool instant) {
			if (state == SelectionState.Disabled && gameObject.activeInHierarchy) {
				Hide();
			}
			/*
			else if (state == SelectionState.Normal) {
			}
			*/
		}
	}
}