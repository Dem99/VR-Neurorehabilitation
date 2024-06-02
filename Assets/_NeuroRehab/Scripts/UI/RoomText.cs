using UnityEngine;
using UnityEngine.Localization.Components;

using NeuroRehab.ScriptObjects;
using NeuroRehab.Core;

namespace NeuroRehab.UI {

	public class RoomText : MonoBehaviour {
		[SerializeField] private NetworkSettings networkSettings;
		[SerializeField] private LocalizeStringEvent localizeStringEvent;

		public string roomName;
		public string roomId;

		void Start() {
			roomId = networkSettings.GetRoomId();
			if (roomId == null || roomId == "") {
				return;
			}
			RoomData room = networkSettings.GetRoomById(roomId);
			roomName = room.roomName;

			localizeStringEvent.RefreshString();
		}
	}
}