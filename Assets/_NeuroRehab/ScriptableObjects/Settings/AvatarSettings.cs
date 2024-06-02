using UnityEngine;
using System;

using NeuroRehab.Interfaces;

namespace NeuroRehab.ScriptObjects {

	[CreateAssetMenu(menuName = "ScriptObjects/Settings/Avatar")]
	public class AvatarSettings : ScriptableObject, IAvatarChanger{
		[SerializeField] private bool isFemale;
		[SerializeField] private int avatarNumber;
		[SerializeField] private float sizeMultiplier;
		[SerializeField] private float offsetDistance;

		[SerializeField, NonReorderable] private GameObject[] avatarMalePrefabs;
		[SerializeField, NonReorderable] private GameObject[] avatarFemalePrefabs;


		public event Action OnAvatarChange;
		public event Action OnResetHeight;

		public bool IsFemale { get => isFemale;}
		public int AvatarNumber { get => avatarNumber;}
		public float SizeMultiplier { get => sizeMultiplier; set => sizeMultiplier = value; }
		public float OffsetDistance { get => offsetDistance; set => offsetDistance = value; }

		public void SetAvatarSettings(bool _isFemale, int _avatarNumber) {
			isFemale = _isFemale;
			avatarNumber = _avatarNumber;

			OnAvatarChange?.Invoke();
		}

		public void Init(bool isFemale, int avatarNumber, float sizeMultiplier, float offsetDistance) {
			this.isFemale = isFemale;
			this.avatarNumber = avatarNumber;
			this.sizeMultiplier = sizeMultiplier;
			this.offsetDistance = offsetDistance;
		}

		public (bool, GameObject) GetAvatarInfo(bool _isFemale, int _index) {
			if (_isFemale) {
				return (_isFemale, avatarFemalePrefabs[_index]);
			}
			return (_isFemale, avatarMalePrefabs[_index]);
		}

		public void ResetHeight() {
			OnResetHeight?.Invoke();
		}

		public (bool, GameObject) GetAvatarInfo() {
			return GetAvatarInfo(isFemale, avatarNumber);
		}

		public float GetSizeMulti() {
			return sizeMultiplier;
		}

		public float GetOffsetDist() {
			return offsetDistance;
		}
	}
}