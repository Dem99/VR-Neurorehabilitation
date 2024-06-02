using UnityEngine;
using Mirror;

namespace NeuroRehab.Avatar {

	/// <summary>
	/// Same as 'AvatarWalkingController', but used to sync walking value over network, so that we can animate avatars on 'nonLocalPlayer'.
	/// </summary>
	public class NetworkAvatarWalkingController : NetworkBehaviour {

		[SerializeField] private AvatarModelManager avatarModelManager;

		[SerializeField] private Animator maleAnimator;
		[SerializeField] private Animator femaleAnimator;
		private Animator animator;

		[SerializeField] [SyncVar(hook = nameof(ChangeIsWalking))]
		private bool isWalking;

		[SerializeField] [SyncVar(hook = nameof(ChangeWalkingSpeed))]
		private float walkingSpeed;

		[SerializeField] [SyncVar(hook = nameof(ChangeIsStrafing))]
		private bool isStrafing;

		[SerializeField] [SyncVar(hook = nameof(ChangeStrafeSpeed))]
		private float strafeDirection;

		private void Start() {
			InitAnimator();
		}

		private void InitAnimator() {
			if (avatarModelManager.IsFemale) {
				animator = femaleAnimator;
			} else {
				animator = maleAnimator;
			}
		}

		public void HandleMovement(bool _isWalking, bool _isStrafing, float _walkingSpeed, float _strafeDirection) {
			isWalking = _isWalking;
			isStrafing = _isStrafing;
			walkingSpeed = _walkingSpeed;
			strafeDirection = _strafeDirection;

			CMDUpdateIsWalking(isWalking);
			CMDUpdateIsStrafing(isStrafing);
			CMDUpdateWalkingSpeed(walkingSpeed);
			CMDUpdateStrafeDirection(strafeDirection);
		}

		/// <summary>
		/// Setting Animator variables
		/// </summary>
		private void AnimateLegs() {
			if (isLocalPlayer) {
				return;
			}
			if (animator == null) {
				InitAnimator();
			}

			if (isWalking) {
				animator.SetBool("isWalking", true);

				animator.SetBool("isStrafingRight", false);
				animator.SetBool("isStrafingLeft", false);

				animator.SetFloat("animationSpeed", walkingSpeed);
			} else if (isStrafing) {
				animator.SetBool("isWalking", false);
				animator.SetFloat("strafeSpeed", 1);

				animator.SetBool("isStrafingRight", strafeDirection == 1f);
				animator.SetBool("isStrafingLeft", strafeDirection == -1f);
			}
		}

		/// <summary>
		/// Stoppings animations on animator
		/// </summary>
		private void StopAnimateLegs() {
			if (isLocalPlayer) {
				return;
			}
			if (animator == null) {
				InitAnimator();
			}

			if (!isWalking) {
				animator.SetBool("isWalking", false);
				animator.SetFloat("animationSpeed", 0);
			}

			if (!isStrafing) {
				animator.SetBool("isStrafingRight", false);
				animator.SetBool("isStrafingLeft", false);
				animator.SetFloat("strafeSpeed", 0);
			}
			AnimateLegs();
		}

		private void ChangeIsWalking(bool _old, bool _new) {
			if (_new == true) {
				AnimateLegs();
			} else {
				StopAnimateLegs();
			}
		}

		private void ChangeWalkingSpeed(float _old, float _new) {
			if (_new != _old) {
				AnimateLegs();
			}
		}

		private void ChangeIsStrafing(bool _old, bool _new) {
			if (_new == true) {
				AnimateLegs();
			} else {
				StopAnimateLegs();
			}
		}

		private void ChangeStrafeSpeed(float _old, float _new) {
			if (_new != _old) {
				AnimateLegs();
			}
		}

		[Command]
		public void CMDUpdateIsWalking(bool _isWalking) {
			isWalking = _isWalking;
		}

		[Command]
		public void CMDUpdateIsStrafing(bool _isStrafing) {
			isStrafing = _isStrafing;
		}
		[Command]
		public void CMDUpdateWalkingSpeed(float _walkingSpeed) {
			walkingSpeed = _walkingSpeed;
		}
		[Command]
		public void CMDUpdateStrafeDirection(float _direction) {
			strafeDirection = _direction;
		}
	}
}