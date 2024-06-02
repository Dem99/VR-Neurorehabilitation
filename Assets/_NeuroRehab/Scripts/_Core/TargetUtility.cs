using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility class containing references for helper objects, renderers or customTargetPosition if required.
/// </summary>
namespace NeuroRehab.Core {
	public class TargetUtility : MonoBehaviour {
		public GameObject ArmIK_target_helper;
		public GameObject ThumbIK_target_helper;
		public GameObject IndexChainIK_target_helper;
		public GameObject MiddleChainIK_target_helper;
		public GameObject RingChainIK_target_helper;
		public GameObject PinkyChainIK_target_helper;

		public List<Renderer> renderers = new();

		public GameObject customTargetPos;
	}
}