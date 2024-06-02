using Mirror;
using UnityEngine;

namespace NeuroRehab.Utility {
	public class OnlineHelper : MonoBehaviour {
		private void OnEnable() {
			// basically 'isServer', but on MonoBehaviour, so can be used in offline scene as well
			if (NetworkManager.singleton != null && NetworkServer.active) {
				gameObject.SetActive(false);
			}
		}
	}
}
