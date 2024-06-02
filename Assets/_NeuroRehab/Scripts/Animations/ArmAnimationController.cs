using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using AYellowpaper;

using NeuroRehab.Core;
using NeuroRehab.ScriptObjects;
using NeuroRehab.Interfaces;
using RootMotion.FinalIK;

namespace NeuroRehab.Animation {
	/// <summary>
	/// Main Patient Arm Animation component. Contains methods for animating arms, switching active arms, lerps, etc.
	/// </summary>
	public class ArmAnimationController : MonoBehaviour {
		private bool isInitialized = false;
		private CustomDebug customDebug;
		[Header("Serialized objects")]
		[SerializeField] private RoleSettings roleSettings;
		[SerializeField] private AnimationSettingsSO animSettings;
		[SerializeField] private MessageEventsSO messageEvents;

		[Header("Dependencies")]
		[SerializeField] private InterfaceReference<ICharacter> charManager;

		[SerializeField] private bool isLeft;
		[SerializeField] private AnimationMapping animationMapping;

		[Header("Rigs")]
		[SerializeField] private Rig restArmRig;
		[SerializeField] private Rig armRig;
		[SerializeField] private Rig handRig;

		[Header("Arm objects")]
		[SerializeField] private Renderer[] armObjects;
		[SerializeField] private Renderer[] fakeArmObjects;
		[SerializeField] private TargetsHelper targetsHelper;

		[SerializeField] private Vector3 armRestOffset;

		[Header("Arm range and mirroring mappings objects")]
		[SerializeField] private MeshFilter armRangeMesh;
		[SerializeField] private float armRangeSlack = 0.01f;

		// --------------------------------------------------------
		private Transform targetObject;
		private Transform armRestHelperObject;
		private PosRotMapping originalArmRestPosRot;

		private SceneHelper sceneHelper;
		private ISceneObjectManager sceneObjectManager;

		private bool m_isArmResting = false;
		private ArmAnimationState m_animState;

		private Transform _mirror;
		private float armLength;

		// Coroutines
		private Coroutine armRestCoroutine;
		private Coroutine armAnimationCoroutine;

		private LerpUtility lerpUtility;
		private VRIK ikComponent;
		private IKSolverVR.Arm ikArm;
		private FullBodyBipedEffector effector;

		

		private void OnEnable() {
			if (sceneHelper == null) {
				sceneHelper = new(gameObject.scene);
				sceneObjectManager = sceneHelper.FindRootObject<ISceneObjectManager>();
			}
			ikComponent = GetComponent<VRIK>();

		
			armRig.gameObject.SetActive(true);
			handRig.gameObject.SetActive(true);
		}

		private void OnDisable() {
			armRig.gameObject.SetActive(false);
			handRig.gameObject.SetActive(false);
		}

		void Start() {
			customDebug = new(isLeft ? "ARM_LEFT" : "ARM_RIGHT");

		
			m_animState = ArmAnimationState.Stopped;

			Init();

			if (!isInitialized) {
				isInitialized = true;
				animationMapping.ResizeMappings(charManager.Value.GetSizeMulti());

				if(charManager.Value.UserRole == UserRole.Patient){
					if (isLeft) animationMapping.MirrorMappings(_mirror);
				}
			}
		}

		public void Init() {
			if (isInitialized) {
				return;
			}
			lerpUtility = new();

			armRestHelperObject = isLeft ? sceneObjectManager.Table.LeftArmRestHelper : sceneObjectManager.Table.RightArmRestHelper;
			ikArm = isLeft ? ikComponent.solver.leftArm : ikComponent.solver.rightArm;
			effector = isLeft ? FullBodyBipedEffector.LeftHand : FullBodyBipedEffector.RightHand;

			//ikArm.target = armRestHelperObject;
			
			_mirror = sceneObjectManager.MirrorPlane;

			armLength = CalculateArmLength();

		}
		void Update() {			
			if(m_animState == ArmAnimationState.Stopped){

				float distance = Vector3.Distance(transform.position, armRestHelperObject.position);
				// Calculate the IK weight using a distance-based curve
				float ikWeight = CalculateIKWeight(distance);

				SetArmWeights(ikArm, ikWeight);
			}
		}

