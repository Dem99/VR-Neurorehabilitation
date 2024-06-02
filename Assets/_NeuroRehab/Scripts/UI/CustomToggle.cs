using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NeuroRehab.UI {
	[RequireComponent(typeof(Toggle))]
	public class CustomToggle : MonoBehaviour {

		private Color defaultBackgroundColor;
		private Color defaultCheckmarkColor;
		[SerializeField] private Color selectedBackgroundColor;
		[SerializeField] private Color selectedCheckmarkColor;

		[SerializeField] private Image background;
		[SerializeField] private Image checkmark;
		private RectTransform checkmarkTransform;
		private Vector2 defaultHandlePos;

		private Toggle toggle;

		private readonly float lerpDuration = 0.2f;
		private Coroutine posCoroutine;
		private Coroutine colorCoroutine;

		void Awake() {
			toggle = gameObject.GetComponent<Toggle>();

			if (!checkmark || !background) {
				return;
			}

			defaultBackgroundColor = background.color;
			defaultCheckmarkColor = checkmark.color;
			checkmarkTransform = (RectTransform) checkmark.transform;
			defaultHandlePos = checkmarkTransform.anchoredPosition;

			toggle.onValueChanged.AddListener(ChangeGraphic);
			ChangeGraphic(false);
		}

		private void ChangeGraphic(bool value) {
			if (toggle.isOn) {
				if (posCoroutine != null) {
					StopCoroutine(posCoroutine);
				}
				if (colorCoroutine != null) {
					StopCoroutine(colorCoroutine);
				}
				posCoroutine = StartCoroutine(LerpPosition(-defaultHandlePos));
				colorCoroutine = StartCoroutine(LerpColor(selectedBackgroundColor, selectedCheckmarkColor));
			} else {
				if (posCoroutine != null) {
					StopCoroutine(posCoroutine);
				}
				if (colorCoroutine != null) {
					StopCoroutine(colorCoroutine);
				}
				posCoroutine = StartCoroutine(LerpPosition(defaultHandlePos));
				colorCoroutine = StartCoroutine(LerpColor(defaultBackgroundColor, defaultCheckmarkColor));
			}
		}

		private IEnumerator LerpPosition(Vector2 endValue) {
			float time = 0;
			Vector2 startValue = checkmarkTransform.anchoredPosition;

			while (time < lerpDuration) {
				checkmarkTransform.anchoredPosition = Vector2.Lerp(startValue, endValue, time / lerpDuration);
				time += Time.deltaTime;
				yield return null;
			}
			checkmarkTransform.anchoredPosition = endValue;
		}

		private IEnumerator LerpColor(Color bgColor, Color checkmarkColor) {
			float time = 0;
			Color startBgValue = background.color;
			Color startCheckmarkValue = checkmark.color;

			while (time < lerpDuration) {
				background.color = Color.Lerp(startBgValue, bgColor, time / lerpDuration);
				checkmark.color = Color.Lerp(startCheckmarkValue, checkmarkColor, time / lerpDuration);
				time += Time.deltaTime;
				yield return null;
			}
			background.color = bgColor;
			checkmark.color = checkmarkColor;
		}
	}
}