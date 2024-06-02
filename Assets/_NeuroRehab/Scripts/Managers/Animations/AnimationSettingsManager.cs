using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System;
using System.Linq;
using System.Collections.Generic;

using NeuroRehab.Core;
using NeuroRehab.Utility;
using NeuroRehab.Managers.Character;
using NeuroRehab.ScriptObjects;
using NeuroRehab.ObjectProvider;
using NeuroRehab.Interfaces;

namespace NeuroRehab.Managers.Animations {

	[Serializable, RequireComponent(typeof(AnimationServerManager))]
	public class AnimationSettingsManager : NetworkBehaviour, IAnimationSettingsManager {
		private readonly CustomDebug customDebug = new("ANIM_SETTINGS");
		[Header("Scriptable objects")]
		[SerializeField] private RoleSettings roleSettings;
		[SerializeField] private AnimationSettingsSO animSettings;


		[Header("Dependencies")]

		[Header("Spawnable Prefabs")]
		/*
			we can't use SerializedDictionary for these options, there is an error with it on networked objects,
			it causes the dictionary to be "duplicated", because we spawn the same object multiple times (once for each scene);
			in other words, DO NOT USE SERIALIZED DICTIONARY ON NETWORKED OBJECTS
		*/
		[SerializeField] private List<MarkerNumber> markerPrefabs = new();

		[Space]
		[SerializeField] private float waitDuration = 0.5f;
		[SerializeField] private float keyTurnDuration = 1f;

		[SyncVar(hook = nameof(ChangeArmMoveDurationHook)), Range(0.5f, 5f)]
		private float net_armMoveDuration = 1.5f;
		[SyncVar(hook = nameof(ChangeHandMoveDurationHook)), Range(0.5f, 5f)]
		private float net_handMoveDuration = 1.5f;
		[SyncVar(hook = nameof(ChangeEvalDurationHook)), Range(0.5f, 20f)]
		private float net_evalDuration = 10f;
		[SyncVar(hook = nameof(ChangeRestDurationHook)), Range(0.5f, 20f)]
		private float net_restDuration = 5f;
		[SyncVar(hook = nameof(ChangeMoveDurationHook)), Range(0.5f, 10f)]
		private float net_moveDuration = 4f;
		[SyncVar(hook = nameof(ChangeRepetitionsHook)), Range(1, 20)]
		private int net_repetitions = 5;
		[SyncVar(hook = nameof(ChangeAnimTypeHook))]
		private AnimationType net_animType = AnimationType.Block;
		[SyncVar]
		private AnimationType net_prevAnimType;

		// Accessible on both server and client
		[SyncVar] private uint net_targetId;
		[SyncVar] private uint net_targetLockId;
		private readonly SyncList<uint> net_targetLockIds = new();

		// [Header("Move positions sync lists")]
		// https://mirror-networking.gitbook.io/docs/guides/synchronization/synclists
		private readonly SyncList<PosRotMapping> blockSetup = new();
		private readonly SyncList<PosRotMapping> cubeSetup = new();
		private readonly SyncList<PosRotMapping> cupSetup = new();
		private readonly SyncList<PosRotMapping> keySetup = new();

		// ---------------------------------------------------------------
		private IObjectProvider objectProvider;
		private SceneHelper sceneHelper;
		private ISceneObjectManager sceneObjectManager;

		private PhysicsScene physicsScene;
		private readonly float maxRaycastDistance = 10f;

		private AnimationServerManager animationServerManager;

		private readonly List<MarkerNumber> spawnedMarkers = new();
		private Transform spawnArea;

		// Accessible only on Client
		private GameObject targetFake;

		// ---------------------------------------------------------------
		public AnimationType Net_AnimType { get => net_animType; }
		public AnimationType Net_PrevAnimType { get => net_prevAnimType; }
		public float Net_armMoveDuration { get => net_armMoveDuration; }
		public float Net_handMoveDuration { get => net_handMoveDuration; }
		public float Net_evalDuration { get => net_evalDuration; }
		public float Net_restDuration { get => net_restDuration; }
		public float Net_moveDuration { get => net_moveDuration; }
		public int Net_repetitions { get => net_repetitions; }
		public uint Net_TargetId { get => net_targetId; }
		public uint Net_TargetLockId { get => net_targetLockId; }
		public SyncList<uint> Net_TargetLockIds { get => net_targetLockIds; }