		float CalculateIKWeight(float distance) {
			// Linear falloff starting from 0.8 meter
			return Mathf.Clamp01(2f - distance);
		}

		void SetArmWeights(IKSolverVR.Arm arm, float weight) {
			arm.positionWeight = weight;
			arm.rotationWeight = weight;
		}

		/// <summary>
		/// Coroutine for animation control. Refer to https://gamedevbeginner.com/coroutines-in-unity-when-and-how-to-use-them/ for more details
		/// </summary>
		/// <param name="isFakeAnimation"></param>
		/// <returns></returns>
		private IEnumerator StartArmAnimationCoroutine(bool isFakeAnimation, bool isPartOfTraining, Transform lockTransform) {
			float cupMoveDuration = animSettings.MoveDuration / 2;
			AnimationType animationType = animSettings.AnimType;
			List<PosRotMapping> currentAnimSetup = animSettings.CurrentSetup;

			// Smoothly move the object to its initial position
			List<(Transform, PosRotMapping, PosRotMapping)> transformsToLerp = new() {
				(targetObject, new PosRotMapping(targetObject), currentAnimSetup[0])
			};

			if (animationType == AnimationType.Key) {
				PosRotMapping endMapping = new(
					currentAnimSetup[1].Position + lockTransform.GetComponent<TargetUtility>().customTargetPos.transform.localPosition, 
					lockTransform.rotation
				);
				transformsToLerp.Add((lockTransform, new PosRotMapping(lockTransform), endMapping));
			}

			yield return StartCoroutine(lerpUtility.LerpTransforms(transformsToLerp, 1f));

			// Start the interaction with the appropriate hand
			InteractionSystem interactionSystem = GetComponent<InteractionSystem>();
			InteractionObject interactionObject = targetObject.GetComponent<InteractionObject>();

			interactionSystem.StartInteraction(effector, interactionObject, true);

			// Handling special case for Cup animation at the start
			if (animationType == AnimationType.Cup) {
				yield return StartCoroutine(MoveCupUpAndDown(currentAnimSetup[0], animSettings.WaitDuration, cupMoveDuration));
			}

			PosRotMapping previousMapping = currentAnimSetup[0].Clone();
			foreach (PosRotMapping mapping in currentAnimSetup.Skip(1)) {
				yield return StartCoroutine(lerpUtility.LerpTransform(targetObject, previousMapping, mapping, animSettings.MoveDuration));
				
				// Handling special case for Cup animation after initial movement
				if (animationType == AnimationType.Cup) {
					yield return StartCoroutine(MoveCupUpAndDown(mapping, animSettings.WaitDuration, cupMoveDuration));
				}
				previousMapping = mapping.Clone();
			}

			// Special handling for Key animation type
			if (animationType == AnimationType.Key) {
				yield return StartCoroutine(HandleKeyAnimation(currentAnimSetup[^1], currentAnimSetup[0], animSettings.WaitDuration, animSettings.KeyTurnDuration));
			}

			// Release the object after moving
			interactionSystem.StopInteraction(effector);

			StopAnimation(isPartOfTraining);
		}

		/// <summary>
		/// Coroutine - Animation control for stopping animation
		/// </summary>
		/// <returns></returns>
		private IEnumerator StopArmAnimationCoroutine(bool isPartOfTraining) {
			// Code to disable fake arm objects and enable real arm objects
			foreach (Renderer item in fakeArmObjects) {
				item.enabled = false;
			}
			foreach (Renderer item in armObjects) {
				item.enabled = true;
			}

			// If we are a patient and it's part of the training, inform the server to progress the animation step
			if (isPartOfTraining && charManager.Value.IsLocalPlayer && charManager.Value.UserRole == UserRole.Patient) {
				charManager.Value.ProgressAnimationStep(ExerciseState.EVAL);
			}

			// Handle end animation if there is a target object
			if (targetObject != null) {
				HandleEndAnimation();
			}

			// Update animation state to stopped
			m_animState = ArmAnimationState.Stopped;
			if (charManager.Value.IsLocalPlayer && charManager.Value.UserRole == UserRole.Patient) {
				charManager.Value.SetAnimationState(m_animState);
			}

			// Since we removed actual asynchronous operations, we just yield return null to keep it as a coroutine
			yield return null;
		}


