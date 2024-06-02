using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Vivox;
using NeuroRehab.ScriptObjects;
using NeuroRehab.Managers;
using UnityEngine.SceneManagement;

namespace NeuroRehab.UI.Managers.Menu.Settings {

	public class AudioSettingsMenuManager : MonoBehaviour {
		[Header("Scriptable objects")]
		[SerializeField] private CustomAudioSettings audioSettings;

		[Header("Dependencies")]
		[Header("Audio Sliders")]
		[SerializeField] private TMP_Text UIAudioVolumeTextValue;
		[SerializeField] private Slider UIAudioVolumeSlider;

		[SerializeField] private TMP_Text MasterAudioVolumeTextValue;
		[SerializeField] private Slider MasterAudioVolumeSlider;

		[SerializeField] private Toggle VoiceChatMuteToggle;

		[SerializeField] private TMP_Text MusicAudioVolumeTextValue;
    	[SerializeField] private Slider MusicAudioVolumeSlider;

		[SerializeField] private Toggle MusicMuteToggle;
		private BackgroundMusicManager backgroundMusicManager;
		private static bool isVoiceMuted = false;

		private void Start() {
			UIAudioVolumeTextValue.text = $"{Mathf.Round(UIAudioVolumeSlider.value * 100)} %";
			MasterAudioVolumeTextValue.text = $"{Mathf.Round(MasterAudioVolumeSlider.value * 100)} %";
			MusicAudioVolumeTextValue.text = $"{Mathf.Round(MusicAudioVolumeSlider.value * 100)} %";

			UIAudioVolumeSlider.onValueChanged.AddListener(UIAudioVolumeSliderHandler);
			UIAudioVolumeSlider.value = audioSettings.UIVolume;

			MasterAudioVolumeSlider.onValueChanged.AddListener(MasterAudioVolumeSliderHandler);
			MasterAudioVolumeSlider.value = audioSettings.MasterVolume;

			MusicAudioVolumeSlider.onValueChanged.AddListener(MusicAudioVolumeSliderHandler);
			MusicAudioVolumeSlider.value = audioSettings.MusicVolume;
			if (SceneManager.GetActiveScene().name == "MainScene"){
				VoiceChatMuteToggle.onValueChanged.AddListener(VoiceMuteToggleHandler);
				VoiceChatMuteToggle.isOn = isVoiceMuted;
			}
			else {
				VoiceChatMuteToggle.gameObject.SetActive(false);
			}
			

 			backgroundMusicManager = FindObjectOfType<BackgroundMusicManager>();
			if (backgroundMusicManager != null){
				MusicMuteToggle.onValueChanged.AddListener(MusicMuteToggleHandler);
        		MusicMuteToggle.isOn = !backgroundMusicManager.isPlaying;
			}
			else {
				MusicMuteToggle.gameObject.SetActive(false);
			}
			
		}

		private void Update(){
			if (backgroundMusicManager != null){
        		MusicMuteToggle.isOn = !backgroundMusicManager.isPlaying;
			}
		}

		 private void MusicMuteToggleHandler(bool value) {
        	if (value) {
				backgroundMusicManager.StopPlayer();
        	}
        	else {
            	backgroundMusicManager.StartPlayer();
        	}
    	}

		private void VoiceMuteToggleHandler(bool value) {
        	if (value) {
            	VivoxService.Instance.MuteInputDevice();
        		VivoxService.Instance.MuteOutputDevice();
				isVoiceMuted = true;
        	}
        	else {
            	VivoxService.Instance.UnmuteInputDevice();
        		VivoxService.Instance.UnmuteOutputDevice();
				isVoiceMuted = false;
        	}
    	}

		private void UIAudioVolumeSliderHandler(float value) {
			UIAudioVolumeTextValue.text = $"{Mathf.Round(value * 100)} %";

			audioSettings.UIVolume = value;
		}

		private void MasterAudioVolumeSliderHandler(float value) {
			MasterAudioVolumeTextValue.text = $"{Mathf.Round(value * 100)} %";

			audioSettings.MasterVolume = value;
		}

		private void MusicAudioVolumeSliderHandler(float value) {
			MusicAudioVolumeTextValue.text = $"{Mathf.Round(value * 100)} %";

			audioSettings.MusicVolume = value;
		}
	}

}