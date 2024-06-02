using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeuroRehab.UI {
	public class RoomButton : Button {
		[SerializeField] private TMP_Text roomNameField;
		[SerializeField] private TMP_Text userCountField;
		[SerializeField] private TMP_Text roomIDField;

		private string roomName;
		private string roomID;
		private int userCount = 0;

		public TMP_Text RoomNameField { get => roomNameField; set => roomNameField = value; }
		public TMP_Text UserCountField { get => userCountField; set => userCountField = value; }
		public TMP_Text RoomIDField { get => roomIDField; set => roomIDField = value; }

		public void SetRoomName(string _roomName) {
			roomName = _roomName;

			roomNameField.text = roomName;
		}

		public void SetUserCount(int _userCount) {
			userCount = _userCount;

			userCountField.text = userCount + "";
		}

		public void SetRoomID(string _roomID) {
			roomID = _roomID;

			roomIDField.text = roomID;
		}

		public void SetRoomButton(string _roomName, int _userCount, string _roomID) {
			SetRoomName(_roomName);
			SetRoomID(_roomID);
			SetUserCount(_userCount);
		}
	}
}