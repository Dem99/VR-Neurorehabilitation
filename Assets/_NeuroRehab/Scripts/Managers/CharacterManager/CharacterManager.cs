using Mirror;
using UnityEngine;
using System;
using System.Collections;

using NeuroRehab.Avatar;
using NeuroRehab.Core;
using NeuroRehab.ScriptObjects;
using NeuroRehab.Animation;
using NeuroRehab.Interfaces;
using UnityEngine.XR.Interaction.Toolkit;

namespace NeuroRehab.Managers.Character {
	/// <summary>
	/// Abstract parent class for Multiplayer Character manager. Setups starting components - enables/disables components that would otherwise create conflicts in the scene.
	/// </summary>
	public abstract class CharacterManager : NetworkBehaviour, IAvatarChanger, IArmRestingManager, ICharacter {
		protected CustomDebug customDebug = new("CHAR_MANAGER");

		[Header("Scriptable objects")]
		[SerializeField] private AvatarSettings avatarSettings;
		[SerializeField] private MenuHelperSO menuHelper;
		[SerializeField] private AnimationSettingsSO animSettings;

		[Header("Dependencies")]
		[Header("Spawn sync vars")]
		[SyncVar] private UserRole userRole;
		[SyncVar] private bool isFemale;
		[SyncVar] private int avatarNumber;
		[SyncVar] private float avatarSizeMultiplier;
		[SyncVar] private float avatarOffsetDistance;
		[SyncVar] private string roomID;

		[Header("Run sync vars")]
		[SyncVar(hook = nameof(ChangeAnimatedArm))] private bool isLeftArmAnimated = false;
		[SyncVar(hook = nameof(FixArms))] private bool isArmFixed = false;
		[SyncVar(hook = nameof(ChangeArmRestingState))] private bool isArmResting;

		[Header("Avatar Components")]
		[SerializeField] private AvatarModelManager modelManager;
		[SerializeField] private Transform offsetObject;
		[SerializeField] private Camera localCamera;
		[SerializeField] protected CharacterController characterController;

		[Header("Activated objects based on 'authority'")]
		// Items is array of components that may need to be enabled / activated  only locally
		[SerializeField] private GameObject[] itemsToActivate;
		[SerializeField] private GameObject[] prefabsToSpawn;
		[SerializeField] protected AvatarWalkingController[] avatarWalkingControllers;
		[SerializeField] protected NetworkAvatarWalkingController networkAvatarWalkingController;

		[Header("Camera culling and objects to disable")]
		[SerializeField] private GameObject[] objectsToCull;
		[SerializeField] private GameObject[] avatars;
		[SerializeField] private SingleUnityLayer layerCameraCull;

		[Header("Components On / Off")]
		[Tooltip("Disables component if current instance is non local")]
		[SerializeField] private Behaviour[] componentsToDisable;
		[Tooltip("Enables components, doesn't matter whether instance is local or not")]
		[SerializeField] private Behaviour[] componentsToEnable;
		[Tooltip("Disables component if current instance is local")]
		[SerializeField] private Behaviour[] componentsToEnableLocally;

		// ---------------------------------------------------------------------
		protected INetworkCharacter networkCharacter;
		protected ISceneObjectManager sceneObjectManager;
		protected SceneHelper sceneHelper;

		private ArmAnimationController activeArmAnimationController;
		private MeshRenderer activeArmRangeMarker;

		private Transform leftRestTarget;
		private Transform rightRestTarget;

#pragma warning disable 67
		public event Action OnAvatarChange;
		public event Action OnResetHeight;
#pragma warning restore 67

		public Camera LocalCamera { get => localCamera; }
		public bool IsLeftArmAnimated { get => isLeftArmAnimated; protected set => isLeftArmAnimated = value; }
		public bool IsArmFixed { get => isArmFixed; set => isArmFixed = value; }
		public UserRole UserRole { get => userRole; protected set => userRole = value; }
		public bool IsFemale { get => isFemale; protected set => isFemale = value; }
		public int AvatarNumber { get => avatarNumber; protected set => avatarNumber = value; }
		public float AvatarSizeMultiplier { get => avatarSizeMultiplier; protected set => avatarSizeMultiplier = value; }
		public float AvatarOffsetDistance { get => avatarOffsetDistance; protected set => avatarOffsetDistance = value; }
		public string RoomID { get => roomID; protected set => roomID = value; }
		public bool IsArmResting { get => isArmResting; set => isArmResting = value; }
		bool ICharacter.IsLocalPlayer { get => base.isLocalPlayer; }
		public uint Id { get => netId; }

