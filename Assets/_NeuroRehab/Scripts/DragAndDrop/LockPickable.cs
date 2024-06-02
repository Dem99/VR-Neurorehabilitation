using UnityEngine;

using NeuroRehab.ScriptObjects;
using NeuroRehab.Interfaces;
using NeuroRehab.Core;

namespace NeuroRehab.Drag {
	public class LockPickable : MonoBehaviour, IPickable {
		[Header("Scriptable objects")]
		[SerializeField] private AnimationSettingsSO animationSettings;

		[Header("Dependencies")]
		[SerializeField] private TargetUtility targetUtility;

		public void OnPickUp() {
			animationSettings.LockPickUp(transform, targetUtility.customTargetPos.transform);
		}

		public void OnRelease() {
			// do nothing
		}
	}

}