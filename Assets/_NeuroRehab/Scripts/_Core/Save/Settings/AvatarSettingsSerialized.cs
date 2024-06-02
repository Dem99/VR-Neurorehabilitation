using System;
using UnityEngine;

namespace NeuroRehab.Core {
	[Serializable]
	public class AvatarSettingsSerialized {
		[SerializeField] private bool isFemale;
		[SerializeField] private int avatarNumber;
		[SerializeField] private float sizeMultiplier;
		[SerializeField] private float offsetDistance;

		public bool IsFemale { get => isFemale; }
		public int AvatarNumber { get => avatarNumber; }
		public float SizeMultiplier { get => sizeMultiplier; }
		public float OffsetDistance { get => offsetDistance; }

		public AvatarSettingsSerialized(bool isFemale, int avatarNumber, float sizeMultiplier, float offsetDistance) {
			this.isFemale = isFemale;
			this.avatarNumber = avatarNumber;
			this.sizeMultiplier = sizeMultiplier;
			this.offsetDistance = offsetDistance;
		}
	}
}