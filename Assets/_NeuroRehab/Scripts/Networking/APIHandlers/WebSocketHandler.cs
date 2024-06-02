using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using WebSocketSharp;
using WebSocketSharp.Server;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Collections.Generic;
// using AYellowpaper.SerializedCollections;

using NeuroRehab.Managers.Animations;
using NeuroRehab.Core;
using NeuroRehab.ScriptObjects;
using NeuroRehab.Interfaces;

namespace NeuroRehab.Networking.API {
	public class WebSocketHandler : MonoBehaviour, INetApi {
		private readonly CustomDebug customDebug = new("WEBSOCKET");
		[Header("Scriptable objects")]
		[SerializeField] private NetworkSettings networkSettings;


		[Header("Dependencies")]
		// [SerializeField] private SerializedDictionary<string, SocketBehaviourHandler> connectionsList = new();
		private Dictionary<string, SocketBehaviourHandler> connectionsList = new();
		private readonly ConcurrentQueue<WebsocketMessageData> incomingMessagesQueue = new();

		private int port;
		private IPAddress ip;

		private WebSocketServer webSocketServer;

		private void OnEnable() {
			port = networkSettings.WebsocketPort;
		}

		void Start() {
			ip = IPAddress.Parse(NetworkManager.singleton.networkAddress);
			webSocketServer = new(ip, port);

			webSocketServer.AddWebSocketService("/", () => new SocketBehaviourHandler(customDebug, connectionsList, incomingMessagesQueue, networkSettings));
			// webSocketServer.Log.Level = LogLevel.Trace;

			webSocketServer.Start();
			customDebug.LogInfo($"Websocket server running on port {webSocketServer.Port}");
		}

		private void OnDestroy() {
			if (webSocketServer == null) {
				return;
			}
			webSocketServer.Stop();
		}

		private void Update() {
			if (incomingMessagesQueue.TryDequeue(out WebsocketMessageData message)) {
				if (message.op == "move") {
					HandleMove(message);
				} else {
					customDebug.LogError("Unknown operation added to queue!!");
				}
			}
		}

		private void HandleMove(WebsocketMessageData websocketData) {
			try {
				Scene sceneToUse = networkSettings.GetSceneByRoomId(websocketData.roomid);

				SceneHelper sceneHelper = new(sceneToUse);
				AnimationServerManager animServer = sceneHelper.FindRootObject<AnimationServerManager>();
				if (animServer != null) {
					animServer.ProgressStep(ExerciseState.MOVE);
				}
			} catch (Exception e) {
				customDebug.LogError(e.Message);
			}
		}

		public void StartRestPeriod(string roomID) {
			if (connectionsList.TryGetValue(roomID, out var connection)) {
				connection.SendStartRestPeriod(roomID);
			} else {
				customDebug.LogError($"No connection found for room: '{roomID}'");
			}
		}

		public void StopRestPeriod(string roomID) {
			if (connectionsList.TryGetValue(roomID, out var connection)) {
				connection.SendStopRestPeriod(roomID);
			} else {
				customDebug.LogError($"No connection found for room: '{roomID}'");
			}
		}

		public void StartEegEvaluation(string roomID) {
			if (connectionsList.TryGetValue(roomID, out var connection)) {
				connection.SendStartEegEvaluation(roomID);
			} else {
				customDebug.LogError($"No connection found for room: '{roomID}'");
			}
		}

		public void StopEegEvaluation(string roomID) {
			if (connectionsList.TryGetValue(roomID, out var connection)) {
				connection.SendStopEegEvaluation(roomID);
			} else {
				customDebug.LogError($"No connection found for room: '{roomID}'");
			}
		}
	}

	[Serializable]
	public class SocketBehaviourHandler : WebSocketBehavior {
		private readonly CustomDebug customDebug;
		// private readonly SerializedDictionary<string, SocketBehaviourHandler> connections;
		private readonly Dictionary<string, SocketBehaviourHandler> connections;
		private readonly ConcurrentQueue<WebsocketMessageData> messageQueue;
		private readonly NetworkSettings networkSettings;

		/*
		public SocketBehaviourHandler(CustomDebug _customDebug, SerializedDictionary<string, SocketBehaviourHandler> _connections, ConcurrentQueue<WebsocketMessageData> _queue, NetworkSettings _networkSettings) {
			customDebug = _customDebug;
			connections = _connections;
			messageQueue = _queue;
			networkSettings = _networkSettings;
		}
		*/
		public SocketBehaviourHandler(CustomDebug _customDebug, Dictionary<string, SocketBehaviourHandler> _connections, ConcurrentQueue<WebsocketMessageData> _queue, NetworkSettings _networkSettings) {
			customDebug = _customDebug;
			connections = _connections;
			messageQueue = _queue;
			networkSettings = _networkSettings;
		}

		protected override void OnMessage (MessageEventArgs e) {
			WebsocketMessageData data = JsonUtility.FromJson<WebsocketMessageData>(e.Data);

			// customDebug.Log(data);
			switch (data.op) {
				case "checkin":
					HandleCheckIn(data);
					break;
				case "move":
					HandleMove(data);
					break;
				default:
					customDebug.LogWarning($"Unknown message received - '{Regex.Replace(e.Data, @"\s+", string.Empty)}'");
					Send(JsonUtility.ToJson(new WebsocketMessageData("unknown", data.roomid)));
					break;
			}
		}

		protected override void OnOpen() {
			customDebug.Log($"New connection - {ID}");
		}

		protected override void OnClose(CloseEventArgs e) {
			foreach (var item in connections) {
				if (item.Value.Equals(this)) {
					connections.Remove(item.Key);
					break;
				}
			}
			customDebug.Log($"Closed connection - {ID}");
		}

		private void HandleCheckIn(WebsocketMessageData websocketData) {
			if (!networkSettings.ExistsRoomId(websocketData.roomid) || connections.ContainsKey(websocketData.roomid) || connections.ContainsValue(this)) {
				Send(JsonUtility.ToJson(new WebsocketMessageData("failedCheckin", websocketData.roomid)));
				return;
			}

			customDebug.Log($"Connection {ID} checked in to room: {websocketData.roomid}");
			connections.Add(websocketData.roomid, this);
		}

		private void HandleMove(WebsocketMessageData websocketData) {
			// Send(JsonUtility.ToJson(websocketData));
			messageQueue.Enqueue(websocketData);
		}

		public void SendStartRestPeriod(string roomID) {
			Send(JsonUtility.ToJson(new WebsocketMessageData("startRestPeriod", roomID)));
		}

		public void SendStopRestPeriod(string roomID) {
			Send(JsonUtility.ToJson(new WebsocketMessageData("stopRestPeriod", roomID)));
		}

		public void SendStartEegEvaluation(string roomID) {
			Send(JsonUtility.ToJson(new WebsocketMessageData("startEegEvaluation", roomID)));
		}

		public void SendStopEegEvaluation(string roomID) {
			Send(JsonUtility.ToJson(new WebsocketMessageData("stopEegEvaluation", roomID)));
		}
	}

	public class WebsocketMessageData {
		public string op;
		public string roomid;

		public WebsocketMessageData(string _op, string _roomid) {
			op = _op;
			roomid = _roomid;
		}

		public override string ToString() {
			return $"{op}, {roomid}";
		}
	}
}
