using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;

using NeuroRehab.Core;
using NeuroRehab.ScriptObjects;
using NeuroRehab.Networking.API;
using NeuroRehab.Managers.Character;
using NeuroRehab.Drag;
using NeuroRehab.Interfaces;

namespace NeuroRehab.Networking {

	/// <summary>
	/// Class overriding default NetworkManager from Mirror. Used for spawning custom character models and when character disconnects.
	/// </summary>
	[DisallowMultipleComponent]
	public class CustomNetworkManager : NetworkManager {
		private readonly CustomDebug customDebug = new("NET_MANAGER");
		private static bool isAppInitialized = false;

		[Header("Custom Script Attributes")]
		[Header("Scriptable objects")]
		[SerializeField] private CustomXRSettings xrSettings;
		[SerializeField] private RoleSettings roleSettings;
		[SerializeField] private AvatarSettings avatarSettings;
		[SerializeField] private NetworkSettings networkSettings;
		[SerializeField] private SceneChangeEventsSO sceneChange;

		[Header("Dependencies")]
		[SerializeField] private GameObject onlineXRPrefab;
		[SerializeField] private GameObject onlineDesktopPrefab;

		[SerializeField] private Behaviour XRUIInputModule;

		[SerializeField] private WebSocketHandler webSocketHandler;
		[SerializeField] private RestRequestHandler restRequestHandler;

		[Header("Room management")]
		[SerializeField] private Spawner spawner;
		[Scene] public string menuScene;
		[Scene] public string roomScene;

		public WebSocketHandler WebSocketHandler { get => webSocketHandler; }
		public RestRequestHandler RestRequestHandler { get => restRequestHandler; }

		public override void Awake() {
			{
				if (isAppInitialized) {
					Destroy(gameObject);
					return;
				}

				// bandaid solution to prevent errors spam when client disconnects from server
				// TODO rework ??? find a way to make this normal, not this monster
				gameObject.AddComponent<SceneInterestManagement>();
				isAppInitialized = true;
			}

			base.Awake();
		}

		private void OnEnable() {
			networkSettings.OnRequestJoin += JoinClient;
			networkSettings.OnRequestDisconnect += DisconnectClient;
		}

		private void OnDisable() {
			networkSettings.OnRequestJoin -= JoinClient;
			networkSettings.OnRequestDisconnect -= DisconnectClient;
		}

		// Called on SERVER only when SERVER starts
		public override void OnStartServer() {
			base.OnStartServer();
			customDebug.Log($"Server started: {networkAddress}");

			// get root objects in current active scene (menu scene)
			List<GameObject> rootObjects = new();
			Scene scene = SceneManager.GetActiveScene();
			scene.GetRootGameObjects(rootObjects);
			foreach (GameObject rootObject in rootObjects) {
				rootObject.SetActive(false);
			}

			// little hack on server to prevent warnings to flood console when XR client connects (it's pointless warning, since we're not using UI on server)
			XRUIInputModule.transform.root.gameObject.SetActive(true);
			XRUIInputModule.GetComponent<EventSystem>().enabled = false;
			XRUIInputModule.enabled = false;

			NetworkServer.RegisterHandler<CharacterMessage>(OnCreateCharacter);
			StartCoroutine(ServerLoadAllSubScenes());

			webSocketHandler.enabled = true;
			restRequestHandler.enabled = true;
		}

		/// <summary>
		/// In case you still need to use Start(), don't forget to call 'base.Start();'. The reason is because NetworkManager (parent class) already starts server if this is server build.
		/// </summary>
		public override void Start() {
			string[] args = Environment.GetCommandLineArgs();

			bool ipSetFromArgs = false;
			string input = "";
			for (int i = 0; i < args.Length; i++) {
				// Debug.Log("ARG " + i + ": " + args [i]);
				if (args[i].ToLower() == "-serverip") {
					input = args[i + 1];
					networkAddress = input;
					ipSetFromArgs = true;
					continue;
				} else if (args[i].ToLower() == "-socketport") {
					input = args[i + 1];
					networkSettings.WebsocketPort = int.Parse(input);
					continue;
				}
			}

			if (!ipSetFromArgs && networkSettings.InitializedFromFile) {
				networkAddress = networkSettings.IpAddress;
			} else {
				networkSettings.IpAddress = networkAddress;
			}
#if UNITY_EDITOR
			GetComponent<NetworkManagerHUD>().enabled = true;
#endif

			base.Start();
		}

