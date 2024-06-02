using System;
using UnityEngine;

namespace NeuroRehab.Interfaces {
	public interface IAvatarChanger {
		public event Action OnAvatarChange;
		public event Action OnResetHeight;
		public (bool, GameObject) GetAvatarInfo();
		public float GetSizeMulti();
		public float GetOffsetDist();
	}
}