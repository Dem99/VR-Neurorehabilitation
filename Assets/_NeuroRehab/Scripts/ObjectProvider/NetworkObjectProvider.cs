using UnityEngine;

using NeuroRehab.Interfaces;

namespace NeuroRehab.ObjectProvider {
	public class NetworkObjectProvider : IObjectProvider {
		private readonly IObjectProvider innerProvider;

		public NetworkObjectProvider(bool isServer) {
			if (isServer) {
				innerProvider = new ServerNetworkProvider();
			} else {
				innerProvider = new ClientNetworkProvider();
			}
		}

		public GameObject GetObject(uint id) {
			return innerProvider.GetObject(id);
		}

		public bool TryGetObject(uint id, out GameObject gameObject) {
			return innerProvider.TryGetObject(id, out gameObject);
		}
	}
}