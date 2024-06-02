using UnityEngine;

using NeuroRehab.Core;

namespace NeuroRehab.Animation {

	/// <summary>
	/// Component used to hold all animations mappings used in animations.
	/// </summary>
	[System.Serializable]
	public class AnimationMapping {

		// Filled in Inspector / Editor
		[NonReorderable] public TargetMappingGroup[] targetMappingGroups;

		public TargetMappingGroup GetTargetMappingByType(AnimationType animType) {
			foreach (TargetMappingGroup item in targetMappingGroups) {
				if (item.animationType == animType) {
					return item;
				}
			}
			return null;
		}

		public void ResizeMappings(float multiplier) {
			foreach (TargetMappingGroup item in targetMappingGroups) {
				item.ResizeMapping(multiplier);
			}
		}

		public void MirrorMappings(Transform _mirror) {
			foreach (TargetMappingGroup item in targetMappingGroups) {
				item.MirrorMapping(_mirror);
			}
		}
	}
}
