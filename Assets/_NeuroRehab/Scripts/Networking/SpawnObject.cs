using UnityEngine;

using NeuroRehab.Core;

namespace NeuroRehab.Networking {

	[System.Serializable]
	public class SpawnObject {
		public PosRotMapping posRotMapping;

		public GameObject prefab;
	}
}