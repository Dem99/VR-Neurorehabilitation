using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Unity.XR.CoreUtils;

using NeuroRehab.Core;
using NeuroRehab.Utility;
using NeuroRehab.ScriptObjects;
using NeuroRehab.ObjectProvider;
using NeuroRehab.Interfaces;

namespace NeuroRehab.Managers.Character {
	public class NetworkCharacterManager : NetworkBehaviour, INetworkCharacter {
		private readonly CustomDebug customDebug = new("NET_CHAR_MANAGER");
		[Header("Scriptable objects")]
		[SerializeField] private NetworkSettings networkSettings;
		[SerializeField] private CountdownEventsSO countdownEvents;
		[SerializeField] private MessageEventsSO messageEvents;
		[SerializeField] private AnimationSettingsSO animSettings;

		[Header("Dependencies")]
		private IAnimationSettingsManager animSettingsManager;
		private IAnimationServerManager animServerManager;

		// -----------------------------------------------------------------------------
		private ISceneObjectManager sceneObjectManager;
		private SceneHelper sceneHelper;

		private IObjectProvider objectProvider;

		private void Awake() {
			sceneHelper = new(gameObject.scene);
			sceneObjectManager = sceneHelper.FindRootObject<ISceneObjectManager>();
		}

		public override void OnStartLocalPlayer() {
			base.OnStartLocalPlayer();

			sceneObjectManager.LocalNetChar = this;
		}

		private void Start() {
			objectProvider = new NetworkObjectProvider(isServer);

			animSettingsManager = sceneObjectManager.AnimSettings;
			animServerManager = sceneObjectManager.AnimServer;

			if (animSettingsManager == null) {
				customDebug.LogError("'AnimationSettingsManager' not found");
				return;
			}
		}

		/*
		*
		* MESSAGING
		*
		*/

		[ClientRpc]
		public void RpcMessageClients(string message, MessageType messageType, bool localizeString, Arg[] args) {
			messageEvents.ShowMessage(new(message, messageType, localizeString, args));
		}

		[ClientRpc]
		public void RpcMessageClients(string message, MessageType messageType, bool localizeString) {
			messageEvents.ShowMessage(new(message, messageType, localizeString));
		}

		/*
		*
		* ITEM PICKUP
		*
		*/
		[Client]
		public bool RequestOwnership(GameObject obj) {
			if (!isLocalPlayer) {
				customDebug.LogWarning("Ownership of object requested by a non local character!!");
				return false;
			}
			if (!obj.TryGetComponent(out NetworkIdentity targetObjIdentity)) {
				customDebug.LogWarning($"Object '{obj}' is not NetworkObject, cannot request ownership!!");
				return false;
			}
			if (targetObjIdentity.isOwned) {
				// customDebug.LogInfo("Object already owned!");
				return true;
			}

			CmdSetItemAuthority(targetObjIdentity);
			return true;
		}

		/// <summary>
		/// Command wrapper to change item authority
		/// </summary>
		/// <param name="item"></param>
		/// <param name="sender"></param>
		[Command]
		public void CmdSetItemAuthority(NetworkIdentity item, NetworkConnectionToClient sender = null) {
			SetItemAuthority(item, sender);
		}

		[Server]
		private void SetItemAuthority(NetworkIdentity item, NetworkConnectionToClient sender = null) {
			// No need to re-assign authority
			if (sender.owned.Contains(item)) {
				return;
			}
			item.gameObject.GetComponent<NetworkTransform>().syncDirection = SyncDirection.ClientToServer;
			customDebug.Log($"Granting authority: OBJECT '{item.netId}' to: USER '{sender.identity.netId}'");
			item.RemoveClientAuthority();
			item.AssignClientAuthority(sender);

			// force reset snapshots, new client does not have the correct knowledge of previous snapshots
			item.gameObject.GetComponent<NetworkTransform>().Reset();
		}
		/*
		*
		* SYNCING START / END POSITIONS
		*
		*/

		[Command]
		public void CmdSetAnimationStartPosition(bool forceNew, PosRotMapping mapping, NetworkConnectionToClient sender = null) {
			animSettingsManager.SetAnimationStartPosition(forceNew, mapping, sender);
		}

