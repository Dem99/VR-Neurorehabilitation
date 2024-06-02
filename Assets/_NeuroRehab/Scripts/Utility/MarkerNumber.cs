using UnityEngine;
using TMPro;

using NeuroRehab.ScriptObjects;

namespace NeuroRehab.Utility {
	/// <summary>
	/// Class used for markers and their numbers
	/// </summary>
	public class MarkerNumber : MonoBehaviour {
		[Header("Scriptable objects")]
		[SerializeField] private MenuHelperSO menuHelper;

		[Header("Dependencies")]
		[SerializeField] private TMP_Text textField;

		private string orderString;

		public void Init(string _orderString) {
			orderString = _orderString;
			textField.text = orderString;
		}

		private void LateUpdate() {
			if (menuHelper.RenderingCamera != null) { // Either SO or use old way
				textField.transform.rotation = Quaternion.LookRotation(textField.transform.position - menuHelper.RenderingCamera.transform.position);
			}
		}
	}
}