		public GameObject TargetFake { get => targetFake; }
		// ---------------------------------------------------------------

		private void Awake() {
			sceneHelper = new(gameObject.scene);
			sceneObjectManager = sceneHelper.FindRootObject<ISceneObjectManager>();

			sceneObjectManager.AnimSettings = this;

			physicsScene = gameObject.scene.GetPhysicsScene();
			animationServerManager = gameObject.GetComponent<AnimationServerManager>();
		}

		public override void OnStartServer() {
			objectProvider = new NetworkObjectProvider(isServer);

			net_animType = AnimationType.Block;
			net_prevAnimType = AnimationType.Block;
			animSettings.AnimType = Net_AnimType;

			string animTypeString = Net_AnimType.ToString();

			GameObject targetObject = null;
			if (Net_TargetId == 0) {
				TargetUtility targetObjUtility = sceneHelper.FindRootObject<TargetUtility>();
				if (targetObjUtility != null) {
					targetObject = targetObjUtility.gameObject;
				}
			} else { // will prolly never happen
				targetObject = objectProvider.GetObject(Net_TargetId);
			}
			if (targetObject == null) {
				customDebug.LogError($"Failed to find object: {animTypeString}");
			} else {
				PosRotMapping objPostRot = new(targetObject.transform.position, targetObject.transform.rotation.eulerAngles);
				net_targetId = targetObject.GetComponent<NetworkIdentity>().netId;

				if (IsTargetInBounds(objPostRot)) {
					GetCurrentAnimSetup().Add(objPostRot);
				}
			}
		}

		public override void OnStartClient() {
			animSettings.AnimType = Net_AnimType;
			animSettings.ArmMoveDuration = net_armMoveDuration;
			animSettings.HandMoveDuration = net_handMoveDuration;
			animSettings.EvalDuration = net_evalDuration;
			animSettings.RestDuration = net_restDuration;
			animSettings.MoveDuration = net_moveDuration;
			animSettings.Repetitions = net_repetitions;
			animSettings.CurrentSetup = GetCurrentAnimSetupList();

			animSettings.OnChangeAnimTypeValue += ChangeAnimType;
			animSettings.OnChangeArmMoveDuration += ChangeArmMoveDuration;
			animSettings.OnChangeEvalDuration += ChangeEvalDuration;
			animSettings.OnChangeMoveDuration += ChangeMoveDuration;
			animSettings.OnChangeRepetitions += ChangeRepetitions;
			animSettings.OnChangeHandMoveDuration += ChangeHandMoveDuration;
			animSettings.OnChangeRestDuration += ChangeRestDuration;
		}

		private void Start() {
			animSettings.WaitDuration = waitDuration;
			animSettings.KeyTurnDuration = keyTurnDuration;

			spawnArea = sceneObjectManager.Table.TargetSpawnArea;

			if (isClient) {
				objectProvider = new NetworkObjectProvider(isServer);

				net_prevAnimType = AnimationType.Block;

				blockSetup.Callback += OnAnimationSetupUpdated;
				cubeSetup.Callback += OnAnimationSetupUpdated;
				cupSetup.Callback += OnAnimationSetupUpdated;
				keySetup.Callback += OnAnimationSetupUpdated;

				SetupMarkers();
				SpawnCorrectTargetFakes(AnimationType.Off, Net_AnimType);
			}
		}

		public SyncList<PosRotMapping> GetCurrentAnimSetup() {
			switch (Net_AnimType) {
				case AnimationType.Block:
					return blockSetup;
				case AnimationType.Cube:
					return cubeSetup;
				case AnimationType.Cup:
					return cupSetup;
				case AnimationType.Key:
					return keySetup;
				case AnimationType.Off:
					return new SyncList<PosRotMapping>();
				default: return null;
			}
		}

		public List<PosRotMapping> GetCurrentAnimSetupList() {
			return GetCurrentAnimSetup()?.ToList();
		}

		public SyncList<PosRotMapping> GetAnimSetupByAnimType(AnimationType AnimType) {
			switch (AnimType) {
				case AnimationType.Block:
					return blockSetup;
				case AnimationType.Cube:
					return cubeSetup;
				case AnimationType.Cup:
					return cupSetup;
				case AnimationType.Key:
					return keySetup;
				case AnimationType.Off:
					return new SyncList<PosRotMapping>();
				default: return null;
			}
		}

