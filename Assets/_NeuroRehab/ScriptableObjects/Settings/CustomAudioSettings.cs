using System;
using UnityEngine;

namespace NeuroRehab.ScriptObjects {

	[CreateAssetMenu(menuName = "ScriptObjects/Settings/Audio")]
	public class CustomAudioSettings : ScriptableObject {
		[SerializeField] private float m_UIVolume = 1f;
		[SerializeField] private float m_MasterVolume = 1f;
		[SerializeField] private float m_MusicVolume = 1f;

		public event Action OnUIVolumeChanged;
		public event Action OnMasterVolumeChanged;
		public event Action OnMusicVolumeChanged;

		public void Init(float uiVol, float masterVol, float musicVol) {
			UIVolume = uiVol;
			MasterVolume = masterVol;
			MusicVolume = musicVol;

		}

		public float UIVolume {
			get => m_UIVolume;
			set {
				m_UIVolume = value;

				OnUIVolumeChanged?.Invoke();
			}
		}

		public float MasterVolume {
			get => m_MasterVolume;
			set {
				m_MasterVolume = value;

				OnMasterVolumeChanged?.Invoke();
			}
		}

		public float MusicVolume {
			get => m_MusicVolume;
			set {
				m_MusicVolume = value;

				OnMusicVolumeChanged?.Invoke();
			}
		}
	}
}