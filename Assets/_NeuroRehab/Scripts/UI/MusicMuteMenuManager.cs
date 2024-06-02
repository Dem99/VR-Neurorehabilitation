using UnityEngine;
using UnityEngine.UI;
using NeuroRehab.Managers;


namespace NeuroRehab.UI {

	public class MusicMuteMenuManager : MonoBehaviour {
		[SerializeField] private Toggle MusicMuteToggle;
		private BackgroundMusicManager backgroundMusicManager;

		void Start() {
			backgroundMusicManager = FindObjectOfType<BackgroundMusicManager>();
			if (backgroundMusicManager != null){
				MusicMuteToggle.onValueChanged.AddListener(MusicMuteToggleHandler);
        		MusicMuteToggle.isOn = !backgroundMusicManager.isPlaying;
			}
			else {
				MusicMuteToggle.gameObject.SetActive(false);
			}
		}

        void Update() {
            MusicMuteToggle.isOn = !backgroundMusicManager.isPlaying;
        }

        private void MusicMuteToggleHandler(bool value) {
        	if (value) {
				backgroundMusicManager.StopPlayer();
        	}
        	else {
            	backgroundMusicManager.StartPlayer();
        	}
    	}
	}
}