		/// <summary>
		/// Adds new move position at the end of synclist. Currently server assigns the value. In case there is an issue with correct position (server jitter), we could send the value over network from client.
		/// </summary>
		[Command]
		public void CmdAddMovePosition(NetworkConnectionToClient sender = null) {
			if (animSettingsManager.GetCurrentAnimSetup().Count >= 10) {
				return;
			}

			PosRotMapping movePosRotMapping = animSettingsManager.GetPosRotFromCurrentObject();
			if (movePosRotMapping == null) {
				return;
			}
			if (!animSettingsManager.IsTargetInBounds(movePosRotMapping, sender)) {
				customDebug.LogWarning("Cannot set target position - 'target object' not in bounds");
				return;
			}
			animSettingsManager.GetCurrentAnimSetup().Add(movePosRotMapping);
		}

		/// <summary>
		/// Command wrapper for setting lock position
		/// </summary>
		/// <param name="lockTargetPosRot"></param>
		[Command]
		public void CmdSetLockPosition(PosRotMapping lockTargetPosRot, uint lockId, NetworkConnectionToClient sender = null) {
			animSettingsManager.SetLockPosition(lockTargetPosRot, lockId, sender);
		}


		[Command]
		public void CmdClearMovePositions() {
			animSettingsManager.GetCurrentAnimSetup().Clear();
		}

		/// <summary>
		/// Deletes last move position
		/// </summary>
		[Command]
		public void CmdDeleteMovePosition() {
			int lastIndex = animSettingsManager.GetCurrentAnimSetup().Count - 1;
			if (lastIndex < 0 || !(animSettingsManager.GetCurrentAnimSetup().Count > lastIndex)) {
				return;
			}
			animSettingsManager.GetCurrentAnimSetup().RemoveAt(animSettingsManager.GetCurrentAnimSetup().Count - 1);
		}

		/**
		*
		* TARGET OBJECT SPAWNING
		*
		*/

		/// <summary>
		/// Command wrapper to spawn correct target object
		/// </summary>
		/// <param name="_oldAnimType"></param>
		/// <param name="_newAnimType"></param>
		[Command]
		public void CmdSpawnCorrectTarget(AnimationType _oldAnimType, AnimationType _newAnimType, NetworkConnectionToClient sender = null) {
			if (_newAnimType == _oldAnimType) {
				customDebug.Log($"Animation types equal '{_newAnimType}'. Cancelling spawn!");
				return;
			}
			string roomID = sender.identity.GetComponent<ICharacter>().RoomID;

			animSettingsManager.SpawnTarget(_oldAnimType, _newAnimType, roomID);
		}

		/**
		*
		* CALLING ANIMATIONS ON CLIENTS
		* server calls starts animation on every client separately, clients decide what to do with animation
		*
		*/

		[Command]
		public void CmdStartAnimationShowcase() {
			animServerManager.RpcStartActualAnimation(true, "", false);
		}

		[Command]
		public void CmdStartSingleAnimation() {
			animServerManager.RpcStartActualAnimation(false, "", false);
		}

		[Command]
		public void CmdStartTraining(TrainingType trainingType, ExerciseState startState, NetworkConnectionToClient sender = null) {
			animServerManager.StartTraining(this, trainingType, startState);
		}

		[Command]
		public void CmdStopTraining() {
			animServerManager.StopTraining();
		}

		[Command]
		public void CmdCancelTraining() {
			animServerManager.CancelTraining();
		}

		[Command]
		public void CmdProgressAnimationStep(ExerciseState state, NetworkConnectionToClient sender = null) {
			animServerManager.ProgressStep(this, state);
		}


		/// <summary>
		/// Alternates arm resting state SyncVar, which calls hook on clients and causes other events
		/// </summary>
		/// <param name="patientId"></param>
		[Command]
		public void CmdSetArmRestPosition(uint patientId) {
			if (objectProvider.TryGetObject(patientId, out GameObject patient)) {
				IArmRestingManager patientChar = patient.GetComponent<IArmRestingManager>();
				patientChar.IsArmResting = !patientChar.IsArmResting;

			}
		}

		[Command]
		public void CmdSetAnimationState(ArmAnimationState animationState, NetworkConnectionToClient sender = null) {
			sender.identity.GetComponent<ICharacter>().SetAnimState(animationState);
		}

		/*
		*
		* Setting up positioning
		*
		*/

