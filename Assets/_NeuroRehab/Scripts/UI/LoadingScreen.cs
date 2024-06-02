using UnityEngine;

using NeuroRehab.ScriptObjects;
using NeuroRehab.Utility;

namespace NeuroRehab.UI {
	public class LoadingScreen : MonoBehaviour {
		[SerializeField] private SceneChangeEventsSO sceneChangeEvents;

		[SerializeField] private Canvas loadingScreenCanvas;
		[SerializeField] private FadeInOut vrCameraFade;

		private void Awake() {
			if (vrCameraFade != null) {
				vrCameraFade.FadeDuration = sceneChangeEvents.SceneChangeDuration - 0.05f;
			}
		}

		private void OnEnable() {
			sceneChangeEvents.OnSceneChangeStarted += SceneChangeStarted;
			sceneChangeEvents.OnSceneChangeDone += SceneChangeDone;
		}

		private void OnDisable() {
			sceneChangeEvents.OnSceneChangeStarted -= SceneChangeStarted;
			sceneChangeEvents.OnSceneChangeDone -= SceneChangeDone;
		}

		private void SceneChangeStarted() {
			loadingScreenCanvas.enabled = true;
			if (vrCameraFade != null) {
				vrCameraFade.FadeOut(false);
			}
		}

		private void SceneChangeDone() {
			if (vrCameraFade != null) {
				vrCameraFade.FadeIn(false, true);
			}
		}
	}
}