		private void Awake() {
			networkCharacter = GetComponent<INetworkCharacter>();

			sceneHelper = new(gameObject.scene);
			sceneObjectManager = sceneHelper.FindRootObject<ISceneObjectManager>();
		}

		public override void OnStartLocalPlayer() {
			base.OnStartLocalPlayer();
			sceneObjectManager.LocalChar = this;

			customDebug.Log("Local Character started");

			SetupTeleportation();

			// itemsToActivate is array of components that may need to be enabled / activated only locally
			for (int i = 0; i < itemsToActivate.Length; i++) {
				itemsToActivate[i].SetActive(true);
			}

			// prefabs that may need to be spawned only locally
			for (int i = 0; i < prefabsToSpawn.Length; i++) {
				Instantiate(prefabsToSpawn[i]);
			}

			for (int i = 0; i < avatarWalkingControllers.Length; i++) {
				avatarWalkingControllers[i].enabled = true;
			}

			foreach (GameObject gameObject in objectsToCull) {
				gameObject.layer = layerCameraCull.LayerIndex;
			}

			foreach (Behaviour component in componentsToEnableLocally) {
				component.enabled = true;
			}

			menuHelper.RenderingCamera = LocalCamera;

			animSettings.OnLockPickedUp += LockPickedUp;
			animSettings.OnTargetPickedUp += ItemPickUp;
			animSettings.OnTargetReleased += ItemRelease;
			
		}

		public override void OnStopLocalPlayer() {
			base.OnStopLocalPlayer();

			animSettings.OnLockPickedUp -= LockPickedUp;
			animSettings.OnTargetPickedUp -= ItemPickUp;
			animSettings.OnTargetReleased -= ItemRelease;
		}

		protected virtual void Start() {
			if (userRole == UserRole.Patient) {
				sceneObjectManager.ActivePatient = this;
			}

			rightRestTarget =sceneObjectManager.Table.RightArmRestHelper;
			leftRestTarget =sceneObjectManager.Table.LeftArmRestHelper;

			// Setting correct avatar, based on which one was chosen in the lobby
			OnAvatarChange?.Invoke();

			// Move whole transform offset object up or down depending on scaling (taking into account difference between male and female model height)
			offsetObject.position *= avatarSizeMultiplier / modelManager.GetActiveAvatarController().CalculateStandardizedSizeMultiplier();

			if (userRole == UserRole.Patient) {
				ChangeAnimatedArm(false, IsLeftArmAnimated);
			} else {
				enableControllerTracking();
			}

			networkAvatarWalkingController.enabled = true;

			// if non local character prefab is loaded we have to disable components such as camera, etc. otherwise Multiplayer aspect wouldn't work properly
			if (!isLocalPlayer) {
				foreach (Behaviour component in componentsToDisable) {
					component.enabled = false;
				}
			}

			foreach (Behaviour component in componentsToEnable) {
				component.enabled = true;
			}
		}

		protected void OnDestroy() {
			if (userRole == UserRole.Patient) {
				sceneObjectManager.ActivePatient = null;
			}
		}

		public void Initialize(bool isFemale, int avatarNumber, float sizeMultiplier, float offsetDistance, UserRole userRole, string roomID) {
			IsFemale = isFemale;
			AvatarNumber = avatarNumber;
			AvatarSizeMultiplier = sizeMultiplier;
			AvatarOffsetDistance = offsetDistance;
			UserRole = userRole;
			RoomID = roomID;
		}

		/// <summary>
		/// Handler method for item pickup. We request authority from Server and we also show arm range if possible
		/// </summary>
		/// <param name="obj">Transform of object grabbed</param>
		protected void ItemPickUp(Transform obj) {
			if (!isOwned) {
				return;
			}

			if (obj.TryGetComponent(out NetworkIdentity objNetIdentity)) {
				StartCoroutine(ItemPickedUp(objNetIdentity));
			}
		}

