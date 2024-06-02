using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeuroRehab.Core {
	public class SceneHelper {
		private readonly CustomDebug customDebug = new("SCENE_HELPER");
		private Scene m_scene;

		public SceneHelper(Scene scene) {
			m_scene = scene;
		}

		public T FindRootObject<T>() {
			foreach (GameObject obj in m_scene.GetRootGameObjects()) {
				if (obj.TryGetComponent(out T component)) {
					return component;
				}
			}
			customDebug.LogError($"Failed to find object of type '{typeof(T).FullName}' in scene '{m_scene.name}'");
			return default;
		}
	}
}