		public List<SyncList<PosRotMapping>> GetAllAnimationSetups() {
			List<SyncList<PosRotMapping>> setups = new() {
				blockSetup,
				cubeSetup,
				cupSetup,
				keySetup
			};

			return setups;
		}

		/*
		* COMMANDS FOR SERVER - SYNCVARs
		*
		*/

		[Command(requiresAuthority = false)]
		public void CmdUpdateArmDuration(float value) {
			net_armMoveDuration = value;
		}

		[Command(requiresAuthority = false)]
		public void CmdUpdateHandDuration(float value) {
			net_handMoveDuration = value;
		}

		[Command(requiresAuthority = false)]
		public void CmdUpdateEvalDuration(float value) {
			net_evalDuration = value;
		}

		[Command(requiresAuthority = false)]
		public void CmdUpdateRestDuration(float value) {
			net_restDuration = value;
		}

		[Command(requiresAuthority = false)]
		public void CmdUpdateMoveDuration(float value) {
			net_moveDuration = value;
		}

		[Command(requiresAuthority = false)]
		public void CmdUpdateRepetitions(int value) {
			net_repetitions = value;
		}

		[Command(requiresAuthority = false)]
		public void CmdUpdateAnimType(AnimationType _oldAnimType, AnimationType _newAnimType, NetworkConnectionToClient sender = null) {
			if (Net_AnimType == _newAnimType) return;

			if (SetAnimType(_oldAnimType, _newAnimType)) {
				string roomID = sender.identity.GetComponent<ICharacter>().RoomID;

				SpawnTarget(_oldAnimType, _newAnimType, roomID);
			} else {
				TargetSetAnimTypeValue(sender, _oldAnimType);
				if (sender.identity.TryGetComponent(out INetworkCharacter netCharacter)) {
					netCharacter.RpcMessageClients(Globals.LocalizedMessages.AnimationCantChange, MessageType.NORMAL, true);
				}
			}
		}

		[TargetRpc]
		public void TargetSetAnimTypeValue(NetworkConnection connection, AnimationType animationType) {
			animSettings.AnimType = animationType;
		}

		/*
		*
		* FUNCTIONS FOR SETTING SYNCVAR VALUES
		*
		*/

		private void ChangeArmMoveDurationHook(float _old, float _new) {
			animSettings.ArmMoveDuration = net_armMoveDuration;
		}

		private void ChangeHandMoveDurationHook(float _old, float _new) {
			animSettings.HandMoveDuration = net_handMoveDuration;
		}

		private void ChangeEvalDurationHook(float _old, float _new) {
			animSettings.EvalDuration = net_evalDuration;
		}

		private void ChangeRestDurationHook(float _old, float _new) {
			animSettings.RestDuration = net_restDuration;
		}

		private void ChangeMoveDurationHook(float _old, float _new) {
			animSettings.MoveDuration = net_moveDuration;
		}

		private void ChangeRepetitionsHook(int _old, int _new) {
			animSettings.Repetitions = net_repetitions;
		}

		private void ChangeAnimTypeHook(AnimationType _old, AnimationType _new) {
			animSettings.AnimType = Net_AnimType;
			animSettings.CurrentSetup = GetCurrentAnimSetupList();

			SetupMarkers();
		}

		private void ChangeArmMoveDuration() {
			if (net_armMoveDuration == animSettings.ArmMoveDuration) {
				return;
			}
			CmdUpdateArmDuration(animSettings.ArmMoveDuration);
		}

		private void ChangeHandMoveDuration() {
			if (net_handMoveDuration == animSettings.HandMoveDuration) {
				return;
			}
			CmdUpdateHandDuration(animSettings.HandMoveDuration);
		}

		private void ChangeEvalDuration() {
			if (net_evalDuration == animSettings.EvalDuration) {
				return;
			}
			CmdUpdateEvalDuration(animSettings.EvalDuration);
		}

		private void ChangeRestDuration() {
			if (net_restDuration == animSettings.RestDuration) {
				return;
			}
			CmdUpdateRestDuration(animSettings.RestDuration);
		}

		private void ChangeMoveDuration() {
			if (net_moveDuration == animSettings.MoveDuration) {
				return;
			}
			CmdUpdateMoveDuration(animSettings.MoveDuration);
		}

		private void ChangeRepetitions() {
			if (net_repetitions == animSettings.Repetitions) {
				return;
			}
			CmdUpdateRepetitions(animSettings.Repetitions);
		}

