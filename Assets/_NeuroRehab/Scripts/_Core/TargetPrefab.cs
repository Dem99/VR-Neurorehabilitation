using UnityEngine;

namespace NeuroRehab.Core {
	[System.Serializable]
	public class TargetPrefab {
		[SerializeField] private GameObject prefab;

		[SerializeField] private GameObject prefabFake;

		[SerializeField] private GameObject prefabExtra;

		public GameObject PrefabExtra { get => prefabExtra; }
		public GameObject PrefabFake { get => prefabFake; }
		public GameObject Prefab { get => prefab; }
	}
}