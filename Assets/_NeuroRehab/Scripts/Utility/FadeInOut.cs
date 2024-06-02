using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace NeuroRehab.Utility {
	// This class is used to fade the entire screen to black (or
	// any chosen colour).  It should be used to smooth out the
	// transition between scenes or restarting of a scene.
	public class FadeInOut : MonoBehaviour {
		public event Action OnFadeComplete;                             // This is called when the fade in or out has finished.
		[SerializeField] private List<Image> m_FadeImages;                     // Reference to the image that covers the screen.
		[SerializeField] private AudioMixerSnapshot m_DefaultSnapshot;  // Settings for the audio mixer to use normally.
		[SerializeField] private AudioMixerSnapshot m_FadedSnapshot;    // Settings for the audio mixer to use when faded out.
		[SerializeField] private float m_FadeDuration = 2.0f;           // How long it takes to fade in seconds.
		[SerializeField] private bool m_FadeInOnSceneLoad = false;      // Whether a fade in should happen as soon as the scene is loaded.
		[SerializeField] private bool m_FadeInOnStart = false;          // Whether a fade in should happen just but Updates start.


		private bool m_IsFading;                                        // Whether the screen is currently fading.
		private float m_FadeStartTime;                                  // The time when fading started.
		private List<Color> m_FadeColors = new();       // The colour the image fades out to.
		private List<Color> m_FadeOutColors = new();                                   // This is a transparent version of the fade colour, it will ensure fading looks normal.


		public bool IsFading { get { return m_IsFading; } }

		public float FadeDuration { get => m_FadeDuration; set => m_FadeDuration = value; }

		private void Awake() {
			SceneManager.sceneLoaded += HandleSceneLoaded;
			foreach (Image image in m_FadeImages) {
				m_FadeColors.Add(image.color);
				m_FadeOutColors.Add(new Color(image.color.r, image.color.g, image.color.b, 0f));
			}

			foreach (var m_FadeImage in m_FadeImages) {
				m_FadeImage.enabled = true;
			}
		}

		void OnDestroy() {
			SceneManager.sceneLoaded -= HandleSceneLoaded;
		}

		private void Start() {
			// If applicable set the immediate colour to be faded out and then fade in.
			if (m_FadeInOnStart) {
				if (gameObject.TryGetComponent(out Canvas canvas)) {
					canvas.enabled = true;
				}
				for (int i = 0; i < m_FadeImages.Count; i++) {
					m_FadeImages[i].color = m_FadeColors[i];
				}
				FadeIn(true, true);
			}
		}


		private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode) {
			// If applicable set the immediate colour to be faded out and then fade in.
			if (m_FadeInOnSceneLoad) {
				for (int i = 0; i < m_FadeImages.Count; i++) {
					m_FadeImages[i].color = m_FadeColors[i];
				}
				FadeIn(true, true);
			}
		}


		// Since no duration is specified with this overload use the default duration.
		public void FadeOut(bool fadeAudio) {
			FadeOut(m_FadeDuration, fadeAudio);
		}


		public void FadeOut(float duration, bool fadeAudio) {
			// If not already fading start a coroutine to fade from the fade out colour to the fade colour.
			if (m_IsFading || !gameObject.activeSelf)
				return;
			StartCoroutine(BeginFade(m_FadeOutColors, m_FadeColors, duration));

			// Fade out the audio over the same duration.
			if (m_FadedSnapshot && fadeAudio)
				m_FadedSnapshot.TransitionTo(duration);
		}


		// Since no duration is specified with this overload use the default duration.
		public void FadeIn(bool fadeAudio, bool disableCanvas) {
			FadeIn(m_FadeDuration, fadeAudio, disableCanvas);
		}


		public void FadeIn(float duration, bool fadeAudio, bool disableCanvas) {
			// If not already fading start a coroutine to fade from the fade colour to the fade out colour.
			if (m_IsFading || !gameObject.activeSelf)
				return;
			StartCoroutine(BeginFade(m_FadeColors, m_FadeOutColors, duration, disableCanvas));

			// Fade in the audio over the same duration.
			if (m_DefaultSnapshot && fadeAudio)
				m_DefaultSnapshot.TransitionTo(duration);
		}


		public IEnumerator BeginFadeOut(bool fadeAudio) {
			// Fade out the audio over the default duration.
			if (m_FadedSnapshot && fadeAudio)
				m_FadedSnapshot.TransitionTo(m_FadeDuration);

			yield return StartCoroutine(BeginFade(m_FadeOutColors, m_FadeColors, m_FadeDuration));
		}


		public IEnumerator BeginFadeOut(float duration, bool fadeAudio) {
			// Fade out the audio over the given duration.
			if (m_FadedSnapshot && fadeAudio)
				m_FadedSnapshot.TransitionTo(duration);

			yield return StartCoroutine(BeginFade(m_FadeOutColors, m_FadeColors, duration));
		}


		public IEnumerator BeginFadeIn(bool fadeAudio) {
			// Fade in the audio over the default duration.
			if (m_DefaultSnapshot && fadeAudio)
				m_DefaultSnapshot.TransitionTo(m_FadeDuration);

			yield return StartCoroutine(BeginFade(m_FadeColors, m_FadeOutColors, m_FadeDuration));
		}


		public IEnumerator BeginFadeIn(float duration, bool fadeAudio) {
			// Fade in the audio over the given duration.
			if (m_DefaultSnapshot && fadeAudio)
				m_DefaultSnapshot.TransitionTo(duration);

			yield return StartCoroutine(BeginFade(m_FadeColors, m_FadeOutColors, duration));
		}


		private IEnumerator BeginFade(List<Color> startCol, List<Color> endCol, float duration, bool disableCanvas = false) {
			// Fading is now happening.  This ensures it won't be interupted by non-coroutine calls.
			m_IsFading = true;

			// Execute this loop once per frame until the timer exceeds the duration.
			float timer = 0f;
			while (timer <= duration) {
				// Set the colour based on the normalised time.

				for (int i = 0; i < m_FadeImages.Count; i++) {
					m_FadeImages[i].color = Color.Lerp(startCol[i], endCol[i], timer / duration);
				}
				foreach (var m_FadeImage in m_FadeImages) {
				}

				// Increment the timer by the time between frames and return next frame.
				timer += Time.deltaTime;
				yield return null;
			}

			// Fading is finished so allow other fading calls again.
			m_IsFading = false;

			if (disableCanvas) {
				if (gameObject.TryGetComponent(out Canvas canvas)) {
					canvas.enabled = false;
				}
			}

			// If anything is subscribed to OnFadeComplete call it.
			if (OnFadeComplete != null)
				OnFadeComplete();
		}
	}
}