		private void ChangeAnimType() {
			if (Net_AnimType == animSettings.AnimType) {
				return;
			}

			CmdUpdateAnimType(animSettings.PrevAnimType, animSettings.AnimType);
		}

		/// <summary>
		/// Helper method used for disabling colliders on old object to prevent new spawned to "bump" into them and be offset. This happens due to natural lag when information is delivered from server.
		/// </summary>
		/// <param name="_oldAnimType"></param>
		[ClientRpc]
		private void DisableTargetObjects(uint _old) {
			if (Net_PrevAnimType == Net_AnimType) {
				return;
			}

			if (objectProvider.TryGetObject(_old, out GameObject targetObj)) {
				targetObj.GetComponent<Renderer>().enabled = false;
				targetObj.GetComponent<Collider>().enabled = false;
			}
		}

		[ClientRpc]
		private void DisableLockObjects(List<uint> targetLockIds) {
			if (Net_PrevAnimType == Net_AnimType) {
				return;
			}

			foreach (var lockNetId in targetLockIds) {
				if (objectProvider.TryGetObject(lockNetId, out GameObject targetLockObj)) {
					targetLockObj.GetComponent<Renderer>().enabled = false;
					targetLockObj.GetComponent<Collider>().enabled = false;
				}
			}
		}

		/// <summary>
		/// Changes animation type only if (activePatient && animation not playing)
		/// </summary>
		/// <param name="_old">Previous (current) animation type</param>
		/// <param name="_new">New animation type</param>
		/// <returns>Bool value depending on whether animation type was changed or not</returns>
		[Server]
		public bool SetAnimType(AnimationType _old, AnimationType _new, bool force = false) {
			// change is not forced AND (there is existing training running OR there is a patient that is playing animation currently)
			if (!force && (animationServerManager.IsTrainingRunning || (sceneObjectManager.ActivePatient != null && sceneObjectManager.ActivePatient.IsAnimationPlaying()))) {
				return false;
			}
			if (_old != AnimationType.Off) {
				net_prevAnimType = _old;
			}
			net_animType = _new;
			return true;
		}

		/// <summary>
		/// Method used for handling changes made to SyncList. Refer to https://mirror-networking.gitbook.io/docs/manual/guides/synchronization/synclists for more information.
		/// </summary>
		/// <param name="op"></param>
		/// <param name="index"></param>
		/// <param name="oldItem"></param>
		/// <param name="newItem"></param>
		private void OnAnimationSetupUpdated(SyncList<PosRotMapping>.Operation op, int index, PosRotMapping oldItem, PosRotMapping newItem) {
			animSettings.CurrentSetup = GetCurrentAnimSetupList();

			if (roleSettings.CharacterRole == UserRole.Patient) {
				return;
			}

			int prefabIndex;
			// customDebug.Log(newItem);
			switch (op) {
				case SyncList<PosRotMapping>.Operation.OP_ADD:
					// index is where it was added into the list
					// newItem is the new item
					// customDebug.Log("Item added");
					prefabIndex = index == 0 ? 0 : 1;

					MarkerNumber marker = Instantiate(markerPrefabs[prefabIndex], newItem.Position, Quaternion.Euler(newItem.Rotation), transform);
					marker.Init($"{index + 1}");

					spawnedMarkers.Add(marker);
					break;
				case SyncList<PosRotMapping>.Operation.OP_INSERT:
					// index is where it was inserted into the list
					// newItem is the new item
					break;
				case SyncList<PosRotMapping>.Operation.OP_REMOVEAT:
					// index is where it was removed from the list
					// oldItem is the item that was removed
					if (spawnedMarkers.Count > index) {
						Destroy(spawnedMarkers[index].gameObject);
						spawnedMarkers.RemoveAt(index);
					}
					break;
				case SyncList<PosRotMapping>.Operation.OP_SET:
					// index is of the item that was changed
					// oldItem is the previous value for the item at the index
					// newItem is the new value for the item at the index
					// customDebug.Log("Item changed");
					if (spawnedMarkers.Count > index) {
						Destroy(spawnedMarkers[index].gameObject);
					}
					prefabIndex = index == 0 ? 0 : 1;

					MarkerNumber newMarker = Instantiate(markerPrefabs[prefabIndex], newItem.Position, Quaternion.Euler(newItem.Rotation), transform);
					newMarker.Init($"{index + 1}");

					if (spawnedMarkers.Count > index) {
						spawnedMarkers[index] = newMarker;
					} else {
						spawnedMarkers.Add(newMarker);
					}
					break;
				case SyncList<PosRotMapping>.Operation.OP_CLEAR:
					// list got cleared
					// customDebug.Log("List cleared");
					foreach (var item in spawnedMarkers) {
						Destroy(item.gameObject);
					}

					spawnedMarkers.Clear();
					break;
			}
		}

