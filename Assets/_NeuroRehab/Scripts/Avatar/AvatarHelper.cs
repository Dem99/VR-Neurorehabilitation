using System;
using UnityEngine;

using NeuroRehab.Animation;

namespace NeuroRehab.Avatar {
	[Serializable]
	public class AvatarHelper {
		[SerializeField] private GameObject avatarObject;
		[SerializeField] private AvatarController avatarController;
		[SerializeField] private AvatarLowerBodyAnimationController lowerBodyController;
		[SerializeField] private AvatarSetup avatarSetup;
		[SerializeField, NonReorderable] private ArmAnimationController[] animationControllers;

		public GameObject AvatarObject { get => avatarObject;}
		public AvatarController AvatarController { get => avatarController;}
		public AvatarLowerBodyAnimationController LowerBodyController { get => lowerBodyController;}
		public AvatarSetup AvatarSetup { get => avatarSetup;}
		public ArmAnimationController[] AnimationControllers { get => animationControllers; }
	}
}