		// https://mirror-networking.gitbook.io/docs/manual/guides/communications/networkmanager-callbacks
		public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) {
			StartCoroutine(LoadSceneOnClient(newSceneName, sceneOperation));
		}

		// https://mirror-networking.gitbook.io/docs/manual/guides/communications/networkmanager-callbacks
		public override void OnClientSceneChanged() {
			base.OnClientSceneChanged();

			sceneChange.FinishSceneChange();
		}

		private IEnumerator LoadSceneOnClient(string sceneName, SceneOperation sceneOperation) {
			sceneChange.StartSceneChange();
			yield return new WaitForSecondsRealtime(sceneChange.SceneChangeDuration);

			loadingSceneAsync = SceneManager.LoadSceneAsync(sceneName);

			// don't change the client's current networkSceneName when loading additive scene content
			if (sceneOperation == SceneOperation.Normal)
				networkSceneName = sceneName;
			// base.OnClientChangeScene(sceneName, sceneOperation, customHandling);
		}

		/// <summary>
		/// Called on CLIENT only when CLIENT connects
		/// </summary>
		public override void OnClientConnect() {
			//base.OnClientConnect();

			// you can send the message here
			CharacterMessage characterMessage = new() {
				role = roleSettings.CharacterRole,
				hmdType = xrSettings.HmdType,
				controllerType = xrSettings.ControllerType,
				isFemale = avatarSettings.IsFemale,
				avatarNumber = avatarSettings.AvatarNumber,
				sizeMultiplier = avatarSettings.SizeMultiplier,
				offsetDistance = avatarSettings.OffsetDistance,
				isXRActive = xrSettings.XrActive,
				roomID = networkSettings.GetRoomId()
			};

			NetworkClient.Send(characterMessage);
		}

		/// <summary>
		/// Custom Character spawner. We use CharacterMessage to determine what data to use. Refer to https://mirror-networking.gitbook.io/docs/guides/gameobjects/custom-character-spawning for more information.
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="message"></param>
		private void OnCreateCharacter(NetworkConnectionToClient conn, CharacterMessage message) {
			customDebug.Log($"New connection requested, Client using: HMD: '{message.hmdType}', female: '{message.isFemale}', avatarIndex: '{message.avatarNumber}', role: '{message.role}', XR: '{message.isXRActive}', room: '{message.roomID}'");

			RoomData room = networkSettings.GetRoomById(message.roomID);
			if (!room.loadedScene) {
				customDebug.Log($"Loading room {room.roomID}");
				StartCoroutine(ServerLoadSubScene(room));
			}

			StartCoroutine(OnServerCreateCharacter(conn, message, room));
		}

		private IEnumerator OnServerCreateCharacter(NetworkConnectionToClient conn, CharacterMessage message, RoomData roomToUse) {
			// Wait for server to async load subscene
			yield return new WaitUntil(() => roomToUse.loadedScene);

			if (roomToUse == null) {
				customDebug.LogError($"Cannot Instantiate character prefab, room ID: '{message.roomID}' not found!!");
				yield break;
			}
			while (!roomToUse.loadedScene)
				yield return null;

			// Send Scene message to client to additively load the game scene
			conn.Send(new SceneMessage { sceneName = roomScene, sceneOperation = SceneOperation.Normal, customHandling = true });

			// Wait for end of frame before adding the player to ensure Scene Message goes first
			yield return new WaitForEndOfFrame();

			GameObject characterPrefab;
			if (message.role == UserRole.Therapist || message.role == UserRole.Patient) {
				if (message.isXRActive) {
					characterPrefab = onlineXRPrefab;
				} else {
					characterPrefab = onlineDesktopPrefab;
				}
			} else {
				customDebug.LogError($"Cannot Instantiate character prefab '{message.role}'- not found!!");
				yield break;
			}

			SceneManager.SetActiveScene(roomToUse.scene);
			GameObject newUserObject = Instantiate(characterPrefab);

			CharacterManager characterManager = newUserObject.GetComponent<CharacterManager>();

			characterManager.Initialize(message.isFemale, message.avatarNumber, message.sizeMultiplier, message.offsetDistance, message.role, message.roomID);
			if (message.isXRActive) {
				((XRCharacterManager)characterManager).Initialize(message.controllerType, message.hmdType);
			}

			newUserObject.name = $"{characterPrefab.name} [connId={conn.connectionId}]";

			customDebug.Log($"New connection: '{conn.connectionId}'");
			NetworkServer.AddPlayerForConnection(conn, newUserObject);
			roomToUse.users.Add(conn.connectionId + "");

			// Do this only on server, not on clients
			// This is what allows the NetworkSceneChecker on player and scene objects
			// to isolate matches per scene instance on server.
			SceneManager.MoveGameObjectToScene(newUserObject, roomToUse.scene);
		}

