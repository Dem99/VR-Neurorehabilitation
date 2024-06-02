using UnityEngine;

using NeuroRehab.Interfaces;
using NeuroRehab.Core;
using NeuroRehab.Utility;

namespace NeuroRehab.UI.Managers.Menu {
	/// <summary>
	/// Class containing handlers for UI elements - Therapist Adjustment Menu
	/// </summary>
	public class TherapistAdjustmentMenuManager : MonoBehaviour {
		private ISceneObjectManager sceneObjectManager;
		private SceneHelper sceneHelper;

		private void Awake() {
			sceneHelper = new(gameObject.scene);

			sceneObjectManager = sceneHelper.FindRootObject<ISceneObjectManager>();
		}

		public void SetArmRestHandler() {
			if (sceneObjectManager.ActivePatient == null) {
				return;
			}

			sceneObjectManager.LocalNetChar.CmdSetArmRestPosition(sceneObjectManager.ActivePatient.Id);
		}

		public void SitPatientHandler() {
			if (sceneObjectManager.ActivePatient == null) {
				return;
			}

			sceneObjectManager.LocalNetChar.CmdMovePatientToSit(sceneObjectManager.ActivePatient.Id);
		}

		public void SitAcrossTableHandler() {
			sceneObjectManager.LocalChar.TeleportCharacter(sceneObjectManager.Table.TherapistSitPosition, ((TableHelper)sceneObjectManager.Table).transform);
		}

		public void MoveTableUpHandler() {
			sceneObjectManager.LocalNetChar.CmdMoveTable(new Vector3(0f, 0.02f, 0f));
		}

		public void MoveTableDownHandler() {
			sceneObjectManager.LocalNetChar.CmdMoveTable(new Vector3(0f, -0.02f, 0f));
		}

		public void MovePatientForwardHandler() {
			if (sceneObjectManager.ActivePatient == null) {
				return;
			}

			sceneObjectManager.LocalNetChar.CmdMovePatient(new Vector3(0f, 0f, 0.02f), sceneObjectManager.ActivePatient.Id);
		}

		public void MovePatientBackwardsHandler() {
			if (sceneObjectManager.ActivePatient == null) {
				return;
			}

			sceneObjectManager.LocalNetChar.CmdMovePatient(new Vector3(0f, 0f, -0.02f), sceneObjectManager.ActivePatient.Id);
		}

		public void MovePatientRightHandler() {
			if (sceneObjectManager.ActivePatient == null) {
				return;
			}

			sceneObjectManager.LocalNetChar.CmdMovePatient(new Vector3(0.02f, 0f, 0f), sceneObjectManager.ActivePatient.Id);
		}

		public void MovePatientLeftHandler() {
			if (sceneObjectManager.ActivePatient == null) {
				return;
			}

			sceneObjectManager.LocalNetChar.CmdMovePatient(new Vector3(-0.02f, 0f, 0f), sceneObjectManager.ActivePatient.Id);
		}

		public void MoveArmRestForwardHandler() {
			sceneObjectManager.LocalNetChar.CmdMoveArmRest(new Vector3(0f, 0f, 0.02f));
		}

		public void MoveArmRestBackwardsHandler() {
			sceneObjectManager.LocalNetChar.CmdMoveArmRest(new Vector3(0f, 0f, -0.02f));
		}

		public void MoveArmRestRightHandler() {
			sceneObjectManager.LocalNetChar.CmdMoveArmRest(new Vector3(0.02f, 0f, 0f));
		}

		public void MoveArmRestLeftHandler() {
			sceneObjectManager.LocalNetChar.CmdMoveArmRest(new Vector3(-0.02f, 0f, 0f));
		}

		public void SetPatientAnimatedArm(bool isLeft) {
			if (sceneObjectManager.ActivePatient == null) {
				return;
			}

			sceneObjectManager.LocalNetChar.CmdSetActiveArm(isLeft, sceneObjectManager.ActivePatient.Id);
		}

		public void FixPatientArms(bool isFix) {
			if (sceneObjectManager.ActivePatient == null) {
				return;
			}

			sceneObjectManager.LocalNetChar.CmdFixArms(isFix, sceneObjectManager.ActivePatient.Id);
		}
	}
}