		/// <summary>
		/// Method that re-creates all markers locally only
		/// </summary>
		private void SetupMarkers() {
			if (roleSettings.CharacterRole == UserRole.Patient) {
				return;
			}

			foreach (var item in spawnedMarkers) {
				Destroy(item.gameObject);
			}
			spawnedMarkers.Clear();

			if (Net_AnimType == AnimationType.Off) {
				return;
			}

			SyncList<PosRotMapping> currentMapping = GetCurrentAnimSetup();

			int mappingsCount = currentMapping.Count;
			if (mappingsCount > 0) {
				MarkerNumber marker = Instantiate(markerPrefabs[0], currentMapping[0].Position, Quaternion.Euler(currentMapping[0].Rotation), transform);
				marker.Init($"{1}");

				spawnedMarkers.Add(marker);

				for (int i = 1; i < mappingsCount; i++) {
					marker = Instantiate(markerPrefabs[1], currentMapping[i].Position, Quaternion.Euler(currentMapping[i].Rotation), transform);
					marker.Init($"{i + 1}");

					spawnedMarkers.Add(marker);
				}
			}
		}


		[Server]
		public void SpawnTarget(AnimationType _oldAnimType, AnimationType _newAnimType, string roomID = "") {
			customDebug.Log($"Spawning object: '{_newAnimType}', old object: '{_oldAnimType}' in room: '{roomID}'");
			SpawnCorrectTarget(_oldAnimType, _newAnimType); // Call on server
			RpcSpawnCorrectTargetFakes(_oldAnimType, _newAnimType); // Call on all clients
		}

		[ClientRpc]
		public void RpcSpawnCorrectTargetFakes(AnimationType _oldAnimType, AnimationType _newAnimType) {
			SpawnCorrectTargetFakes(_oldAnimType, _newAnimType);
		}

		/// <summary>
		/// Method called on server, old objects are then destroyed and new spawned on all clients. Refer to https://mirror-networking.gitbook.io/docs/guides/gameobjects/spawning-gameobjects for more details.
		/// </summary>
		/// <param name="_oldAnimType"></param>
		/// <param name="_newAnimType"></param>
		[Server]
		public void SpawnCorrectTarget(AnimationType _oldAnimType, AnimationType _newAnimType) {
			// destroy old object
			if (objectProvider.TryGetObject(net_targetId, out GameObject targetObj)) {
				DisableTargetObjects(net_targetId);

				NetworkServer.Destroy(targetObj);
				net_targetId = 0;
			}

			if (_oldAnimType == AnimationType.Key) {
				DisableLockObjects(net_targetLockIds.ToList());
				foreach (var lockNetId in net_targetLockIds) {
					NetworkServer.Destroy(objectProvider.GetObject(lockNetId));
				}
				net_targetLockIds.Clear();
				net_targetLockId = 0;
			}

			if (_newAnimType == AnimationType.Off) {
				return;
			}

			GameObject targetPrefab = animSettings.TargetPrefabs[_newAnimType].Prefab;
			if (targetPrefab == null) {
				return;
			}

			float halfHeight = targetPrefab.GetComponent<Renderer>().bounds.extents.y;

			SceneManager.SetActiveScene(gameObject.scene);

			GameObject newObject = Instantiate(targetPrefab, spawnArea.position + new Vector3(0, halfHeight, 0), targetPrefab.transform.rotation);

			// if animation is Key AND patient is left handed, we flip the key
			if (_newAnimType == AnimationType.Key && sceneObjectManager.ActivePatient != null && sceneObjectManager.ActivePatient.IsLeftArmAnimated && newObject.TryGetComponent(out ObjectMirrorable objectMirrorable)) {
				objectMirrorable.MirrorObject();
			}
			newObject.name = targetPrefab.name;

			SceneManager.MoveGameObjectToScene(newObject, gameObject.scene);
			NetworkServer.Spawn(newObject);

			net_targetId = newObject.GetComponent<NetworkIdentity>().netId;

			if (GetCurrentAnimSetup().Count == 0) {
				SetAnimationStartPosition(false);
			}

			if (_newAnimType != AnimationType.Key) {
				return;
			}

			SpawnLocks();
		}