		private IEnumerator ServerLoadAllSubScenes() {
			foreach (var room in networkSettings.GetRooms()) {
				// we wait for each sub-scene to finish loading, then we continue;
				yield return StartCoroutine(ServerLoadSubScene(room));
				yield return new WaitUntil(() => room.loadedScene);
			}
		}

		private IEnumerator ServerLoadSubScene(RoomData roomData) {
			if (roomData.loadedScene) {
				yield break;
			}
			int index = SceneManager.sceneCount;

			yield return SceneManager.LoadSceneAsync(roomScene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics3D });

			roomData.scene = SceneManager.GetSceneAt(index); // new scene has always highest index
			spawner.SpawnRoomObjects(roomData.scene, roomData.roomID);

			customDebug.Log($"Opened new room: '{roomData.roomID}'");
			roomData.loadedScene = true;
		}

		public override void OnClientDisconnect() {
			base.OnClientDisconnect();

			if (SceneManager.GetActiveScene().buildIndex == SceneUtility.GetBuildIndexByScenePath(menuScene)) {
				return;
			}

			// Added to force load of offline scene, we can't rely on default functinality
			// bcs we don't have it setup 'properly' in network manager due to the fact we are handling scenes manually (additive scenes)
			SceneManager.LoadScene(menuScene);
		}

		/// <summary>
		/// When client disconnects, we clear all client authorities besides Avatar, because otherwise all objects that he has authority over would be destroyed
		/// </summary>
		/// <param name="conn"></param>
		public override void OnServerDisconnect(NetworkConnectionToClient conn) {
			NetworkIdentity[] ownedObjects = new NetworkIdentity[conn.owned.Count];
			conn.owned.CopyTo(ownedObjects);

			ICharacter character = conn.identity.GetComponent<ICharacter>();

			int avatarLayer = LayerMask.NameToLayer("Avatar");
			foreach (NetworkIdentity objectIdentity in ownedObjects) {
				if (objectIdentity.gameObject.layer == avatarLayer) { // Layer 7 = Avatar
					continue;
				}

				if (objectIdentity.TryGetComponent(out TargetDraggable targetDraggable)) {
					if (targetDraggable.ItemPickedUp) { // this should not happen, but just in case
						targetDraggable.EnableDragWrapper();
					}
				}
				objectIdentity.gameObject.GetComponent<NetworkTransform>().syncDirection = SyncDirection.ServerToClient;
				objectIdentity.RemoveClientAuthority();
				customDebug.Log($"USER'{conn.identity.netId}' - object with netID '{objectIdentity.netId}' released authority.");
			}

			networkSettings.GetRoomById(character.RoomID).users.Remove(conn.connectionId + "");

			customDebug.LogInfo($"USER'{conn.identity.netId}' - disconnected!");
			base.OnServerDisconnect(conn);
		}

		private void JoinClient() {
			if (!networkAddress.Equals("localhost")) {
				if (!IsIPValid(networkAddress)) {
					customDebug.LogWarning($"Invalid IP: {networkAddress}");
					return;
				}
			}

			StartClient();
		}

		private void DisconnectClient() {
			StopClient();
		}

		private bool IsIPValid(string strIP) {
			// Split string by ".", check that array length is 4
			string[] arrOctets = strIP.Split('.');
			if (arrOctets.Length != 4)
				return false;

			// Check each substring whether it parses to byte
			foreach (string strOctet in arrOctets)
				if (!byte.TryParse(strOctet, out _))
					return false;

			return true;
		}
	}
}