		/// <summary>
		/// Handler method for item release. We simply hide arm range. We don't have to release authority, we can save up some processing this way...
		/// </summary>
		/// <param name="args"></param>
		protected void ItemRelease(Transform obj) {
			if (!isOwned) {
				return;
			}

			if (obj.TryGetComponent(out NetworkIdentity objNetIdentity)) {
				if (!objNetIdentity.isOwned) {
					return;
				}
				ItemReleased(objNetIdentity);
			}
		}

		protected void LockPickedUp(Transform lockTransform, Transform keyPosTransform) {
			if (lockTransform.TryGetComponent(out NetworkIdentity netIdentity)) {
				networkCharacter.CmdSetLockPosition(new PosRotMapping(keyPosTransform), netIdentity.netId);
			}
		}

		protected virtual IEnumerator ItemPickedUp(NetworkIdentity itemNetIdentity) {
			if (itemNetIdentity == null) {
				customDebug.LogError("Can't start drag of null object!");
				yield break;
			}
			// we ask server to grant us authority over target object
			if (!RequestOwnership(itemNetIdentity.gameObject)) {
				yield break;
			}
			yield return new WaitUntil(() => itemNetIdentity.isOwned);

			itemNetIdentity.TryGetComponent(out IDraggable draggable);
			draggable?.OnStartDrag();

			if (itemNetIdentity.TryGetComponent(out ITargetDisable targetDisable)) {
				targetDisable?.CmdDisableDrag();
				if (targetDisable.ShowRange && sceneObjectManager.ActivePatient != null) {
					sceneObjectManager.ActivePatient.ShowArmRangeMarker();
				}
			}
		}

		public virtual void ItemStartMove(Transform itemObj) {
			if (itemObj == null) {
				customDebug.LogError("Can't start move of null object!");
				return;
			}
			StartCoroutine(ItemStartMove(itemObj.GetComponent<NetworkIdentity>()));
		}

		protected virtual IEnumerator ItemStartMove(NetworkIdentity itemNetIdentity) {
			if (itemNetIdentity == null) {
				customDebug.LogError("Can't start move of non networked object!");
				yield break;
			}
			// we ask server to grant us authority over target object
			if (!RequestOwnership(itemNetIdentity.gameObject)) {
				yield break;
			}
			yield return new WaitUntil(() => itemNetIdentity.isOwned);

			itemNetIdentity.TryGetComponent(out ITargetDisable targetDisable);
			targetDisable?.CmdDisableDrag();
		}

		protected virtual void ItemReleased(NetworkIdentity itemNetIdentity) {
			itemNetIdentity.TryGetComponent(out IDraggable draggable);
			draggable?.OnStopDrag();

			if (itemNetIdentity.TryGetComponent(out ITargetDisable targetDisable)) {
				targetDisable?.CmdEnableDrag();
				if (targetDisable.ShowRange && sceneObjectManager.ActivePatient != null) {
					sceneObjectManager.ActivePatient.HideArmRangeMarker();
				}
			}
		}

		/// <summary>
		/// Handler triggered by SyncVar used to change rest state of active arm
		/// </summary>
		/// <param name="_old"></param>
		/// <param name="_new"></param>
		protected void ChangeArmRestingState(bool _old, bool _new) {
			if (!(userRole == UserRole.Patient) || activeArmAnimationController == null) {
				return;
			}

			if (activeArmAnimationController.SetArmIntoRestPosition(_new)) {
				modelManager.GetActiveAvatarController().SetAvatarTurn(!_new);
			}
		}

		/// <summary>
		/// Method used to change active animated arm. First we change active arm variable. If arm is in resting position we make sure it stays that way.
		/// </summary>
		/// <param name="_old"></param>
		/// <param name="_new"></param>
		public virtual void ChangeAnimatedArm(bool _old, bool _new) {
			if (IsArmFixed){
				return;
			}

			IsLeftArmAnimated = _new;

			activeArmAnimationController = modelManager.InitAnimationControllers(IsLeftArmAnimated);

			if (activeArmAnimationController == null) {
				customDebug.LogError("Failed to find correct Arm animation controller for Patient");
				return;
			}

			AvatarController avatarController = modelManager.GetActiveAvatarController();

			avatarController.SetActiveArms(!IsLeftArmAnimated, IsLeftArmAnimated, rightRestTarget, leftRestTarget);
			//avatarController.ResetHandIKTargets();

			activeArmRangeMarker = avatarController.GetArmRangeRenderer(IsLeftArmAnimated);
		}

