using System.Collections.Generic;
using UnityEngine;

using NeuroRehab.Core;
using NeuroRehab.ScriptObjects;

namespace NeuroRehab.Utility {
	/// <summary>
	/// Hides object if not in the list of allowed roles. Does not deactivate object, only hides it.
	/// </summary>
	public class RoleBasedVisibility : MonoBehaviour {
		[Header("Scriptable objects")]
		[SerializeField] private RoleSettings roleSettings;

		[Header("Dependencies")]
		[SerializeField] private List<UserRole> allowedRoles = new();

		void Start() {
			if (!allowedRoles.Contains(roleSettings.CharacterRole)) {
				if (gameObject.TryGetComponent(out Canvas canvas)) {
					canvas.enabled = false;
				}
				if (gameObject.TryGetComponent(out CanvasGroup canvasGroup)) {
					canvasGroup.interactable = false;
					canvasGroup.blocksRaycasts = false;
					canvasGroup.alpha = 0f;
				}
				if (gameObject.TryGetComponent(out Renderer renderer)) {
					renderer.enabled = false;
				}
			}
		}
	}
}