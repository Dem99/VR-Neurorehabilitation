using UnityEngine;
using AYellowpaper;

using NeuroRehab.Animation;
using NeuroRehab.Interfaces;
using NeuroRehab.Core;

namespace NeuroRehab.Avatar {
	/// <summary>
	/// Contains methods for changing models.
	/// </summary>
	public class AvatarModelManager : MonoBehaviour {
		private readonly CustomDebug customDebug = new("AvatarModelManager");

		[Header("Dependencies")]
		[SerializeField] private InterfaceReference<IAvatarChanger> avatarChanger;

		[SerializeField] private AvatarHelper avatarFemaleHelper;
		[SerializeField] private AvatarHelper avatarMaleHelper;

		private GameObject currentPrefab;
		private bool isFemale;

		public bool IsFemale { get => isFemale;}

		private void Awake() {
			if (avatarChanger == null) {
				throw new System.Exception($"Wrong object for 'avatarChanger' on object: '{gameObject.name}'");
			}
		}

		private void OnEnable() {
			avatarChanger.Value.OnAvatarChange += ChangeModel;
		}

		private void OnDisable() {
			avatarChanger.Value.OnAvatarChange -= ChangeModel;
		}

		private void Start() {
			ChangeModel();
		}

		/// <summary>
		/// Method used to change avatar
		/// </summary>
		public void ChangeModel() {
			(bool _isFemale, GameObject avatarPrefab) = avatarChanger.Value.GetAvatarInfo();

			if (currentPrefab == avatarPrefab && isFemale) {
				return;
			}

			isFemale = _isFemale;

			avatarFemaleHelper.AvatarObject.SetActive(false);
			avatarMaleHelper.AvatarObject.SetActive(false);

			AvatarHelper tmpHelper = isFemale ? avatarFemaleHelper : avatarMaleHelper;

			tmpHelper.AvatarController.SetSizeMulti(avatarChanger.Value.GetSizeMulti());
			tmpHelper.LowerBodyController.SetGroundOffset(avatarChanger.Value.GetOffsetDist());

			tmpHelper.AvatarObject.SetActive(true);
			tmpHelper.AvatarSetup.SetupModel(avatarPrefab);
			currentPrefab = avatarPrefab;
		}

		public GameObject GetActiveAvatarObj() {
			return isFemale ? avatarFemaleHelper.AvatarObject : avatarMaleHelper.AvatarObject;
		}

		public AvatarController GetActiveAvatarController() {
			return isFemale ? avatarFemaleHelper.AvatarController : avatarMaleHelper.AvatarController;
		}

		public ArmAnimationController InitAnimationControllers(bool isLeftArmAnimated) {
			ArmAnimationController[] armAnimationControllers = isFemale ? avatarFemaleHelper.AnimationControllers : avatarMaleHelper.AnimationControllers;
			ArmAnimationController activeController = null;

			foreach (ArmAnimationController armAnimationController in armAnimationControllers) {
				armAnimationController.Init();
				if (isLeftArmAnimated == armAnimationController.IsLeft()) {
					armAnimationController.enabled = true;
					activeController = armAnimationController;
				} else {
					armAnimationController.enabled = false;
					armAnimationController.SetArmIntoRestPosition(false);
				}
			}

			return activeController;
		}

		public void disableAnimationControllers() {
			ArmAnimationController[] armAnimationControllers = isFemale ? avatarFemaleHelper.AnimationControllers : avatarMaleHelper.AnimationControllers;

			foreach (ArmAnimationController armAnimationController in armAnimationControllers) {
					armAnimationController.enabled = false;
			}
		}
	}
}