		public virtual void enableControllerTracking() {

			modelManager.disableAnimationControllers();
			AvatarController avatarController = modelManager.GetActiveAvatarController();

			avatarController.SetActiveArms(true, true, rightRestTarget, leftRestTarget);
			//avatarController.ResetHandIKTargets();

		}

		public virtual void FixArms(bool _old, bool _new) {
			IsArmFixed = _new;

			activeArmAnimationController = modelManager.InitAnimationControllers(IsLeftArmAnimated);
			AvatarController avatar = modelManager.GetActiveAvatarController();
			
			if (IsLeftArmAnimated){
				avatar.SetActiveArms(false, !IsArmFixed, rightRestTarget, leftRestTarget);
			} else {
				avatar.SetActiveArms(!IsArmFixed, false, rightRestTarget, leftRestTarget);
			}

			activeArmRangeMarker = avatar.GetArmRangeRenderer(IsLeftArmAnimated);
		}

		private void SetupTeleportation() {
			GameObject groundObject = GameObject.FindGameObjectWithTag("Ground");
			if (groundObject != null) {
				TeleportationArea teleportationArea = groundObject.GetComponent<TeleportationArea>();
				if (teleportationArea != null) {
					TeleportationProvider provider = GetComponentInChildren<TeleportationProvider>();
					if (provider != null) {
						teleportationArea.teleportationProvider = provider;
					} else {
						Debug.LogError("No TeleportationProvider found on the local XR Rig.");
					}
				} else {
					Debug.LogError("No XRTeleportationArea found on the ground object.");
				}
			} else {
				Debug.LogError("Ground object not found.");
			}
		}

		public void ShowArmRangeMarker() {
			activeArmRangeMarker.enabled = true;
		}

		public void HideArmRangeMarker() {
			activeArmRangeMarker.enabled = false;
		}

		public bool IsAnimationPlaying() {
			return activeArmAnimationController.GetAnimationState() == ArmAnimationState.Playing;

		}

		public (bool, GameObject) GetAvatarInfo() {
			return avatarSettings.GetAvatarInfo(isFemale, avatarNumber);
		}

		public float GetSizeMulti() {
			return avatarSizeMultiplier;
		}

		public float GetOffsetDist() {
			return avatarOffsetDistance;
		}

		public void SetAnimState(ArmAnimationState animationState) {
			activeArmAnimationController.SetAnimState(animationState);
		}

		public bool StartAnimation(bool isShowcase, bool isTraining, Transform targetTransform, Transform lockTransform) {
			return activeArmAnimationController.StartAnimation(isShowcase, isTraining, targetTransform, lockTransform);
		}

		public void StopAnimation() {
			activeArmAnimationController.StopAnimation();
		}

		public bool IsTargetInRange(Vector3 position) {
			return activeArmAnimationController.IsTargetInRange(position);
		}

		public float GetArmLength() {
			return activeArmAnimationController.GetArmLength();
		}

		public float GetArmLengthWithSlack() {
			return activeArmAnimationController.GetArmLengthWithSlack();
		}

		public float GetTargetRangeFromShoulder(Vector3 targetPos) {
			return Vector3.Distance(targetPos, activeArmAnimationController.GetArmRangePosition());
		}

		public bool RequestOwnership(GameObject obj) {
			return networkCharacter.RequestOwnership(obj);
		}

		public void ProgressAnimationStep(ExerciseState state) {
			networkCharacter.CmdProgressAnimationStep(state);
		}

		public void SetAnimationState(ArmAnimationState m_animState) {
			networkCharacter.CmdSetAnimationState(m_animState);
		}

		public abstract void TeleportCharacter(Transform targetPosition, Transform lookTarget = null);
	}

}