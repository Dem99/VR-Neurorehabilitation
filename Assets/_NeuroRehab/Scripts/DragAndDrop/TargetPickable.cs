using UnityEngine;

using NeuroRehab.ScriptObjects;
using NeuroRehab.Interfaces;

namespace NeuroRehab.Drag {
	public class TargetPickable : MonoBehaviour, IPickable {
		[Header("Scriptable objects")]
		[SerializeField] private AnimationSettingsSO animationSettings;

		// [Header("Dependencies")]

		public void OnPickUp() {
			animationSettings.TargetPickUp(transform);
		}

		public void OnRelease() {
			animationSettings.TargetRelease(transform);
		}
	}
}