using UnityEngine;

using NeuroRehab.Core;
using NeuroRehab.ScriptObjects;

namespace NeuroRehab.Managers {
	/// <summary>
	/// Used for XR Rigs in Offline setup, where we don't need to setup all elements.
	/// </summary>
	public class SimpleRigManager : MonoBehaviour {
		[SerializeField] private CustomXRSettings xrSettings;
		[SerializeField] private MenuHelperSO menuHelper;

		[SerializeField] private Transform offset;

		[SerializeField] private Camera localCamera;

		private Vector3 initOffsetPos;

		private void Awake() {
			initOffsetPos = Vector3.zero;
			if (offset != null) {
				initOffsetPos = offset.position;
			}
		}

		private void OnEnable() {
			menuHelper.RenderingCamera = localCamera;

			xrSettings.OnHmdChange += ChangeHMDOffset;
		}

		private void OnDisable() {
			xrSettings.OnHmdChange -= ChangeHMDOffset;
		}

		/// <summary>
		/// This is used in offline scene for simple rig (this rig does not have CharacterManager component)
		/// </summary>
		private void ChangeHMDOffset() {
			if (offset == null) {
				return;
			}

			if (xrSettings.HmdType == HMDType.Other || xrSettings.HmdType == HMDType.Android) {
				offset.position = new Vector3(0f, 0f, 0f);
			} else {
				offset.position = initOffsetPos;
			}
		}
	}
}