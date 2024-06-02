using UnityEngine;

using NeuroRehab.Core;

namespace NeuroRehab.Animation {

	/// <summary>
	/// Contains mapping positioning for hand for animations. Does not contain move positions, only starting PosRotMapping.
	/// </summary>
	[System.Serializable]
	public class TargetMappingGroup {
		public AnimationType animationType;
		public PosRotMapping armMapping;
		public PosRotMapping thumbMapping;
		public PosRotMapping indexMapping;
		public PosRotMapping middleMapping;
		public PosRotMapping ringMapping;
		public PosRotMapping pinkyMapping;
		private Plane mirrorPlane;

		public TargetMappingGroup(AnimationType _animationType, PosRotMapping _armMapping, PosRotMapping _thumbMapping, PosRotMapping _indexMapping,
				PosRotMapping _middleMapping, PosRotMapping _ringMapping, PosRotMapping _pinkyMapping) {
			animationType = _animationType;
			armMapping = _armMapping;
			thumbMapping = _thumbMapping;
			indexMapping = _indexMapping;
			middleMapping = _middleMapping;
			ringMapping = _ringMapping;
			pinkyMapping = _pinkyMapping;
		}

		public void ResizeMapping(float multiplier) {
			armMapping.Position *= multiplier;
			thumbMapping.Position *= multiplier;
			indexMapping.Position *= multiplier;
			middleMapping.Position *= multiplier;
			ringMapping.Position *= multiplier;
			pinkyMapping.Position *= multiplier;
		}

		public void MirrorMapping(Transform _mirror) {
			mirrorPlane = new Plane(_mirror.forward, _mirror.position);

			armMapping.MirrorMapping(mirrorPlane);
			thumbMapping.MirrorMapping(mirrorPlane);
			indexMapping.MirrorMapping(mirrorPlane);
			middleMapping.MirrorMapping(mirrorPlane);
			ringMapping.MirrorMapping(mirrorPlane);
			pinkyMapping.MirrorMapping(mirrorPlane);
		}
	}
}
