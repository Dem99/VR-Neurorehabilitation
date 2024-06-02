using Mirror;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

using NeuroRehab.ScriptObjects;
using NeuroRehab.Interfaces;
using NeuroRehab.Core;

namespace NeuroRehab.Drag {
	public class TargetDraggable : NetworkBehaviour, IDraggable, ITargetDisable {
		[Header("Scriptable objects")]
		[SerializeField] private AnimationSettingsSO animationSettings;

		[Header("Dependencies")]
		[SerializeField] private Rigidbody _rigidbody;
		[SerializeField] private XRGrabInteractable XRGrabInteractable;
		[SerializeField] private SingleUnityLayer targetLayer;
		[SerializeField] private bool showRange;

		private bool itemPickedUp = false;

		public bool ItemPickedUp { get => itemPickedUp; }
		public bool ShowRange { get => showRange; }

		public void OnStartDrag() {
			_rigidbody.useGravity = false;
		}

		public void OnStopDrag() {
			_rigidbody.useGravity = true;
			_rigidbody.velocity = Vector3.zero;
		}

		[Command]
		public void CmdDisableDrag() {
			itemPickedUp = true;
			RpcDisableDrag();
		}

		[ClientRpc]
		public void RpcDisableDrag() {
			// Debug.Log($"{gameObject.name} __ {CharacterManager.localClientInstance.netId} __ {netIdentity.isOwned}");
			if (!netIdentity.isOwned) {
				gameObject.layer = 0;
				XRGrabInteractable.enabled = false;
			}
		}

		[Command]
		public void CmdEnableDrag() {
			EnableDragWrapper();
		}

		[Server]
		public void EnableDragWrapper() {
			itemPickedUp = false;
			RpcEnableDrag();
		}

		[ClientRpc]
		public void RpcEnableDrag() {
			if (!netIdentity.isOwned) {
				gameObject.layer = targetLayer.LayerIndex;
				XRGrabInteractable.enabled = true;
			}
		}
	}
}