		[Server]
		private void SpawnLocks() {
			GameObject lockObject = animSettings.TargetPrefabs[AnimationType.Key].PrefabExtra;
			if (lockObject == null) {
				return;
			}
			float halfHeight = lockObject.transform.lossyScale.y * lockObject.GetComponent<MeshFilter>().sharedMesh.bounds.extents.y;

			// we spawn three locks in this case
			// Center
			GameObject newLock1 = Instantiate(lockObject, spawnArea.position + new Vector3(0, halfHeight, 0.25f), lockObject.transform.rotation);
			SceneManager.MoveGameObjectToScene(newLock1, gameObject.scene);
			NetworkServer.Spawn(newLock1);
			// Left
			GameObject newLock2 = Instantiate(lockObject, spawnArea.position + new Vector3(-0.3f, halfHeight, 0.18f), lockObject.transform.rotation);
			newLock2.transform.LookAt(spawnArea.position + new Vector3(0, halfHeight, 0));
			SceneManager.MoveGameObjectToScene(newLock2, gameObject.scene);
			NetworkServer.Spawn(newLock2);
			// Right
			GameObject newLock3 = Instantiate(lockObject, spawnArea.position + new Vector3(+0.3f, halfHeight, 0.18f), lockObject.transform.rotation);
			newLock3.transform.LookAt(spawnArea.position + new Vector3(0, halfHeight, 0));
			SceneManager.MoveGameObjectToScene(newLock3, gameObject.scene);
			NetworkServer.Spawn(newLock3);

			net_targetLockIds.Add(newLock1.GetComponent<NetworkIdentity>().netId);
			net_targetLockIds.Add(newLock2.GetComponent<NetworkIdentity>().netId);
			net_targetLockIds.Add(newLock3.GetComponent<NetworkIdentity>().netId);

			SetLockPosition(new PosRotMapping(newLock1.GetComponent<TargetUtility>().customTargetPos.transform), newLock1.GetComponent<NetworkIdentity>().netId);
		}

		/// <summary>
		/// Function used to destroy old objects and spawn new. Used only on client.
		/// </summary>
		/// <param name="_oldAnimType"></param>
		/// <param name="_newAnimType"></param>
		[Client]
		public void SpawnCorrectTargetFakes(AnimationType _oldAnimType, AnimationType _newAnimType) {
			// We destroy fake object here on client
			Destroy(targetFake);

			// We destroy normal object on client, in case they did not get destroyed by server
			if (objectProvider.TryGetObject(net_targetId, out GameObject targetObj)) {
				if (targetObj.name == _oldAnimType.ToString()) {
					Destroy(targetObj);
				}
			}

			if (_newAnimType == AnimationType.Off) {
				return;
			}

			GameObject targetFakePrefab = animSettings.TargetPrefabs[_newAnimType].PrefabFake;
			if (targetFakePrefab == null) {
				return;
			}
			targetFake = Instantiate(targetFakePrefab, spawnArea.position, targetFakePrefab.transform.rotation);

			// if animation is Key AND patient is left handed, we flip the key
			if (_newAnimType == AnimationType.Key && sceneObjectManager.ActivePatient != null && sceneObjectManager.ActivePatient.IsLeftArmAnimated && targetFake.TryGetComponent(out ObjectMirrorable objectMirrorable)) {
				objectMirrorable.MirrorObject();
			}
			targetFake.name = targetFakePrefab.name;
		}

		/// <summary>
		/// Function used for changing or adding start position for current chosen setup. Uses current position and rotation of target object
		/// </summary>
		/// <param name="forceNew">Used to determine whether we want to force rewrite first position no matter what</param>
		/// <param name="targetPosRotMapping">Custom position and rotation</param>
		[Server]
		public void SetAnimationStartPosition(bool forceNew, PosRotMapping targetPosRotMapping = null, NetworkConnectionToClient sender = null) {
			if (!isServer) {
				return;
			}

			if (targetPosRotMapping == null) {
				targetPosRotMapping = GetPosRotFromCurrentObject();
				if (targetPosRotMapping == null) { // we have to double check in case we failed to get current mapping
					return;
				}
			}

			if (!forceNew && !IsTargetInBounds(targetPosRotMapping, sender)) {
				customDebug.LogWarning("Cannot set target position - 'target object' not in bounds");
				return;
			}

			// due to how SyncList works we can't simply change value in list element, we have to replace whole element
			// https://mirror-networking.gitbook.io/docs/manual/guides/synchronization/synclists
			if (GetCurrentAnimSetup().Count >= 1) {
				GetCurrentAnimSetup()[0] = targetPosRotMapping;
			} else {
				GetCurrentAnimSetup().Add(targetPosRotMapping);
			}
		}


