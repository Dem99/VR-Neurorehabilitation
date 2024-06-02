using UnityEngine;
using Mirror;

namespace NeuroRehab.Networking {
	public class CustomPhysicsSimulator : MonoBehaviour {
		public PhysicsScene physicsScene;

		bool simulatePhysicsScene;

		void Awake()
		{
			if (NetworkServer.active) {
				physicsScene = gameObject.scene.GetPhysicsScene();
				simulatePhysicsScene = physicsScene.IsValid() && physicsScene != Physics.defaultPhysicsScene;
			} else {
				enabled = false;
			}
		}

		void FixedUpdate() {
			if (!NetworkServer.active) return;

			if (simulatePhysicsScene)
				physicsScene.Simulate(Time.fixedDeltaTime);
		}
	}
}