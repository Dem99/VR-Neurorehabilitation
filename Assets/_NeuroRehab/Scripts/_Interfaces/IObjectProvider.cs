using UnityEngine;

namespace NeuroRehab.Interfaces {
	public interface IObjectProvider {
		public GameObject GetObject(uint id);
		public bool TryGetObject(uint id, out GameObject gameObject);
	}
}