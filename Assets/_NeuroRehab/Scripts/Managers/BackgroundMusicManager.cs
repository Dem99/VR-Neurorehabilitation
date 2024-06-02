using System.Collections;
using UnityEngine;

namespace NeuroRehab.Managers {

    /// <summary>
	/// Used to turn on/off the music supplement 
    /// </summary>
    public class BackgroundMusicManager : MonoBehaviour {
        [SerializeField] private AudioClip[] musicList;
        [SerializeField] private AudioSource musicSource;
        private int currentTrackIndex;
        [SerializeField] public bool isPlaying;

        private void Awake() {
            currentTrackIndex = 0;
            isPlaying = false;
        }

        public void StartPlayer()
        {
            if (!isPlaying)
            {
                isPlaying = true;
                StartCoroutine(PlayTracks());
            }
        }

        public void StopPlayer()
        {
            if (isPlaying)
            {
                isPlaying = false;
                StopCoroutine(PlayTracks());
                musicSource.Stop();
            }
        }

        private IEnumerator PlayTracks()
        {
            while (isPlaying)
            {
                musicSource.clip = musicList[currentTrackIndex];
                musicSource.Play();

                while (musicSource.isPlaying)
                {
                    yield return null;
                }

                currentTrackIndex = (currentTrackIndex + 1) % musicList.Length;
            }
        }
    }
}