		/// <summary>
		/// Sets the lock position at the end of synclist
		/// </summary>
		/// <param name="lockTargetPosRot"></param>
		[Server]
		public void SetLockPosition(PosRotMapping lockTargetPosRot, uint lockId, NetworkConnectionToClient sender = null) {
			if (Net_AnimType != AnimationType.Key) {
				return;
			}

			if (!IsTargetInBounds(lockTargetPosRot, sender)) {
				customDebug.LogWarning("Cannot set lock position - not in bounds");
				return;
			}

			// we only allow 2 positions in case of Key animation
			int setupCount = GetCurrentAnimSetup().Count;
			if (setupCount == 0) {
				customDebug.LogWarning("Cannot set lock position - no key position");
				sender.identity.GetComponent<NetworkCharacterManager>().RpcMessageClients(Globals.LocalizedMessages.MissingKey, MessageType.WARNING, true);
				return;
			}
			if (setupCount > 1) {
				GetCurrentAnimSetup()[setupCount - 1] = lockTargetPosRot;
			} else {
				GetCurrentAnimSetup().Add(lockTargetPosRot);
			}

			net_targetLockId = lockId;
		}

		/// <summary>
		/// Checks if object is above table AND in range of arm. If there is no active patient only checks if object is above table
		/// </summary>
		/// <param name="targetPosRotMapping"></param>
		/// <returns></returns>
		[Server]
		public bool IsTargetInBounds(PosRotMapping targetPosRotMapping, NetworkConnectionToClient sender = null) {
			GameObject tableObject = ((TableHelper)sceneObjectManager.Table).gameObject;

			if (tableObject == null) {
				return false;
			}
			RaycastHit[] hits = new RaycastHit[10];
			// we set max raycast distance due to optimization reasons
			// hits = Physics.RaycastAll(targetPosRotMapping.position, Vector3.down, maxRaycastDistance);
			int hitNum = physicsScene.Raycast(targetPosRotMapping.Position, Vector3.down, hits, maxRaycastDistance);
			bool isAboveTable = false;
			int hitsCount = hits.Length;
			for (int i = 0; i < hitNum && i < hitsCount; i++) {
				if (hits[i].collider.gameObject.Equals(tableObject)) {
					isAboveTable = true;
					break;
				}
			}
			if (!isAboveTable) {
				customDebug.LogWarning("Target object not above Table");
				if (isServer && sender != null) {
					sender.identity.GetComponent<NetworkCharacterManager>().RpcMessageClients(Globals.LocalizedMessages.ObjectNotAboveTable, MessageType.WARNING, true);
				}
				return false;
			}


			if (sceneObjectManager.ActivePatient != null) {
				if (!sceneObjectManager.ActivePatient.IsTargetInRange(targetPosRotMapping.Position)) {
					float armLength = sceneObjectManager.ActivePatient.GetArmLengthWithSlack();
					float targetDistance = sceneObjectManager.ActivePatient.GetTargetRangeFromShoulder(targetPosRotMapping.Position);

					customDebug.LogWarning($"Arm cannot reach object, too far away: {targetDistance}m > {armLength}m");
					if (isServer && sender != null) {
						Arg[] args = new Arg[] { new Arg("0", targetDistance), new Arg("1", armLength) };
						sender.identity.GetComponent<NetworkCharacterManager>().RpcMessageClients(Globals.LocalizedMessages.CannotGrab, MessageType.WARNING, true, args);
					}
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Helper method for getting posRot of current target object
		/// </summary>
		/// <returns></returns>
		public PosRotMapping GetPosRotFromCurrentObject() {
			if (!objectProvider.TryGetObject(Net_TargetId, out GameObject targetObject)) {
				customDebug.LogError("Failed to find object: " + Net_AnimType.ToString());
				return null;
			}

			return new PosRotMapping(targetObject.transform);
		}
	}
}