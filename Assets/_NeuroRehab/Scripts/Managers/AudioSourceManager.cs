using UnityEngine;

using NeuroRehab.ScriptObjects;

namespace NeuroRehab.Managers {
	/// <summary>
	/// Audio source manager for audio such as Button clicks etc. We can use this when we want to play Mono Audio.
	/// </summary>
	public class AudioSourceManager : MonoBehaviour {
		private static bool isAppInitialized = false;

		[Header("Scriptable objects")]
		[SerializeField] private AudioHelperSO audioHelper;

		[Header("Dependencies")]
		[SerializeField] private AudioSource audioSource;

		void Awake() {
			{
				if (isAppInitialized) {
					Destroy(gameObject);
					return;
				}
				isAppInitialized = true;
				if (transform.parent == null) {
					DontDestroyOnLoad(gameObject);
				}
			}
		}
		private void OnEnable() {
			audioHelper.OnPlaySound += PlayOnce;
		}

		private void OnDisable() {
			audioHelper.OnPlaySound -= PlayOnce;
		}

		private void PlayOnce(AudioClip audioClip) {
			audioSource.PlayOneShot(audioClip);
		}
	}
}