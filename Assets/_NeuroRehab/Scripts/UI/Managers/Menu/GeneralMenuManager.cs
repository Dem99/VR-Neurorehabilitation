using UnityEngine;

namespace NeuroRehab.UI.Managers.Menu {
	public class GeneralMenuManager : MonoBehaviour {
		public void QuitApp() {
			Application.Quit();
			#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
			#endif
		}

		public void TriggerActive(GameObject _object) {
			_object.SetActive(!_object.activeSelf);
		}
	}
}