		private void HandleEndAnimation() {
			// hopefully this solves the issue with target object clipping through table at the end of animation and falling
			if (charManager.Value.IsLocalPlayer && charManager.Value.UserRole == UserRole.Patient) {
				// targetObject.position += new Vector3(0, 0.005f, 0);

				if (targetObject.TryGetComponent(out ITargetDisable targetDisableInterface)) {
					targetDisableInterface?.CmdEnableDrag();
				}
			}

			// if this is a fake animation, we have to hide the fake object
			if (targetObject.name.Contains("fake")) {
				if (targetObject.TryGetComponent(out TargetUtility targetUtility)) {
					foreach (Renderer item in targetUtility.renderers) {
						item.enabled = false;
					}
				}
			} else {
				if (targetObject.TryGetComponent(out Rigidbody rb))
					rb.useGravity = true;
			}
		}

		private IEnumerator RestArmStartCoroutine() {
			// we use variable to save old rest position - this is used either when holding arm next to your body OR when therapist changes animated arm (and rest position is active)
			originalArmRestPosRot = new PosRotMapping(targetsHelper.armRestTarget.transform);

			PosRotMapping startMapping = new(targetsHelper.armRestTarget.transform);
			PosRotMapping endMapping = new(armRestHelperObject);
			endMapping.Position += armRestOffset;

			// we simply move arm rest target to it's position
			// just to make it clear, armRestTarget is the same object as Right/LeftHandTarget, we simply have it named differently for purpose of animation
			// the reason is so that we don't need another Rig just for rest animations
			yield return StartCoroutine(lerpUtility.LerpTransform(targetsHelper.armRestTarget.transform, startMapping, endMapping, animSettings.ArmMoveDuration));

			yield return StartCoroutine(AlignRestArmCoroutine());
		}

		private IEnumerator AlignRestArmCoroutine() {
			while (m_isArmResting) {
				targetsHelper.armRestTarget.transform.position = armRestHelperObject.position + armRestOffset;
				targetsHelper.armRestTarget.transform.rotation = armRestHelperObject.rotation;

				yield return null;
			}
		}

		private IEnumerator RestArmStopCoroutine() {
			PosRotMapping startMapping = new(targetsHelper.armRestTarget.transform);

			// we simply move arm rest target to it's original position
			// just to make it clear, armRestTarget is the same object as Right/LeftHandTarget, we simply have it named differently for purpose of animation
			// the reason is so that we don't need another Rig just for rest animations
			yield return StartCoroutine(lerpUtility.LerpTransform(targetsHelper.armRestTarget.transform, startMapping, originalArmRestPosRot, animSettings.ArmMoveDuration));
		}

		/// <summary>
		/// Coroutine for smooth cup movement, currently we move 0.20m up; Could be changed in the future via Therapist, but it seems to be good amount of movement up and down for now.
		/// </summary>
		/// <param name="currentAnimSetup"></param>
		/// <param name="waitDuration"></param>
		/// <param name="cupMoveDuration"></param>
		/// <returns></returns>
		private IEnumerator MoveCupUpAndDown(PosRotMapping currentAnimSetup, float waitDuration, float cupMoveDuration) {
			PosRotMapping tempCupMapping = currentAnimSetup.Clone();
			tempCupMapping.Position += new Vector3(0, 0.2f, 0);

			yield return StartCoroutine(lerpUtility.LerpTransform(targetObject, currentAnimSetup, tempCupMapping, cupMoveDuration));
			yield return new WaitForSeconds(waitDuration);
			yield return StartCoroutine(lerpUtility.LerpTransform(targetObject, tempCupMapping, currentAnimSetup, cupMoveDuration));
		}

