using Mirror;
using TMPro;
using UnityEngine;

namespace NeuroRehab.UI {
	public class PingText : MonoBehaviour {
		private TMP_Text textField;

		void Start() {
			textField = GetComponent<TMP_Text>();
		}

		void FixedUpdate() {
			textField.text = "RTT: " + Mathf.Round((float)(NetworkTime.rtt * 1000)) + "ms";
		}
	}
}