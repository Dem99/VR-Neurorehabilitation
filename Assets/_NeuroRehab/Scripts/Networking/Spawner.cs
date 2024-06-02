using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

namespace NeuroRehab.Networking {

	public class Spawner : MonoBehaviour {
		[SerializeField, NonReorderable] private SpawnObject[] roomObjects;

		public void SpawnRoomObjects(Scene scene, string roomName) {
			if (!NetworkServer.active) return;

			SceneManager.SetActiveScene(scene);
			#if UNITY_EDITOR
			// empty object so that we can differentiate rooms easier in editor
			new GameObject($"------------- {roomName} -------------");
			#endif

			foreach (SpawnObject spawnObject in roomObjects) {
				GameObject obj = Instantiate(spawnObject.prefab, spawnObject.posRotMapping.Position, Quaternion.Euler(spawnObject.posRotMapping.Rotation));
				SceneManager.MoveGameObjectToScene(obj, scene);
				NetworkServer.Spawn(obj);
			}
		}
	}
}