		/// <summary>
		/// Coroutine to rotate the key and move key to it's initial position
		/// </summary>
		/// <param name="lastMapping"></param>
		/// <param name="initialMapping"></param>
		/// <param name="waitDuration"></param>
		/// <param name="keyTurnDuration"></param>
		/// <returns></returns>
		private IEnumerator HandleKeyAnimation(PosRotMapping lastMapping, PosRotMapping initialMapping, float waitDuration, float keyTurnDuration) {
			PosRotMapping tempMapping = lastMapping.Clone();
			// Theoretically both ways should work pretty well
			tempMapping.Rotate(-65, 0, 0);
			//tempMapping.rotation = (Quaternion.Euler(tempMapping.rotation) * Quaternion.Euler(targetObject.forward * 65)).eulerAngles;

			yield return new WaitForSeconds(waitDuration);
			yield return StartCoroutine(lerpUtility.LerpTransform(targetObject, lastMapping, tempMapping, keyTurnDuration));
			yield return new WaitForSeconds(waitDuration);
			yield return StartCoroutine(lerpUtility.LerpTransform(targetObject, tempMapping, lastMapping, keyTurnDuration));
			yield return new WaitForSeconds(waitDuration);
			yield return StartCoroutine(lerpUtility.LerpTransform(targetObject, lastMapping, initialMapping, animSettings.MoveDuration));
		}

		/// <summary>
		/// Coroutine to align Wrapper used for smooth aligning of objects
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		public IEnumerator AlignTransformWrapper(float duration) {
			float time = 0;
			while (time < duration) {
				targetsHelper.AlignTargetTransforms();
				time += Time.deltaTime;
				yield return null;
			}
			targetsHelper.AlignTargetTransforms();
		}

		/// <summary>
		/// Algorithm used to calculate FULL duration of animation with all it's wait times etc., so that main client knows how long to align transforms. We take into account that some positions are not in range
		/// </summary>
		/// <param name="_animationSettingsManager"></param>
		/// <param name="_waitDuration"></param>
		/// <param name="_keyTurnDuration"></param>
		/// <returns></returns>
		private float CalculateAnimationDuration(AnimationSettingsSO _animationSettingsManager, float _waitDuration, float _keyTurnDuration) {
			/*
				+ (currentAnimSetup.Count - 1) * _animationSettingsManager.moveDuration
				+ if (cup) (currentAnimSetup.Count * _animationSettingsManager.moveDuration) + currentAnimSetup.Count * _waitDuration // Up + Down movement
				+ if (key) (_waitDuration * 3) + (2 * _keyTurnDuration) + _animationSettingsManager.moveDuration
			 */
			List<PosRotMapping> currentAnimSetup = _animationSettingsManager.CurrentSetup;
			int animCount = currentAnimSetup.Count;
			foreach (PosRotMapping mapping in currentAnimSetup) {
				if (!IsTargetInRange(mapping.Position)) {
					animCount--;
				}
			}
			if (animCount == 0) {
				return 0f;
			}

			float duration = 0f;
			duration += (animCount - 1) * _animationSettingsManager.MoveDuration;
			if (_animationSettingsManager.AnimType == AnimationType.Cup) {
				duration += (animCount * _animationSettingsManager.MoveDuration) + animCount * _waitDuration;// Up + Down movement
			}
			if (_animationSettingsManager.AnimType == AnimationType.Key) {
				duration += (_waitDuration * 3) + (2 * _keyTurnDuration) + _animationSettingsManager.MoveDuration;
			}

			return duration;
		}

		/// <summary>
		/// Wrapper method used to set animState. Usable only on server!!
		/// </summary>
		public void SetAnimState(ArmAnimationState animationState) {
			m_animState = animationState;
		}

		public bool StartAnimation(bool isFakeAnimation, bool isPartOfTraining, Transform targetTransform, Transform lockTransform) {
			if (animSettings.AnimType == AnimationType.Off) {
				customDebug.LogWarning("No animation type specified");
				return false;
			}
			if (m_animState == ArmAnimationState.Playing) {
				customDebug.LogError("There is an animation running already");
				return false;
			}

			if (!CanAnimationStart()) {
				return false;
			}

			targetObject = targetTransform;

			if (targetObject == null) {
				string targetObjectName = animSettings.AnimType.ToString();
				customDebug.LogError($"Failed to find object: '{targetObjectName + (isFakeAnimation ? "_fake" : "")}'");
				return false;
			}

			// setup targetObject
			if (isFakeAnimation) {
				
				if (targetObject.TryGetComponent(out TargetUtility tu)) {
					foreach (Renderer item in tu.renderers) {
						item.enabled = true;
					}
				}	

				foreach (Renderer item in armObjects) {
					item.enabled = false;
				}
				foreach (Renderer item in fakeArmObjects) {
					item.enabled = true;
				}
			}

			/*
			if (targetObject.TryGetComponent(out TargetUtility targetUtility)) {
				targetsHelper.SetHelperObjects(targetUtility);
			} else {
				customDebug.LogError("Failed to retrieve target helper objects from TargetObj");
				return false;
			} */

			// We have to disable Gravity for the objects
			if (targetObject.TryGetComponent(out Rigidbody rb))
				rb.useGravity = false;

			m_animState = ArmAnimationState.Playing;
			if (charManager.Value.IsLocalPlayer && charManager.Value.UserRole == UserRole.Patient) {
				charManager.Value.SetAnimationState(m_animState);
			}

			// https://gamedevbeginner.com/coroutines-in-unity-when-and-how-to-use-them/
			armAnimationCoroutine = StartCoroutine(StartArmAnimationCoroutine(isFakeAnimation, isPartOfTraining, lockTransform));
			return true;
		}

