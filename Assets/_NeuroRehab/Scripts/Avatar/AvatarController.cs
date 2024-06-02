using UnityEngine;
using AYellowpaper;

using NeuroRehab.ScriptObjects;
using NeuroRehab.Interfaces;
using NeuroRehab.Core;
using RootMotion.FinalIK;


namespace NeuroRehab.Avatar {
	/// <summary>
	/// Class used to align IK target with VR targets in Avatar (hands, head).
	/// </summary>
	[System.Serializable]
	public class MapTransforms {
		public bool followVRTarget = true;
		public Transform vrTarget;
		public Transform ikTarget;
		public Vector3 positionOffset;
		private Vector3 actualPosOffset;

		public Vector3 rotationOffset;
		private Vector3 actualRotOffset;

		/// <summary>
		///
		/// </summary>
		public void MapTargets(){
			if (!followVRTarget || vrTarget == null || ikTarget == null) {
				return;
			}
			ikTarget.SetPositionAndRotation(vrTarget.TransformPoint(actualPosOffset), vrTarget.rotation * Quaternion.Euler(actualRotOffset)); // transformPoint from somewhere acts as if it was offset
		}

		public void MapTargetsDebug(){
			if (!followVRTarget) {
				return;
			}
			ikTarget.SetPositionAndRotation(vrTarget.TransformPoint(positionOffset), vrTarget.rotation * Quaternion.Euler(rotationOffset));
		}

		/// <summary>
		/// Setting multiplier of offsets.
		/// We use extra variable so that we don't have to recalculate scale every update, just once at the beginning
		/// </summary>
		/// <param name="multiplier"></param>
		/// <param name="ignoreMulti"></param>
		public void SetMulti(float multiplier, bool ignoreMulti = false) {
			if (!ignoreMulti) {
				actualPosOffset = positionOffset;
			} else {
				actualPosOffset = positionOffset * multiplier;
			}
			actualRotOffset = rotationOffset;
		}
	}

	/// <summary>
	/// Used for positioning avatar and IK targets. Calculates scale and applies them to offsets.
	/// </summary>
	public class AvatarController : MonoBehaviour {
		private readonly CustomDebug customDebug = new("AvatarController");
		[SerializeField] private AvatarSettings avatarSettings;
		[SerializeField] private InterfaceReference<IAvatarChanger> avatarChanger;

		[SerializeField] private MeshRenderer rightArmRangeMarker;
		[SerializeField] private MeshRenderer leftArmRangeMarker;

		[SerializeField] private MapTransforms head;
		[SerializeField] private MapTransforms headSpine;
		[SerializeField] private MapTransforms leftHand;
		[SerializeField] private MapTransforms rightHand;
		[SerializeField] private float turnSmoothness;
		[SerializeField] private Transform headTarget;
		private Vector3 headOffset;
		[SerializeField] private Vector3 originHeadOffset;

		[SerializeField] private float referenceHeight = 1.725f;
		private readonly float standardizedReferenceHeight = 1.725f;

		[SerializeField] private bool debug;

		private PosRotMapping initialLeftPosRot;
		private PosRotMapping initialRightPosRot;

		public Transform rightController;
		public Transform leftController;
		public VRIK ikComponent;

		private bool m_turnWholeAvatar = true;
		private bool sizeInitialized = false;
		private float sizeMultiplier;
		private bool sizePreset;
		

		private void Awake() {
			if (leftHand.ikTarget != null) {
				initialLeftPosRot = new PosRotMapping(leftHand.ikTarget.transform.localPosition, leftHand.ikTarget.transform.localRotation.eulerAngles);
			}

			if (rightHand.ikTarget != null) {
				initialRightPosRot = new PosRotMapping(rightHand.ikTarget.transform.localPosition, rightHand.ikTarget.transform.localRotation.eulerAngles);
			}

			if (avatarChanger == null) {
				throw new System.Exception($"Wrong object for 'avatarChangerRef' on object: '{gameObject.name}'");
			}
		}

