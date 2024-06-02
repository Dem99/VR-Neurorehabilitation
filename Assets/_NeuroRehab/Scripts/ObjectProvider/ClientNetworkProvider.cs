using Mirror;
using UnityEngine;

using NeuroRehab.Interfaces;

namespace NeuroRehab.ObjectProvider {
	public class ClientNetworkProvider : IObjectProvider {
		public GameObject GetObject(uint id) {
			try {
				return NetworkClient.spawned[id] == null ? null : NetworkClient.spawned[id].gameObject;
			}
			catch (System.Collections.Generic.KeyNotFoundException) {
				return null;
			}
		}

		public bool TryGetObject(uint id, out GameObject gameObject) {
			bool retVal = false;

			try {
				retVal = NetworkClient.spawned.TryGetValue(id, out NetworkIdentity netObj);
				gameObject = !retVal ? null : netObj.gameObject;
			}
			catch (System.Collections.Generic.KeyNotFoundException) {
				gameObject = null;
			}

			return retVal;
		}
	}
}