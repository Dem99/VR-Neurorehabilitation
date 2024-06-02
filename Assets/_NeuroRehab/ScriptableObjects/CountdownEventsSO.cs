using System;
using UnityEngine;

namespace NeuroRehab.ScriptObjects {

	[CreateAssetMenu(menuName = "ScriptObjects/CountdownEvents")]
	public class CountdownEventsSO : ScriptableObject {
		public event Action<float, string, string, bool> OnStartCountdown;
		public event Action<string> OnPauseCountdown;
		public event Action OnStopCountdown;
		public event Action<string, bool> OnSetCountdownMessage;

		public void StartCountdown(float countdownDuration, string extraText, string prefix, bool localized) {
			OnStartCountdown?.Invoke(countdownDuration, extraText, prefix, localized);
		}

		public void PauseCountdown(string extraText) {
			OnPauseCountdown?.Invoke(extraText);
		}

		public void StopCountdown() {
			OnStopCountdown?.Invoke();
		}

		public void SetCountdownMessage(string message, bool isLocalized) {
			OnSetCountdownMessage?.Invoke(message, isLocalized);
		}
	}
}