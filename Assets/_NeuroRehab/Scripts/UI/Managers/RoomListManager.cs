using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;

using NeuroRehab.ScriptObjects;
using NeuroRehab.Core;

namespace NeuroRehab.UI.Managers {
	public class RoomListManager : MonoBehaviour {
		private readonly CustomDebug customDebug = new("ROOM_LIST");

		[SerializeField] private NetworkSettings networkSettings;

		[SerializeField] private RoomButton roomButtonPrefab;
		[SerializeField] private Transform roomListContent;

		[SerializeField] private Color selectedColor;
		[SerializeField] private Color deselectedColor;

		private readonly List<RoomButton> roomButtons = new();

		private int calls = 0;

		private void Start() {
			#if UNITY_SERVER
			return;
			#endif

			StartCoroutine(SetupRoomList("http://" + networkSettings.IpAddress + ":" + networkSettings.RestPort + "/rooms"));
		}

		private void OnDisable() {
			StopAllCoroutines();
		}

		private void OnDestroy() {
			StopAllCoroutines();
		}

		private void SetActiveRoom(string roomId, Button _button) {
			networkSettings.SetRoomId(roomId);

			foreach (Button button in roomButtons) {
				button.image.color = deselectedColor;
			}

			_button.image.color = selectedColor;
		}

		private void PopulateRoomList(RoomDataSerialized[] rooms) {
			foreach (Button item in roomButtons) {
				Destroy(item.gameObject);
			}
			roomButtons.Clear();

			List<RoomData> roomDatas = networkSettings.GetRooms();
			roomDatas.Clear();

			string roomId = networkSettings.GetRoomId();

			foreach (RoomDataSerialized room in rooms) {
				roomDatas.Add(new RoomData(room));

				RoomButton newButton = Instantiate(roomButtonPrefab, roomListContent);

				newButton.onClick.AddListener(delegate {
					SetActiveRoom(room.roomID, newButton);
				});

				newButton.SetRoomButton(room.roomName, room.userCount, room.roomID);
				roomButtons.Add(newButton);

				if (roomId == room.roomID) {
					newButton.onClick.Invoke();
				}
			}

			if (roomId == null || roomId == "") {
				if (roomButtons.Count > 0) {
					roomButtons[0].onClick.Invoke();
				}
			}
		}

		private IEnumerator SetupRoomList(string uri) {
			yield return null; // wait one frame

			while (calls < 10) {
				calls++;
				RoomDataSerialized[] roomsSerialized = new RoomDataSerialized[0];
				bool successfullRequest = false;

				using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)) {
					yield return webRequest.SendWebRequest();

					string[] pages = uri.Split('/');
					int page = pages.Length - 1;

					try {
						switch (webRequest.result) {
							case UnityWebRequest.Result.ConnectionError:
							case UnityWebRequest.Result.DataProcessingError:
								customDebug.LogWarning($"{pages[page]} : Error: {webRequest.error} - src: '{gameObject.name}'");
								break;
							case UnityWebRequest.Result.ProtocolError:
								customDebug.LogWarning($"{pages[page]} : HTTP Error: {webRequest.error} - src: '{gameObject.name}'");
								break;
							case UnityWebRequest.Result.Success:
								// customDebug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
								RoomsArrayJson roomsWrapper = JsonUtility.FromJson<RoomsArrayJson>(webRequest.downloadHandler.text);

								roomsSerialized = JsonHelper.FromJson<RoomDataSerialized>(roomsWrapper.rooms);
								successfullRequest = true;
								break;
						}
					} catch (System.Exception ex) {
						customDebug.LogError("Error occured while trying to parse response. See log trace:");
						Debug.LogError(ex);
					}
				}

				if (!successfullRequest) {
					List<RoomData> rooms = networkSettings.GetRooms();
					int roomCount = rooms.Count;
					roomsSerialized = new RoomDataSerialized[roomCount];
					for (int i = 0; i < roomCount; i++) {
						roomsSerialized[i] = new RoomDataSerialized(rooms[i]);
					}
				}

				PopulateRoomList(roomsSerialized);

				yield return new WaitForSecondsRealtime(60f);
			}
		}

		private class RoomsArrayJson {
			public string rooms;
		}
	}
}