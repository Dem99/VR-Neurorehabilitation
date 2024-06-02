using UnityEngine;
using UnityEngine.UI;

using NeuroRehab.ScriptObjects;

namespace NeuroRehab.UI {
	public class AvatarMenuButton : MonoBehaviour {
		[Header("Scriptable objects")]
		[SerializeField] private AvatarSettings avatarSettings;

		[Header("Dependencies")]
		[SerializeField] private bool isFemale;

		[SerializeField] private int avatarNumber;

		[SerializeField] private Image activeBar;

		private void Start() {
			transform.GetComponent<Button>().onClick.AddListener(SetAvatar);

			if (activeBar) {
				activeBar.enabled = false;
			}

			if (avatarSettings.AvatarNumber == avatarNumber && avatarSettings.IsFemale == isFemale) {
				if (activeBar) {
					activeBar.enabled = true;
					SetAvatar();
				}
			}
		}

		public void SetAvatar() {
			avatarSettings.SetAvatarSettings(isFemale, avatarNumber);
		}
	}
}