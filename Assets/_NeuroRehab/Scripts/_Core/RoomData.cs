using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace NeuroRehab.Core {

	[Serializable]
	public class RoomData {
		public string roomID;
		public string roomName;
		public bool loadedScene;
		public Scene scene;

		public List<string> users = new();

		public RoomData(RoomDataSerialized roomData) {
			roomID = roomData.roomID;
			roomName = roomData.roomName;
		}
	}

	/// <summary>
	/// Custom comparer for RoomData. https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.distinct?view=net-7.0
	/// </summary>
	public class RoomDataComparer : IEqualityComparer<RoomData> {
		public bool Equals(RoomData x, RoomData y) {
			//Check whether the compared objects reference the same data.
			if (ReferenceEquals(x, y)) return true;

			//Check whether any of the compared objects is null.
			if (x is null || y is null)
				return false;

			//Check whether the products' properties are equal.
			return x.roomID == y.roomID;
		}

		// If Equals() returns true for a pair of objects
		// then GetHashCode() must return the same value for these objects.
		public int GetHashCode(RoomData roomData) {
			//Check whether the object is null
			if (roomData is null) return 0;

			//Calculate the hash code for the roomData
			return roomData.roomID.GetHashCode();
		}
	}

}