		public void StopAnimation(bool informServer = false) {
			if (m_animState == ArmAnimationState.Stopped || charManager.Value.UserRole != UserRole.Patient) {
				return;
			}
			// We don't have to set the positions of targets here, because we're simply releasing the hand grip + moving arm to relaxed position
			// all the movements are done using rig weights
			if (armAnimationCoroutine != null) {
				StopCoroutine(armAnimationCoroutine);
			}
			StartCoroutine(StopArmAnimationCoroutine(informServer));
		}

		private bool CanAnimationStart() {
			if (charManager.Value.UserRole != UserRole.Patient) {
				customDebug.LogError($"Not a patient arm object!");
				return false;
			}

			List<PosRotMapping> currentAnimationSetup = animSettings.CurrentSetup;
			if (currentAnimationSetup.Count < 1) {
				customDebug.LogError($"Too few animation positions set: '{currentAnimationSetup.Count}'!");
				messageEvents.ShowMessage(new (Globals.LocalizedMessages.TooFewPositions, MessageType.WARNING, true, new Arg[] { new("0", currentAnimationSetup.Count) }));
				return false;
			}
			if (animSettings.AnimType == AnimationType.Key && currentAnimationSetup.Count != 2) {
				customDebug.LogError($"Too few animation positions set for 'Key': '{currentAnimationSetup.Count}'!");
				messageEvents.ShowMessage(new (Globals.LocalizedMessages.TooFewPositionsForKey, MessageType.WARNING, true));
				return false;
			}
			// Initial starting position HAS to be in arm range
			if (!IsTargetInRange(currentAnimationSetup[0].Position)) {
				float targetDistance = Vector3.Distance(currentAnimationSetup[0].Position, armRangeMesh.transform.position);
				customDebug.LogWarning($"Arm cannot grab object, too far away: '{targetDistance}m > {armLength}m'.");

				Arg[] args = new Arg[] { new("0", targetDistance), new("1", armLength) };
				messageEvents.ShowMessage(new (Globals.LocalizedMessages.CannotGrab, MessageType.WARNING, true, args));
				return false;
			}
			return true;
		}

		public bool SetArmIntoRestPosition(bool _isArmResting) {
			if (m_isArmResting == _isArmResting) {
				return false;
			}
			m_isArmResting = _isArmResting;

			if (m_isArmResting) {
				if (armRestCoroutine != null) {
					StopCoroutine(armRestCoroutine);
				}
				armRestCoroutine = StartCoroutine(RestArmStartCoroutine());
			} else {
				if (armRestCoroutine != null) {
					StopCoroutine(armRestCoroutine);
				}
				StartCoroutine(RestArmStopCoroutine());
			}
			return true;
		}

		public bool IsTargetInRange(Vector3 targetPosition) {
			float targetDistance = Vector3.Distance(targetPosition, armRangeMesh.transform.position);
			return armLength > (targetDistance - armRangeSlack);
		}

		public float CalculateArmLength() {
			return armRangeMesh.transform.lossyScale.x * armRangeMesh.sharedMesh.bounds.extents.x;
		}

		public float GetArmLength() {
			return armLength;
		}

		public float GetArmLengthWithSlack() {
			return armLength + armRangeSlack;
		}

		public Vector3 GetArmRangePosition() {
			return armRangeMesh.transform.position;
		}

		public ArmAnimationState GetAnimationState() {
			return m_animState;
		}

		public bool IsLeft() {
			return isLeft;
		}

	}
}