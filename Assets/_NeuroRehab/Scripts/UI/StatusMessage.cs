using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using NeuroRehab.Core;

namespace NeuroRehab.UI {
	/// <summary>
	/// Component used for Status Message element
	/// </summary>
	public class StatusMessage : MonoBehaviour {

		[SerializeField] private TMP_Text textField;
		[Header("Backgrounds")]
		[SerializeField] private Image okBackground;
		[SerializeField] private Image warningBackground;
		[SerializeField] private Image normalBackground;

		private readonly float messageDuration = 4f;

		private Fadeable fadeable;

		void Start() {
			fadeable = gameObject.GetComponent<Fadeable>();

			HideBackgrounds();
		}

		private IEnumerator StatusMessageCoroutine(string message) {
			textField.text = message;
			if (!Mathf.Approximately(fadeable.CanvasGroup.alpha, 1f)) {
				yield return fadeable.FadeIn();
			}

			yield return new WaitForSecondsRealtime(messageDuration);
			yield return fadeable.FadeOut();
		}

		public void ShowMessage(string message, MessageType messageType) {
			StopAllCoroutines();

			HideBackgrounds();
			switch (messageType) {
				case MessageType.SUCCESS:
					okBackground.enabled = true;
					break;
				case MessageType.WARNING:
					warningBackground.enabled = true;
					break;
				case MessageType.NORMAL:
					normalBackground.enabled = true;
					break;
				default:
					break;
			}
			StartCoroutine(StatusMessageCoroutine(message));
		}

		public void HideMessage() {
			StartCoroutine(fadeable.FadeOut());
			textField.text = "";
		}

		private void HideBackgrounds() {
			okBackground.enabled = false;
			warningBackground.enabled = false;
			normalBackground.enabled = false;
		}
	}
}