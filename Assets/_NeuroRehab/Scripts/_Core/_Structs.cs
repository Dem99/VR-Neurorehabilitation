using Mirror;
using System;
using UnityEngine;

namespace NeuroRehab.Core {
	/// <summary>
	/// Structure used as message to inform server what settings to apply to User Prefab when spawning a character
	/// </summary>
	public struct CharacterMessage : NetworkMessage {
		public UserRole role;
		public HMDType hmdType;
		public ControllerType controllerType;
		public bool isFemale;
		public int avatarNumber;
		public float sizeMultiplier;
		public float offsetDistance;
		public bool isXRActive;

		public string roomID;
	}

	[Serializable]
	public struct LanguageOption {
		public LanguageEnum language;
		public string languageReadable;
		public Sprite sprite;
	}

	[Serializable]
	public struct ServersData {
		public string serverIp;
		public int serverPort;
	}

	public struct ServerInfo {
		public string serverVersion;
	}

	public struct RoomIDJsonBody {
		public string roomID;
	}

	public struct ResolutionHelper {
		public int width;
		public int height;

		public ResolutionHelper(int _w, int _h) {
			width = _w;
			height = _h;
		}
	}
}
