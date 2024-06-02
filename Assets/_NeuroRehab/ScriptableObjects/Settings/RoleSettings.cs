using UnityEngine;

using NeuroRehab.Core;

namespace NeuroRehab.ScriptObjects {

	[CreateAssetMenu(menuName = "ScriptObjects/Settings/Role")]
	public class RoleSettings : ScriptableObject {

		[SerializeField] private UserRole m_characterRole;

		public void Init(UserRole characterRole) {
			CharacterRole = characterRole;
		}

		public UserRole CharacterRole {
			get => m_characterRole;
			set => m_characterRole = value;
		}
	}
}