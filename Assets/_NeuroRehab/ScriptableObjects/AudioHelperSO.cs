using System;
using UnityEngine;
using AYellowpaper.SerializedCollections;

using NeuroRehab.Core;

namespace NeuroRehab.ScriptObjects {

	[CreateAssetMenu(menuName = "ScriptObjects/AudioHelper")]
	public class AudioHelperSO : ScriptableObject {
		[SerializeField, NonReorderable] private SerializedDictionary<AudioClipType, AudioClip> audioClips;

		public SerializedDictionary<AudioClipType, AudioClip> AudioClips { get => audioClips; set => audioClips = value; }

		public event Action<AudioClip> OnPlaySound;

		public void PlaySound(AudioClipType clipType) {
			OnPlaySound?.Invoke(audioClips[clipType]);
		}

		public void PlaySound(AudioClip clip) {
			OnPlaySound?.Invoke(clip);
		}
	}
}