using System;
using UnityEngine;

namespace NeuroRehab.Core {
	[Serializable]
	public class OffsetSettingsSerialized {
		[SerializeField] private bool miniMenusOffsetSettingsInitialized = false;
		[SerializeField] private bool staticMenusOffsetSettingsInitialized = false;
		[SerializeField] private Vector3 miniMenuTransformOffset;
		[SerializeField] private Vector3 staticMenuTransformOffset;

		public bool MiniMenusOffsetInitialized { get => miniMenusOffsetSettingsInitialized; }
		public bool StaticMenusOffsetInitialized { get => staticMenusOffsetSettingsInitialized; }
		public Vector3 MiniMenuTransformOffset { get => miniMenuTransformOffset; }
		public Vector3 StaticMenuTransformOffset { get => staticMenuTransformOffset; }

		public OffsetSettingsSerialized(bool miniMenusOffsetSettingsInitialized, bool staticMenusOffsetSettingsInitialized, Vector3 miniMenuTransformOffset, Vector3 staticMenuTransformOffset) {
			this.miniMenusOffsetSettingsInitialized = miniMenusOffsetSettingsInitialized;
			this.staticMenusOffsetSettingsInitialized = staticMenusOffsetSettingsInitialized;
			this.miniMenuTransformOffset = miniMenuTransformOffset;
			this.staticMenuTransformOffset = staticMenuTransformOffset;
		}
	}
}