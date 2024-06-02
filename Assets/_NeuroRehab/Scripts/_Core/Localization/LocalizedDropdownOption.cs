using System;
using UnityEngine;
using UnityEngine.Localization;

namespace NeuroRehab.Core {

	[Serializable]
	public class LocalizedDropdownOption {
		public LocalizedString text;
		public LocalizedSprite sprite;
		public Texture2D simpleSprite;
	}
}