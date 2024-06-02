using System;
using UnityEngine;

namespace NeuroRehab.Core {

	/// <summary>
	/// Custom class made for Serialization, so that our Settings classes can use objects from scene etc...
	/// </summary>
	[Serializable]
	public class AudioSettingsSerialized {
		[SerializeField] private float uiVolume = 1f;
		[SerializeField] private float masterVolume = 1f;
		[SerializeField] private float musicVolume = 1f;

		public float UIVolume { get => uiVolume; }
		public float MasterVolume { get => masterVolume; }
		public float MusicVolume { get => musicVolume; }

		public AudioSettingsSerialized(float _uiVolume, float _masterVol, float _musicVol) {
			uiVolume = _uiVolume;
			masterVolume = _masterVol;
			musicVolume = _musicVol;
		}

	}
}