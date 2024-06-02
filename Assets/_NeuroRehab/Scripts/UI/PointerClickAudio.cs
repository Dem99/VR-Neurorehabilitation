using UnityEngine;
using UnityEngine.EventSystems;

using NeuroRehab.ScriptObjects;
using NeuroRehab.Core;

namespace NeuroRehab.UI {
	/// <summary>
	/// Class that can be used to add Click Audio to elements when interacting with them.
	/// </summary>
	public class PointerClickAudio : MonoBehaviour, IPointerClickHandler {
		[SerializeField] private AudioHelperSO audioHelper;

		private readonly float buttonAudioRate = 0.25f;
		private float audioPlayed = 0f;

		public void OnPointerClick(PointerEventData eventData) {
			PlaySound();
		}

		/// <summary>
		/// We use reference to audio source object.
		/// </summary>
		private void PlaySound() {
			if (Time.time < audioPlayed + buttonAudioRate) {
				return;
			}
			audioHelper.PlaySound(AudioClipType.UI_INTERACTION);
			audioPlayed = Time.time;
		}
	}
}