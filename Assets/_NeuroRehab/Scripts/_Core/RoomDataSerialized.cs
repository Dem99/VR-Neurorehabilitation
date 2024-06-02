namespace NeuroRehab.Core {
	[System.Serializable]
	public class RoomDataSerialized {
		public string roomID;
		public string roomName;
		public int userCount;

		public RoomDataSerialized(RoomData roomData) {
			roomID = roomData.roomID;
			roomName = roomData.roomName;
			userCount = roomData.users.Count;
		}
	}
}