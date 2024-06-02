using System.Collections;
using UnityEngine;

namespace NeuroRehab.UI {
	public class Fadeable : MonoBehaviour {
		public CanvasGroup CanvasGroup {get; private set;}
		private readonly float defaultFadeDuration = 0.5f;

		private Coroutine fadeCoroutine;

		private void Start() {
			CanvasGroup = gameObject.GetComponent<CanvasGroup>();
			CanvasGroup.alpha = 0f;
		}

		public void Fade(float startLerpValue, float endLerpValue, float lerpDuration) {
			if (fadeCoroutine != null) {
				StopCoroutine(fadeCoroutine);
			}

			fadeCoroutine = StartCoroutine(FadeAlpha(startLerpValue, endLerpValue, lerpDuration));
		}

		private IEnumerator FadeAlpha(float startLerpValue, float endLerpValue, float lerpDuration) {
			float lerpTimeElapsed = 0f;

			while (lerpTimeElapsed < lerpDuration) {
				float t = lerpTimeElapsed / lerpDuration;
				CanvasGroup.alpha = Mathf.Lerp(startLerpValue, endLerpValue, t);
				lerpTimeElapsed += Time.deltaTime;
				// Debug.Log($"{CanvasGroup.alpha} __ {gameObject.name}");
				yield return null;
			}
			// lerp never reaches endValue, that is why we have to set it manually
			CanvasGroup.alpha = endLerpValue;
		}

		public IEnumerator FadeIn(float _fadeDuration) {
			if (fadeCoroutine != null) {
				StopCoroutine(fadeCoroutine);
			}

			yield return StartCoroutine(FadeAlpha(0f, 1f, _fadeDuration));
		}

		public IEnumerator FadeOut(float _fadeDuration) {
			if (fadeCoroutine != null) {
				StopCoroutine(fadeCoroutine);
			}

			yield return StartCoroutine(FadeAlpha(1f, 0f, _fadeDuration));
		}

		public IEnumerator FadeIn() {
			if (fadeCoroutine != null) {
				StopCoroutine(fadeCoroutine);
			}

			yield return StartCoroutine(FadeAlpha(0f, 1f, defaultFadeDuration));
		}

		public IEnumerator FadeOut() {
			if (fadeCoroutine != null) {
				StopCoroutine(fadeCoroutine);
			}

			yield return StartCoroutine(FadeAlpha(1f, 0f, defaultFadeDuration));
		}
	}
}