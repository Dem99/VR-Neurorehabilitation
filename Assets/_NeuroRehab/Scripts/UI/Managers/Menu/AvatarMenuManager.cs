using UnityEngine;

using NeuroRehab.ScriptObjects;

namespace NeuroRehab.UI.Managers.Menu {
	public class AvatarMenuManager : MonoBehaviour {
		[SerializeField] private AvatarSettings avatarSettings;

		public void ResetHeight() {
			avatarSettings.ResetHeight();
		}
	}
}