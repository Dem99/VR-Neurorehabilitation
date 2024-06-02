using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Audio;

using NeuroRehab.ScriptObjects;

namespace NeuroRehab.Managers {
	public class AudioManager : MonoBehaviour {
		[Header("Scriptable objects")]
		[SerializeField] private CustomAudioSettings m_audioSettings;

		[Header("Dependencies")]
		[SerializeField] private AudioMixer mixer;

		private void OnEnable() {
			m_audioSettings.OnUIVolumeChanged += UpdateUIVolumeWrapper;
			m_audioSettings.OnMasterVolumeChanged += UpdateMasterVolumeWrapper;
			m_audioSettings.OnMusicVolumeChanged += UpdateMusicVolumeWrapper;
		}

		private void OnDisable() {
			m_audioSettings.OnUIVolumeChanged -= UpdateUIVolumeWrapper;
			m_audioSettings.OnMasterVolumeChanged -= UpdateMasterVolumeWrapper;
			m_audioSettings.OnMusicVolumeChanged -= UpdateMusicVolumeWrapper;
		}

		private IEnumerator DelayedSetAudioSettingsUpdate(Action extraAction) {
			yield return new WaitUntil(() => mixer);

			extraAction?.Invoke();
		}

		private void UpdateUIVolumeWrapper() {
			if (mixer) {
				UpdateUIVolumeMixerVal();
				return;
			}
			StartCoroutine(DelayedSetAudioSettingsUpdate(() => UpdateUIVolumeMixerVal()));
		}

		private void UpdateUIVolumeMixerVal() {
			mixer.SetFloat("UIVol", Mathf.Log10(m_audioSettings.UIVolume) * 20);
		}

		private void UpdateMasterVolumeWrapper() {
			if (mixer) {
				UpdateMasterVolumeMixerVal();
				return;
			}
			StartCoroutine(DelayedSetAudioSettingsUpdate(() => UpdateMasterVolumeMixerVal()));
		}

		private void UpdateMasterVolumeMixerVal() {
			mixer.SetFloat("MasterVol", Mathf.Log10(m_audioSettings.MasterVolume) * 20);
		}

		private void UpdateMusicVolumeWrapper() {
			if (mixer) {
				UpdateMusicVolumeMixerVal();
				return;
			}
			StartCoroutine(DelayedSetAudioSettingsUpdate(() => UpdateMusicVolumeMixerVal()));
		}

		private void UpdateMusicVolumeMixerVal() {
			mixer.SetFloat("MusicVol", Mathf.Log10(m_audioSettings.MusicVolume) * 20);
		}
	}
}