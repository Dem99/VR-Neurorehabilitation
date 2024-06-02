using UnityEngine;

using NeuroRehab.Interfaces;
using NeuroRehab.Core;

namespace NeuroRehab.Animation {

	[System.Serializable]
	public class TargetsHelper : ITransformAligner {

		/// <summary>
		/// Component containing targets for out Arm/Hand objects. Targets in this case are objects used by 'AnimationRigging' package.
		/// </summary>
		[Header("Arm rest targets")]
		public GameObject armRestTarget;

		[Header("Arm targets")]
		// objects which are used as targets in dynamic animations (IK)
		public GameObject armTarget;
		public GameObject thumbTarget;
		public GameObject indexTarget;
		public GameObject middleTarget;
		public GameObject ringTarget;
		public GameObject pinkyTarget;

		[Header("Target object target templates (filled in code)")]
		// objects which are children of our targeted object (cube, block, cup, key etc.)
		public GameObject armTargetTemplate;
		public GameObject thumbTargetTemplate;
		public GameObject indexTargetTemplate;
		public GameObject middleTargetTemplate;
		public GameObject ringTargetTemplate;
		public GameObject pinkyTargetTemplate;

		public TargetsHelper(GameObject _armTarget, GameObject _thumbTarget, GameObject _indexTarget,
					GameObject _middleTarget, GameObject _ringTarget, GameObject _pinkyTarget,
					GameObject _armTargetFake, GameObject _thumbTargetFake, GameObject _indexTargetFake,
					GameObject _middleTargetFake, GameObject _ringTargetFake, GameObject _pinkyTargetFake) {
			armTarget = _armTarget;
			thumbTarget = _thumbTarget;
			indexTarget = _indexTarget;
			middleTarget = _middleTarget;
			ringTarget = _ringTarget;
			pinkyTarget = _pinkyTarget;

			armTargetTemplate = _armTargetFake;
			thumbTargetTemplate = _thumbTargetFake;
			indexTargetTemplate = _indexTargetFake;
			middleTargetTemplate = _middleTargetFake;
			ringTargetTemplate = _ringTargetFake;
			pinkyTargetTemplate = _pinkyTargetFake;
		}

		public void AlignTargetTransforms() {
			armTarget.transform.SetPositionAndRotation(armTargetTemplate.transform.position, armTargetTemplate.transform.rotation);

			thumbTarget.transform.SetPositionAndRotation(thumbTargetTemplate.transform.position, thumbTargetTemplate.transform.rotation);

			indexTarget.transform.SetPositionAndRotation(indexTargetTemplate.transform.position, indexTargetTemplate.transform.rotation);

			middleTarget.transform.SetPositionAndRotation(middleTargetTemplate.transform.position, middleTargetTemplate.transform.rotation);

			ringTarget.transform.SetPositionAndRotation(ringTargetTemplate.transform.position, ringTargetTemplate.transform.rotation);

			pinkyTarget.transform.SetPositionAndRotation(pinkyTargetTemplate.transform.position, pinkyTargetTemplate.transform.rotation);
		}

		/// <summary>
		/// Setting target positions. Either we use local position or we use world position and offset
		/// </summary>
		/// <param name="target"></param>
		/// <param name="targetMapping"></param>
		/// <param name="useLocalPosition">if false we use global rotation</param>
		/// <param name="useLocalRotation">if false we use global position + offset</param>
		/// <param name="parentObject"></param>
		private void SetTargetMapping(GameObject target, PosRotMapping targetMapping, bool useLocalPosition = true, bool useLocalRotation = true, Transform parentObject = null) {
			if (useLocalRotation) {
				target.transform.localRotation = Quaternion.Euler(targetMapping.Rotation);
			} else {
				target.transform.rotation = Quaternion.Euler(targetMapping.Rotation);
			}

			if (useLocalPosition) {
				target.transform.localPosition = targetMapping.Position;
			} else {
				target.transform.position = parentObject.transform.position - targetMapping.Position;
			}
		}

		public void SetAllTargetMappings(TargetMappingGroup currentAnimMapping, Transform targetObject = null) {
			if (currentAnimMapping == null) {
				return;
			}

			if (targetObject == null) {
				SetTargetMapping(armTargetTemplate, currentAnimMapping.armMapping);
				SetTargetMapping(thumbTargetTemplate, currentAnimMapping.thumbMapping);
				SetTargetMapping(indexTargetTemplate, currentAnimMapping.indexMapping);
				SetTargetMapping(middleTargetTemplate, currentAnimMapping.middleMapping);
				SetTargetMapping(ringTargetTemplate, currentAnimMapping.ringMapping);
				SetTargetMapping(pinkyTargetTemplate, currentAnimMapping.pinkyMapping);
			} else {
				SetTargetMapping(armTargetTemplate, currentAnimMapping.armMapping, false, false, targetObject);
				SetTargetMapping(thumbTargetTemplate, currentAnimMapping.thumbMapping, false, false, targetObject);
				SetTargetMapping(indexTargetTemplate, currentAnimMapping.indexMapping, false, false, targetObject);
				SetTargetMapping(middleTargetTemplate, currentAnimMapping.middleMapping, false, false, targetObject);
				SetTargetMapping(ringTargetTemplate, currentAnimMapping.ringMapping, false, false, targetObject);
				SetTargetMapping(pinkyTargetTemplate, currentAnimMapping.pinkyMapping, false, false, targetObject);
			}
		}

		public void SetHelperObjects(TargetUtility targetUtility) {
			// helper target objects, children of our target object
			armTargetTemplate = targetUtility.ArmIK_target_helper;
			thumbTargetTemplate = targetUtility.ThumbIK_target_helper;
			indexTargetTemplate = targetUtility.IndexChainIK_target_helper;
			middleTargetTemplate = targetUtility.MiddleChainIK_target_helper;
			ringTargetTemplate = targetUtility.RingChainIK_target_helper;
			pinkyTargetTemplate = targetUtility.PinkyChainIK_target_helper;
		}
	}
}