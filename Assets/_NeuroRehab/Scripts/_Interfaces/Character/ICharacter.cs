using UnityEngine;

using NeuroRehab.Core;

namespace NeuroRehab.Interfaces {
	public interface ICharacter {
		public uint Id { get; }
		public bool IsLocalPlayer { get; }
		public bool IsLeftArmAnimated { get; }
		public bool IsArmFixed { get; set;}
		public UserRole UserRole { get; }
		public string RoomID { get; }
		public Camera LocalCamera { get; }

		public void ItemStartMove(Transform itemObj);

		public void ChangeAnimatedArm(bool _old, bool _new);
		public void FixArms(bool _old, bool _new);

		public void ShowArmRangeMarker();

		public void HideArmRangeMarker();
		public bool IsAnimationPlaying();

		public (bool, GameObject) GetAvatarInfo();

		public float GetSizeMulti();
		public float GetOffsetDist();

		public void SetAnimState(ArmAnimationState animationState);
		public bool StartAnimation(bool isShowcase, bool isTraining, Transform targetTransform, Transform lockTransform);
		public void StopAnimation();

		public bool IsTargetInRange(Vector3 position);
		public float GetArmLength();
		public float GetArmLengthWithSlack();
		public float GetTargetRangeFromShoulder(Vector3 targetPos);

		public bool RequestOwnership(GameObject obj);

		public void ProgressAnimationStep(ExerciseState state);
		public void SetAnimationState(ArmAnimationState m_animState);

		public void TeleportCharacter(Transform targetPosition, Transform lookTarget = null);
	}
}