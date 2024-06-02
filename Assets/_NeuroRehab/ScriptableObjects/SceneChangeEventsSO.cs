using System;
using UnityEngine;

namespace NeuroRehab.ScriptObjects {

	[CreateAssetMenu(menuName = "ScriptObjects/SceneChangeEvents")]
	public class SceneChangeEventsSO : ScriptableObject {
		[SerializeField] private float sceneChangeDuration = .5f;
		private bool isSceneChanging = false;

		public float SceneChangeDuration { get => sceneChangeDuration; }
		public bool IsSceneChanging { get => isSceneChanging; }

		public event Action OnSceneChangeStarted;
		public event Action OnSceneChangeDone;

		private void OnEnable() {
			isSceneChanging = false;
		}

		public void StartSceneChange() {
			isSceneChanging = true;
			OnSceneChangeStarted?.Invoke();
		}

		public void FinishSceneChange() {
			isSceneChanging = false;
			OnSceneChangeDone?.Invoke();
		}
	}
}