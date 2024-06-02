using System.Collections.Generic;
using UnityEngine;

namespace NeuroRehab.Managers {
	[System.Serializable]
	public class SimpleObjectListMember {
		public string name;
		public GameObject gameObject;
	}

	public class SimpleObjectListManager : MonoBehaviour {
		[NonReorderable] public List<SimpleObjectListMember> objectList = new();
	}
}