		/// <summary>
		/// Moves patient into correct patient position specified by helper object
		/// </summary>
		/// <param name="patientId"></param> <summary>
		[Command]
		public void CmdMovePatientToSit(uint patientId) {
			if (objectProvider.TryGetObject(patientId, out GameObject patientObject)) {
				if (patientObject.TryGetComponent(out NetworkIdentity patientIdentity)) {
					TargetMovePatientToSit(patientIdentity.connectionToClient);
				}
			}
		}

		/// <summary>
		/// Method to teleport patient behind table AND rotate them to look in the direction 'PatientSitPositionObject' is rotated
		/// </summary>
		/// <param name="connection"></param>
		[TargetRpc]
		public void TargetMovePatientToSit(NetworkConnection connection) {
			// Table here is mostly ignored since we don't expect this method to be called on Patient in DesktopCharacterManager
			// since this is TargetRpc, we HAVE to use sceneObjectManager.LocalCharacter
			sceneObjectManager.LocalChar.TeleportCharacter(sceneObjectManager.Table.PatientSitPosition, ((TableHelper)sceneObjectManager.Table).transform);
		}

		/// <summary>
		/// Method to move table up or down. On top of moving table we also have to change ALL markers and target object as well
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="sender"></param>
		[Command]
		public void CmdMoveTable(Vector3 offset, NetworkConnectionToClient sender = null) {
			Transform tableObj = ((TableHelper)sceneObjectManager.Table).transform;
			if ((tableObj.transform.position.y + offset.y) > -0.5f || (tableObj.transform.position.y + offset.y) <= 0.8f) {
				tableObj.transform.position += offset;
			}

			// we have to completely change object holding positions, otherwise it won't be synced to clients
			// Refer to https://mirror-networking.gitbook.io/docs/manual/guides/synchronization/synclists for more details
			List<SyncList<PosRotMapping>> allSetups = animSettingsManager.GetAllAnimationSetups();

			int mappingCount;
			foreach (SyncList<PosRotMapping> setup in allSetups) {
				mappingCount = setup.Count;
				for (int i = 0; i < mappingCount; i++) {
					setup[i] = new PosRotMapping(setup[i].Position + offset, setup[i].Rotation);
				}
			}

			if (objectProvider.GetObject(animSettingsManager.Net_TargetId).TryGetComponent(out NetworkIdentity objNetId)) {
				SetItemAuthority(objNetId, sender);
			}

			if (animSettingsManager.Net_AnimType == AnimationType.Key) {
				foreach (var lockNetId in animSettingsManager.Net_TargetLockIds) {
					if (objectProvider.GetObject(lockNetId).TryGetComponent(out NetworkIdentity lockNetIdentity)) {
						SetItemAuthority(lockNetIdentity, sender);
					}
				}
			}

			// To keep sync direction consistent, we move target objects on caller Client
			TargetMoveObjects(sender, offset);
		}

		/// <summary>
		/// Moves rest position objects by offset
		/// </summary>
		/// <param name="offset"></param>
		[Command]
		public void CmdMoveArmRest(Vector3 offset) {
			sceneObjectManager.Table.RightArmRestHelper.position += offset;

			sceneObjectManager.Table.LeftArmRestHelper.position += offset;
		}

		/// <summary>
		/// Moving target object(s) on client
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="offset"></param>
		[TargetRpc]
		public void TargetMoveObjects(NetworkConnection connection, Vector3 offset) {
			objectProvider.GetObject(animSettingsManager.Net_TargetId).transform.position += offset;

			if (animSettingsManager.Net_AnimType == AnimationType.Key) {
				foreach (var lockNetId in animSettingsManager.Net_TargetLockIds) {
					objectProvider.GetObject(lockNetId).transform.position += offset;
				}
			}
		}

		/// <summary>
		/// Method to move patient object by offset
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="patientId"></param>
		[Command]
		public void CmdMovePatient(Vector3 offset, uint patientId) {
			if (objectProvider.TryGetObject(patientId, out GameObject patientObject)) {
				if (patientObject.TryGetComponent(out NetworkIdentity patientIdentity)) {
					TargetMovePatient(patientIdentity.connectionToClient, offset);
				}
			}
		}

