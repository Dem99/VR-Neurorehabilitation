using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;

using NeuroRehab.ScriptObjects;

namespace NeuroRehab.UI.Managers {
	public class CountdownManager : MonoBehaviour {
		[SerializeField] private CountdownEventsSO countdownEvents;

		[SerializeField] private Image countdownImage;
		[SerializeField] private TMP_Text textField;
		[SerializeField] private TMP_Text extraTextField;

		private Coroutine countdownCoroutine;
		private Fadeable fadeable;

		private void OnEnable() {
			countdownEvents.OnStartCountdown += StartCountdown;
			countdownEvents.OnPauseCountdown += StopCountdown;
			countdownEvents.OnStopCountdown += StopHideCountdown;
			countdownEvents.OnSetCountdownMessage += SetCountdownMessage;
		}

		private void OnDisable() {
			countdownEvents.OnStartCountdown -= StartCountdown;
			countdownEvents.OnPauseCountdown -= StopCountdown;
			countdownEvents.OnStopCountdown -= StopHideCountdown;
			countdownEvents.OnSetCountdownMessage -= SetCountdownMessage;
		}

		private void Start() {
			fadeable = gameObject.GetComponent<Fadeable>();

			extraTextField.text = "";
			textField.text = "";
			countdownImage.fillAmount = 0f;
		}

		private IEnumerator CountdownCoroutine(float duration, string prefix, bool localized) {
			if (localized) {
				LocalizedString locString = new("Messages", prefix);
				prefix = locString.GetLocalizedString();
			}
			if (prefix.Length > 0) {
				prefix += " ";
			}

			float timePassed = 0f;
			float lastTime = 0f;
			countdownImage.fillAmount = 1f;

			textField.text = $"{prefix}{(int) (duration - lastTime)}";
			while (timePassed < duration) {
				countdownImage.fillAmount = 1f - (timePassed / duration);
				timePassed += Time.deltaTime;

				if (timePassed - lastTime >= 1f) {
					lastTime++;
					textField.text = $"{prefix}{(int) (duration - lastTime)}";
				}

				yield return null;
			}

			textField.text = $"{prefix}0";
			countdownImage.fillAmount = 0f;
		}

		private void StartCountdown(float duration, string extraText, string prefix, bool localized) {
			if (countdownCoroutine != null) {
				StopCoroutine(countdownCoroutine);
			}

			if (!Mathf.Approximately(fadeable.CanvasGroup.alpha, 1f)) {
				fadeable.Fade(0f, 1f, 0.5f);
			}
			prefix ??= "";
			extraTextField.text = extraText.Replace(" ", "");
			countdownCoroutine = StartCoroutine(CountdownCoroutine(duration, prefix, localized));
		}

		private void StopCountdown(string extraText) {
			extraTextField.text = extraText.Replace(" ", "");
			StopCountdown();
		}

		private void StopCountdown() {
			if (countdownCoroutine != null) {
				StopCoroutine(countdownCoroutine);
			}
		}

		private void StopHideCountdown() {
			StopCountdown();

			HideCountdown();
		}

		private void HideCountdown() {
			if (Mathf.Approximately(fadeable.CanvasGroup.alpha, 0f)) {
				return;
			}
			fadeable.Fade(1f, 0f, 0.5f);
		}

		private void SetCountdownMessage(string message, bool isLocalized = false){
			if (countdownCoroutine != null) {
				StopCoroutine(countdownCoroutine);
			}

			if (!Mathf.Approximately(fadeable.CanvasGroup.alpha, 1f)) {
				fadeable.Fade(0f, 1f, 0.5f);
			}

			if (isLocalized) {
				LocalizedString locString = new("Messages", message);
				message = locString.GetLocalizedString();
			}
			extraTextField.text = "";
			textField.text = message;
			countdownImage.fillAmount = 0f;
		}
	}
}