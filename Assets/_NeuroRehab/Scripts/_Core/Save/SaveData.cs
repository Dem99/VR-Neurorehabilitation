using System.Collections.Generic;
using UnityEngine;

namespace NeuroRehab.Core {

	/// <summary>
	/// https://www.youtube.com/watch?v=uD7y4T4PVk0
	/// </summary>
	[System.Serializable]
	public class SaveData {
		[SerializeField] private AvatarSettingsSerialized avatarSettings;
		[SerializeField] private AudioSettingsSerialized audioSettings;
		[SerializeField] private GraphicsSettingsSerialized graphicsSettings;
		[SerializeField] private GeneralSettingsSerialized generalSettings;
		[SerializeField] private RoleSettingsSerialized roleSettings;
		[SerializeField] private OffsetSettingsSerialized offsetSettings;
		[SerializeField] private string ipAddress;
		[SerializeField] private int websocketPort;
		[SerializeField] private int restPort;
		[SerializeField] private string roomID;
		[SerializeField] private LanguageEnum language;

		[SerializeField] private string settingsVersion;
		
		[SerializeField] private List<ServersData> servers;

		public AudioSettingsSerialized AudioSettings { get => audioSettings; set => audioSettings = value; }
		public AvatarSettingsSerialized AvatarSettings { get => avatarSettings; set => avatarSettings = value; }
		public GraphicsSettingsSerialized GraphicsSettings { get => graphicsSettings; set => graphicsSettings = value; }
		public RoleSettingsSerialized RoleSettings { get => roleSettings; set => roleSettings = value; }
		public OffsetSettingsSerialized OffsetSettings { get => offsetSettings; set => offsetSettings = value; }
		public GeneralSettingsSerialized GeneralSettings { get => generalSettings; set => generalSettings = value; }
		public string IpAddress { get => ipAddress; set => ipAddress = value; }
		public int WebsocketPort { get => websocketPort; set => websocketPort = value; }
		public int RestPort { get => restPort; set => restPort = value; }
		public string SettingsVersion { get => settingsVersion; set => settingsVersion = value; }
		public string RoomID { get => roomID; set => roomID = value; }
		public LanguageEnum Language { get => language; set => language = value; }
		public List<ServersData> Servers { get => servers; set => servers = value; }
		
		public string ToJson() {
			return JsonUtility.ToJson(this);
		}

		public void LoadFromJson(string json) {
			JsonUtility.FromJsonOverwrite(json, this);
		}
	}
}