		/// <summary>
		/// Method to move patient camera. Moved patient based on direction using offset
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="offset"></param>
		[TargetRpc]
		protected void TargetMovePatient(NetworkConnection connection, Vector3 offset) {
			// since this is TargetRpc, we HAVE to use sceneObjectManager.LocalCharacter
			Vector3 sidewayMovement = sceneObjectManager.LocalChar.LocalCamera.transform.right * offset.x;
			Vector3 forwardMovement = sceneObjectManager.LocalChar.LocalCamera.transform.forward * offset.z;
			Vector3 movement = sidewayMovement + forwardMovement;
			movement.y = 0;

			GameObject localCharObj = objectProvider.GetObject(sceneObjectManager.LocalChar.Id);
			if (localCharObj.TryGetComponent(out XROrigin xrOrigin)) {
				xrOrigin.MoveCameraToWorldLocation(sceneObjectManager.LocalChar.LocalCamera.transform.position + movement);
			} else {
				localCharObj.transform.position += movement;
			}
		}
	
		[Command]
		public void CmdFixArms(bool isFix, uint patientId, NetworkConnectionToClient sender = null) {
			if (!objectProvider.TryGetObject(patientId, out GameObject patient)) {
				customDebug.LogError($"Failed to find patient object with id: {patientId}");
			}
			patient.TryGetComponent(out ICharacter patientCharManager);
			if (patientCharManager == null) {
				customDebug.LogError($"Failed to retrieve CharacterManager from patient object with netId: {patientId}");
				return;
			}
			patientCharManager.FixArms(false, isFix);
		}
		
		/*
		*
		* Setting active arm
		*
		*/

		/// <summary>
		/// Changes patients active arm. Also changes current objects rotation if necessary
		/// </summary>
		/// <param name="isLeftAnimated"></param>
		/// <param name="patientId"></param>
		/// <param name="sender"></param>
		[Command]
		public void CmdSetActiveArm(bool isLeftAnimated, uint patientId, NetworkConnectionToClient sender = null) {
			if (!objectProvider.TryGetObject(patientId, out GameObject patient)) {
				customDebug.LogError($"Failed to find patient object with id: {patientId}");
			}
			patient.TryGetComponent(out ICharacter patientCharManager);
			if (patientCharManager == null) {
				customDebug.LogError($"Failed to retrieve CharacterManager from patient object with netId: {patientId}");
				return;
			}
			if (patientCharManager.IsLeftArmAnimated == isLeftAnimated) {
				return;
			}

			patientCharManager.ChangeAnimatedArm(false, isLeftAnimated);

			// We continue with script only in 'Key' animation type, only here we need to rotate target object
			if (animSettingsManager.Net_AnimType != AnimationType.Key) {
				return;
			}
			if (!objectProvider.TryGetObject(animSettingsManager.Net_TargetId, out GameObject targetObject)) {
				return;
			}

			GameObject targetPrefab = animSettings.TargetPrefabs[animSettingsManager.Net_AnimType].Prefab;

			PosRotMapping newMapping = new(targetObject.transform.position, targetPrefab.transform.rotation.eulerAngles);

			NetworkIdentity targetNetIdentity = targetObject.GetComponent<NetworkIdentity>();
			NetworkIdentity patientNetIdentity = patient.GetComponent<NetworkIdentity>();

			SetItemAuthority(targetNetIdentity, patientNetIdentity.connectionToClient);
			TargetTransformObject(patientNetIdentity.connectionToClient, newMapping, targetNetIdentity, isLeftAnimated);
		}

		/// <summary>
		/// Transforms object specified by NetworkIdentity. Moves it to specified position and rotation. Can Mirror object if specified by attribute 'Mirror Object'.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="mapping"></param>
		/// <param name="itemNetId"></param>
		/// <param name="mirrorObject"></param>
		[TargetRpc]
		protected void TargetTransformObject(NetworkConnection connection, PosRotMapping mapping, NetworkIdentity itemNetId, bool mirrorObject) {
			if (!itemNetId.isOwned) {
				CmdSetItemAuthority(itemNetId);
			}

			itemNetId.transform.SetPositionAndRotation(mapping.Position, Quaternion.Euler(mapping.Rotation));

			if (mirrorObject) {
				if (itemNetId.TryGetComponent(out ObjectMirrorable objectMirrorable)) {
					objectMirrorable.MirrorObject();
				} else {
					customDebug.LogError("This object can't be mirrored. Does not contain component 'ObjectMirrorable'");
				}
			}
			CmdSetAnimationStartPosition(true, new PosRotMapping(itemNetId.transform));
		}
	}
}