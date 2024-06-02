using Mirror;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using Unity.XR.CoreUtils;

using NeuroRehab.Core;
using NeuroRehab.ScriptObjects;
using NeuroRehab.XR;
using NeuroRehab.Interfaces;

namespace NeuroRehab.Managers.Character {
	/// <summary>
	/// Custom implementation of 'CharacterManager' for XR client. Used both in traditional and simulated XR/VR.
	/// </summary>
	public class XRCharacterManager : CharacterManager {

		//[Header("Run sync vars")]

		//[Header("Avatars prefabs used")]

		//[Header("Camera culling and objects to disable")]
		[Header("XR Character Manager")]
		[SerializeField] private CustomXRSettings xrSettings;

		[Header("Spawn sync vars")]
		[SyncVar(hook = nameof(ChangeControllerType))] private ControllerType controllerType;
		[SyncVar(hook = nameof(ChangeHMDType))] private HMDType hmdType;

		[Header("Activated objects based on 'authority'")]
		// Items is array of components that may need to be enabled / activated  only locally
		[SerializeField] private XRControllerUtility[] XRControllers;

		[SerializeField] private InputActionManager inputActionManager;

		[SerializeField] private GameObject xrDeviceSimulator;

		[SerializeField] private Transform headCollider;

		[SerializeField] private XROrigin xrOrigin;

		public HMDType HmdType { get => hmdType; private set => hmdType = value; }
		public ControllerType ControllerType { get => controllerType; private set => controllerType = value; }

		public override void OnStartLocalPlayer() {
			base.OnStartLocalPlayer();

		// Input Action manager has to be enabled only locally, otherwise it would not work, due to how VR works
		// in other words, only local "player" can have it enabled
		inputActionManager.enabled = true;
		for (int i = 0; i < avatarWalkingControllers.Length; i++) {
			avatarWalkingControllers[i].enabled = false;
		}


			// We look for Device simulator and setup the local player camera transform to camera transform
			// this is only really needed for simulating XR on desktop (Testing in editor or running app with XR active without headset)
			if (xrSettings.XrActive && HmdType == HMDType.Mock) {
				GameObject deviceSimulator = Instantiate(xrDeviceSimulator);

				deviceSimulator.GetComponent<XRDeviceSimulator>().cameraTransform = LocalCamera.transform;
			}

			foreach (XRControllerUtility XRController in XRControllers) {
				XRController.enabled = true;
			}
		}

		public override void OnStopClient() {
			base.OnStopClient();
		}

		protected override void Start() {
			base.Start();
			// Setting up offset based on HMD type used by client
			ChangeHMDType(HmdType, HmdType);

			// Setting up controller model
			ChangeControllerType(ControllerType, ControllerType);
		}

		private void LateUpdate() {
			if (!isLocalPlayer) {
				Vector3 center = xrOrigin.CameraInOriginSpacePos;
				if (headCollider) {
					center = characterController.transform.InverseTransformPoint(headCollider.TransformPoint(headCollider.GetComponent<CapsuleCollider>().center));
				}
				center.y = xrOrigin.CameraInOriginSpaceHeight / 2f + characterController.skinWidth;

				characterController.height = xrOrigin.CameraInOriginSpaceHeight;
				characterController.center = center;
			}
		}

		public void Initialize(ControllerType controllerType, HMDType hmdType) {
			ControllerType = controllerType;
			HmdType = hmdType;
		}

		public void Initialize(bool isFemale, int avatarNumber, float sizeMultiplier, float offsetDistance, UserRole userRole, string roomID, ControllerType controllerType, HMDType hmdType) {
			base.Initialize(isFemale, avatarNumber, sizeMultiplier, offsetDistance, userRole, roomID);

			Initialize(controllerType, hmdType);
		}

		/**
		*
		* ITEM PICKUP / RELEASE
		* these are the methods used for granting player an authority over items and then releasing the authority after the item is released
		* this is used for certain VR functions to work as intended in multiplayer space such as grabbing an item
		*
		*/

		protected override void ItemReleased(NetworkIdentity itemNetIdentity) {
			if (sceneObjectManager.ActivePatient != null) {
				sceneObjectManager.ActivePatient.HideArmRangeMarker();
			}

			itemNetIdentity.TryGetComponent(out ITargetDisable targetDisableInterface);
			targetDisableInterface?.CmdEnableDrag();
		}

		/// <summary>
		/// Method to change controller model
		/// </summary>
		/// <param name="_old"></param>
		/// <param name="_new"></param>
		private void ChangeControllerType(ControllerType _old, ControllerType _new) {
			// customDebug.Log("Change controller called " + _new.ToString());
			foreach (var controller in XRControllers) {
				controller.ChangeModel(_new);
			}
		}

		/// <summary>
		/// Handler method that handles HMD type changes. In case of 'Other' HMD type we reset Offset (this is in case you are using VR headset so that you are not too tall). The offset exists because simulated HMD would otherwise be on the ground.
		/// </summary>
		/// <param name="_old"></param>
		/// <param name="_new"></param>
		public void ChangeHMDType(HMDType _old, HMDType _new) {
			if (_new == HMDType.Other || _new == HMDType.Android) {
				transform.Find("Offset").position = new Vector3(0f, 0f, 0f);
			}
		}

		/// <summary>
		/// Handler method for changing active animated arm.
		/// </summary>
		/// <param name="_old"></param>
		/// <param name="_new"></param>
		public override void ChangeAnimatedArm(bool _old, bool _new) {
			base.ChangeAnimatedArm(_old, _new);

			foreach (var controller in XRControllers) {
				// We turn off controller and ray interactors, based on which arm is being used
				if (IsLeftArmAnimated == controller.IsLeftHand()) {
					controller.gameObject.SetActive(false);
				} else {
					controller.gameObject.SetActive(true);
				}
			}
		}

		public override void FixArms(bool _old, bool _new) {
			base.FixArms(_old, _new);
			foreach (var controller in XRControllers) {
				if (IsArmFixed){
					controller.gameObject.SetActive(false);
				} else if (IsLeftArmAnimated == controller.IsLeftHand()) {
					controller.gameObject.SetActive(false);
				} else {
					controller.gameObject.SetActive(true);
				}
			}
		}

		/// <summary>
		/// Teleports character to specific location and rotates it to look in the direction of targetPosition transform. LokTarget is ignored on XRCharacterManager.
		/// </summary>
		/// <param name="targetPosition"></param>
		/// <param name="lookTarget"></param>
		public override void TeleportCharacter(Transform targetPosition, Transform lookTarget = null) {
			if (targetPosition == null) {
				customDebug.LogError("Argument 'targetPosition' cannot be null");
				return;
			}
			// We have to turn off character controller, as it stops us trying to teleport object around
			characterController.enabled = false;

			Vector3 targetCameraPos = targetPosition.position;
			targetCameraPos.y += xrOrigin.CameraYOffset;
			xrOrigin.MoveCameraToWorldLocation(targetCameraPos);

			float angleToRotate = targetPosition.rotation.eulerAngles.y - base.LocalCamera.transform.rotation.eulerAngles.y;
			xrOrigin.RotateAroundCameraUsingOriginUp(angleToRotate);

			characterController.enabled = true;
		}
	}
}