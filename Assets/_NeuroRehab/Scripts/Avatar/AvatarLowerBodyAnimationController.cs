using UnityEngine;
using AYellowpaper;

using NeuroRehab.ScriptObjects;
using NeuroRehab.Interfaces;
using NeuroRehab.Core;

namespace NeuroRehab.Avatar {
	/// <summary>
	/// Component that calculates whether user is crouching or not and calculates position for feet.
	/// </summary>
	public class AvatarLowerBodyAnimationController : MonoBehaviour {
		private readonly CustomDebug customDebug = new("LowerBodyController");
		[SerializeField] private AvatarSettings avatarSettings;
		[SerializeField] private InterfaceReference<IAvatarChanger> avatarChanger;

		[SerializeField] private Animator animator;

		[SerializeField] [Range(0f, 1f)] private float leftFootPositionWeight;
		[SerializeField] [Range(0f, 1f)] private float rightFootPositionWeight;

		[SerializeField] [Range(0f, 1f)] private float leftFootRotationWeight;
		[SerializeField] [Range(0f, 1f)] private float rightFootRotationWeight;

		[SerializeField] private Vector3 footOffset;

		[SerializeField] private Vector3 raycastLeftOffset;

		[SerializeField] private Vector3 raycastRightOffset;

		[SerializeField] private LayerMask groundLayer;

		[SerializeField] private Transform offsetTransform;

		private float offsetDistance;
		private bool offsetDistInit;

		private float groundOffset;
		private readonly float groundOffsetConst = 0.15f;

		private void Awake() {
			if (avatarChanger == null) {
				throw new System.Exception($"Wrong object for 'avatarChangerRef' on object: '{gameObject.name}'");
			}
		}

		private void OnEnable() {
			if (!offsetDistInit) {
				offsetDistance = offsetTransform.TransformPoint(Vector3.zero).y;
			}

			RecalculateValues();
			avatarChanger.Value.OnResetHeight += ResetHeight;
		}

		private void OnDisable() {
			avatarChanger.Value.OnResetHeight -= ResetHeight;
		}

		public void SetGroundOffset(float groundOffsetDistance) {
			offsetDistance = groundOffsetDistance;
			offsetDistInit = true;
		}

		private void ResetHeight() {
			offsetDistance = offsetTransform.TransformPoint(Vector3.zero).y;
			RecalculateValues();
		}

		private void RecalculateValues() {
			/*
			Debug.Log("offset dist " + offsetDistance);
			Debug.Log(groundOffset);
			*/
			avatarSettings.OffsetDistance = offsetDistance;
			/*
			internalFootOffset = footOffset * avatarController.sizeMultiplier;
			internalRaycastLeftOffset = raycastLeftOffset * avatarController.sizeMultiplier;
			internalRaycastRightOffset = raycastRightOffset * avatarController.sizeMultiplier;
			*/
			groundOffset = groundOffsetConst * avatarChanger.Value.GetSizeMulti();
		}

		// we use this to determine whether character is crouching or not
		// if yes, we move feet to be on the ground level so that avatar appears to be crouching
		private void OnAnimatorIK(int layerIndex) {
			bool isCrouching = Physics.Raycast(offsetTransform.position, Vector3.down, offsetDistance - groundOffset, groundLayer.value);

			if (!isCrouching) {
				return;
			}
			Vector3 leftFootPosition = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
			Vector3 rightFootPosition = animator.GetIKPosition(AvatarIKGoal.RightFoot);


			bool isLeftFootDown = Physics.Raycast(leftFootPosition + raycastLeftOffset, Vector3.down, out RaycastHit leftFootHit);
			bool isRightFootDown = Physics.Raycast(rightFootPosition + raycastRightOffset, Vector3.down, out RaycastHit rightFootHit);

			CalculateFoot(isLeftFootDown, leftFootHit, AvatarIKGoal.LeftFoot, leftFootPositionWeight, leftFootRotationWeight);
			CalculateFoot(isRightFootDown, rightFootHit, AvatarIKGoal.RightFoot, rightFootPositionWeight, rightFootRotationWeight);
		}

		private void CalculateFoot(bool isFootDown, RaycastHit footHit, AvatarIKGoal goal, float footPositionWeight, float footRotationWeight) {
			if(!isFootDown) {
				animator.SetIKPositionWeight(goal, 0f);
				return;
			}

			animator.SetIKPositionWeight(goal, footPositionWeight);
			animator.SetIKPosition(goal, footHit.point + footOffset);

			Quaternion footRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, footHit.normal), footHit.normal);
			animator.SetIKRotationWeight(goal, footRotationWeight);
			animator.SetIKRotation(goal, footRotation);
		}
	}
}