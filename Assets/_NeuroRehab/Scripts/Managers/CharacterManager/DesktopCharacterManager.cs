using UnityEngine;

namespace NeuroRehab.Managers.Character {
	/// <summary>
	/// Custom implementation of 'CharacterManager' for desktop client.
	/// </summary>
	public class DesktopCharacterManager : CharacterManager {


		//[Header("Desktop Character Manager")]
		//[Header("Spawn sync vars")]

		//[Header("Run sync vars")]

		//[Header("Avatars prefabs used")]

		//[Header("Activated objects based on 'authority'")]

		//[Header("Camera culling and objects to disable")]

		public override void OnStartLocalPlayer() {
			base.OnStartLocalPlayer();
		}

		public override void OnStopClient() {
			base.OnStopClient();
		}

		/// <summary>
		/// Teleports character to specific location and rotates it to look at target
		/// </summary>
		/// <param name="targetPosition"></param>
		/// <param name="lookTarget"></param>
		public override void TeleportCharacter(Transform targetPosition, Transform lookTarget = null) {
			if (targetPosition == null || lookTarget == null) {
				customDebug.LogError("Arguments 'targetPosition' and 'lookTarget' cannot be null");
				return;
			}
			// We have to turn off character controller, as it stops us trying to teleport object around
			characterController.enabled = false;

			transform.position = targetPosition.position;
			transform.LookAt(lookTarget);

			characterController.enabled = true;
		}
	}
}