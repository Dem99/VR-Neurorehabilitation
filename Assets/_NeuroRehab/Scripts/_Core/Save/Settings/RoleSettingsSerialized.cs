using System;
using UnityEngine;

namespace NeuroRehab.Core {
	[Serializable]
	public class RoleSettingsSerialized {
		[SerializeField] private UserRole characterRole;

		public RoleSettingsSerialized(UserRole characterRole) {
			this.characterRole = characterRole;
		}

		public UserRole CharacterRole { get => characterRole; }

	}

}
