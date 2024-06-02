using Mirror;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using NeuroRehab.Core;

namespace NeuroRehab.ScriptObjects {

	[CreateAssetMenu(menuName = "ScriptObjects/Settings/Network")]
	public class NetworkSettings : ScriptableObject {

		[SerializeField] private string ipAddress;
		[SerializeField] private int websocketPort = 7778;
		[SerializeField] private int restPort = 7779;
		[SerializeField] private string roomId;
		[SerializeField] private bool initializedFromFile = false;
		[SerializeField] private List<ServersData> serversList;

		[SerializeField, NonReorderable] private List<RoomData> rooms = new();

		public event Action OnRequestJoin;
		public event Action OnRequestDisconnect;

		public int RestPort { get => restPort; set => restPort = value; }
		public int WebsocketPort { get => websocketPort; set => websocketPort = value; }
		public string IpAddress { get => ipAddress; set => ipAddress = value; }
		public bool InitializedFromFile { get => initializedFromFile; }
		public List<ServersData> ServersList{ get => serversList; set => serversList = value; }

		private void OnEnable() {
			initializedFromFile = false;

			rooms = rooms.Distinct(new RoomDataComparer()).ToList();

			foreach (var room in rooms) {
				room.users.Clear();
			}
		}

		public void Init(string _ipAddress, int _websocketPort, int _restPort, string _roomId, List<ServersData> _serversList) {
			initializedFromFile = true;

			ipAddress = _ipAddress;
			websocketPort = _websocketPort;
			restPort = _restPort;
			roomId = _roomId;
			serversList = _serversList;
		}

		public string GetRoomId() {
			return roomId;
		}

		public void SetRoomId(string _roomId) {
			roomId = _roomId;
		}

		public void SetRooms(List<RoomDataSerialized> newRooms) {
			rooms.Clear();
			foreach (var room in newRooms) {
				rooms.Add(new (room));
			}
		}

		public List<RoomData> GetRooms() {
			return rooms;
		}

		public RoomData GetCurrentRoom() {
			return rooms.Find(x => x.roomID == roomId);
		}

		public RoomData GetRoomById(string roomID) {
			return rooms.Find(x => x.roomID == roomID);
		}

		[Server]
		public Scene GetSceneByRoomId(string roomID) {
			return rooms.Find(x => x.roomID == roomID).scene;
		}

		[ServerCallback]
		public string GetRoomIdByScene(Scene scene) {
			return rooms.Find(x => x.scene == scene)?.roomID;
		}

		[Server]
		public bool ExistsRoomId(string roomID) {
			return rooms.Find(x => x.roomID == roomID) != null;
		}

		public void ClientJoin() {
			OnRequestJoin?.Invoke();
		}

		public void ClientDisconnect() {
			OnRequestDisconnect?.Invoke();
		}
	}
}