		private void OnEnable() {
			avatarChanger.Value.OnResetHeight += ResetHeight;

			if (!sizePreset && !sizeInitialized) {
				sizeMultiplier = CalculateSizeMultiplier();

				sizeInitialized = true;
			}

			InitializeValues();

			transform.position = headTarget.position + new Vector3(0, headOffset.y, 0);
			transform.position += new Vector3(transform.forward.x * headOffset.x, 0, transform.forward.z * headOffset.z);

			transform.forward = Vector3.ProjectOnPlane(headTarget.forward, Vector3.up).normalized;

		}

		private void OnDisable() {
			avatarChanger.Value.OnResetHeight -= ResetHeight;
		}

		public void SetSizeMulti(float sizeMulti) {
			sizePreset = true;
			sizeMultiplier = sizeMulti;
		}

		public MeshRenderer GetArmRangeRenderer(bool isLeft) {
			return isLeft ? leftArmRangeMarker : rightArmRangeMarker;
		}

		private void ResetHeight() {
			sizeMultiplier = CalculateSizeMultiplier();

			InitializeValues();
		}

		private void InitializeValues() {
			if (gameObject.activeInHierarchy) {
				avatarSettings.SizeMultiplier = sizeMultiplier; // we don't read this value, only store it for avatar when we connect to server
			}
			transform.localScale = new Vector3(sizeMultiplier, sizeMultiplier, sizeMultiplier);
			headOffset = originHeadOffset * sizeMultiplier;

		/*
		Debug.Log(gameObject.name);
		Debug.Log("head offset " +headOffset);
		Debug.Log("size multi " + sizeMultiplier);
		*/
		head.SetMulti(sizeMultiplier);
		headSpine.SetMulti(sizeMultiplier);
		leftHand.SetMulti(sizeMultiplier, true);
		rightHand.SetMulti(sizeMultiplier, true);
		head.MapTargets();
		headSpine.MapTargets();
		leftHand.MapTargets();
		rightHand.MapTargets();

	}

	void Update() {
	}

		public void RotateAvatar() {
			transform.rotation = Quaternion.Euler(0, 0, 0);

			transform.forward = Vector3.ProjectOnPlane(head.vrTarget.forward, Vector3.up).normalized;
		}

		public float CalculateSizeMultiplier() {
			return Mathf.Round(head.vrTarget.TransformPoint(Vector3.zero).y / referenceHeight * 1000) / 1000;
		}

		/// <summary>
		/// we use this to calculate how much should avatar be scaled in "multiplayer" settings
		/// </summary>
		/// <returns></returns>
		public float CalculateStandardizedSizeMultiplier() {
			return Mathf.Round(standardizedReferenceHeight / referenceHeight * 1000) / 1000;
		}

		/// <summary>
		/// Initial ikTarget position, which is used when arm is not resting. This method is used after changing arms, so that non resting position is not where arm was before it was changed into active animated arm, but next to 'body'
		/// </summary>
		public void ResetHandIKTargets() {
			if (leftHand.ikTarget != null) {
				leftHand.ikTarget.localRotation = Quaternion.Euler(initialLeftPosRot.Rotation);
				leftHand.ikTarget.localPosition = initialLeftPosRot.Position;
				// Debug.Log("Left hand reset position " + leftHand.followVRTarget);
			}
			if (rightHand.ikTarget != null) {
				rightHand.ikTarget.localRotation = Quaternion.Euler(initialRightPosRot.Rotation);
				rightHand.ikTarget.localPosition = initialRightPosRot.Position;
				// Debug.Log("Right hand reset position "  + rightHand.followVRTarget);
			}
		}

		public void SetAvatarTurn(bool avatarTurn) {
			m_turnWholeAvatar = !avatarTurn;
		}

		public void SetActiveArms(bool leftFollowVrTarget, bool rightFollowVrTarget, Transform rightRestTarget, Transform leftRestTarget) {
			SetVRIKArm(ikComponent.solver.leftArm, leftFollowVrTarget, leftController, leftRestTarget);
			SetVRIKArm(ikComponent.solver.rightArm, rightFollowVrTarget, rightController, rightRestTarget);

		}

		private void SetVRIKArm(IKSolverVR.Arm arm, bool followTarget, Transform controller, Transform restTarget) {
			arm.target = followTarget ? controller : restTarget;
			arm.positionWeight = followTarget ? 1f : 0f;
			arm.rotationWeight = followTarget ? 1f : 0f;
		}
	}
}