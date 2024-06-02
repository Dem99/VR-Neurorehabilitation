using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using NeuroRehab.ScriptObjects;

namespace NeuroRehab.UI.Managers.Menu.Mini {
	public class MiniMenuManager : MonoBehaviour {
		[Header("Scriptable objects")]
		[SerializeField] protected OffsetSettings offsetSettings;
		[SerializeField] protected MenuHelperSO menuHelper;
		[SerializeField] private SceneChangeEventsSO sceneChange;

		[Header("Dependencies")]
		[SerializeField] private InputActionReference menuAction;
		[SerializeField] private bool lockCursor;
		[SerializeField] private bool isStickyParent = true;
		[SerializeField] private Transform menuHolder;
		private Canvas menuHolderCanvas;

		[SerializeField] private GameObject menuToShow;
		private RectTransform menuToShowTransform;

		protected Vector3 transformOffset;
		protected Vector3 initialTransformOffset;

		protected float offsetLimit = 0.7f;

		[SerializeField] private Vector3 positionOffset;
		[SerializeField] private bool autoScale = false;
		[SerializeField] private Vector3 scale;

		[SerializeField] private bool offsetByWidth;
		[SerializeField] private bool offsetByHeight;

		[SerializeField] private MiniMenuVisibilityManager miniMenuVisibilityManager;

		[SerializeField] private GameObject reticle;

		[SerializeField] private GameObject offsetControls;
		[SerializeField] private Button offsetControlsButton;
		[SerializeField] private Sprite activeOffsetControlsButton;
		[SerializeField] private Sprite inactiveOffsetControlsButton;

		protected Vector3 originalMenuPosition;
		protected Quaternion originalMenuRotation;
		protected Vector3 originalMenuScale;
		protected Transform originalMenuParent;
		protected bool menuInitialized = false;

		public Canvas MenuHolderCanvas { get => menuHolderCanvas; }

		protected virtual void Awake() {
			if (miniMenuVisibilityManager != null) {
				miniMenuVisibilityManager.RegisterMiniMenuManager(this);
			}
			initialTransformOffset = transform.localPosition;
			menuHolderCanvas = menuHolder.GetComponent<Canvas>();
		}

		private void OnEnable() {
			if (menuToShow == null) {
				menuToShow = menuHelper.MainMenu;
			}
			InitMenu();

			menuHelper.Init();

			menuAction.action.Enable();
			menuAction.action.performed += TriggerMenu;
		}

		private void OnDisable() {
			menuAction.action.Disable();
			menuAction.action.performed -= TriggerMenu;

			// InitMenu();
			if (!sceneChange.IsSceneChanging) {
				ResetMenu();
			}
			if (reticle) {
				reticle.SetActive(true);
			}
		}

		protected virtual void Start() {
			menuHolderCanvas.enabled = false;
			if (offsetControls) {
				offsetControls.SetActive(false);
			}
		}

		private void InitMenu() {
			if (menuInitialized) {
				return;
			}

			if (menuHolderCanvas == null) {
				menuHolderCanvas = menuHolder.GetComponent<Canvas>();
			}

			if (menuToShow != null) {
				originalMenuPosition = menuToShow.transform.localPosition;
				originalMenuRotation = menuToShow.transform.localRotation;
				originalMenuScale = menuToShow.transform.localScale;
				originalMenuParent = menuToShow.transform.parent;

				menuToShowTransform = (RectTransform)menuToShow.transform;
			}

			if (autoScale && menuToShow != null) {
				float tmpScale = ((RectTransform)menuHolder).rect.width / menuToShowTransform.rect.width;
				scale = new Vector3(tmpScale, tmpScale, 1f);
			}

			if (offsetControls) {
				InitOffset();
			}

			menuInitialized = true;
		}

		protected virtual void InitOffset() {
			if (offsetSettings.MiniMenuInitialized) {
				transformOffset = offsetSettings.MiniMenuTransformOffset;

				transform.localPosition = transformOffset;
			} else {
				transformOffset = transform.localPosition;
				SaveOffsetSettings();
				offsetSettings.MiniMenuInitialized = true;
			}
		}

		private void TriggerMenu(InputAction.CallbackContext obj) {
			StartCoroutine(TriggerMenuCoroutine());
		}

		/// <summary>
		/// Coroutine for triggering menu, we have to wrap it in Coroutine and wait for Fixed update manually, because Hold action acts weirdly, triggers unwanted artifacts in VR.
		/// </summary>
		/// <returns></returns>
		private IEnumerator TriggerMenuCoroutine() {
			yield return new WaitForFixedUpdate();

			if (miniMenuVisibilityManager != null) {
				if (miniMenuVisibilityManager.IsMenuShowing(this)) {
					yield break;
				}
				menuHelper.IsMenuShowing = miniMenuVisibilityManager.TriggerMenu(this);
			} else {
				menuHelper.IsMenuShowing = !menuHelper.IsMenuShowing;
			}

			if (menuHelper.IsMenuShowing) {
				if (isStickyParent) {
					menuToShow.transform.SetParent(menuHolder);
				}

				SetupMenuPositining();

				if (reticle) {
					reticle.SetActive(false);
				}
			} else {
				menuToShow.transform.SetParent(originalMenuParent);

				menuToShow.transform.localScale = originalMenuScale;
				menuToShow.transform.localRotation = Quaternion.identity;
				menuToShow.transform.localPosition = originalMenuPosition;
				menuToShow.transform.localRotation = originalMenuRotation;

				if (reticle) {
					reticle.SetActive(true);
				}
			}

			menuHolderCanvas.enabled = menuHelper.IsMenuShowing;
		}

		protected virtual void SetupMenuPositining() {
			menuToShow.transform.localScale = scale;
			menuToShow.transform.localRotation = Quaternion.identity;

			Vector3 newPosition = positionOffset;
			if (offsetByWidth) {
				newPosition.x -= menuToShowTransform.rect.width;
			}
			if (offsetByHeight) {
				newPosition.y -= menuToShowTransform.rect.height;
			}

			menuToShow.transform.localPosition = newPosition;
		}

		private void ResetMenu() {
			if (menuToShow == null || originalMenuParent == null) {
				return;
			}
			if (miniMenuVisibilityManager != null) {
				miniMenuVisibilityManager.SetMenuStatus(this, false);
			}
			menuHelper.IsMenuShowing = false;

			menuToShow.transform.SetParent(originalMenuParent);

			menuToShow.transform.localScale = originalMenuScale;
			menuToShow.transform.localRotation = Quaternion.identity;
			menuToShow.transform.localPosition = originalMenuPosition;
			menuToShow.transform.localRotation = originalMenuRotation;

			menuHolderCanvas.enabled = false;
		}

		public virtual void OffsetRight() {
			if (Mathf.Abs(initialTransformOffset.x - transformOffset.x) >= offsetLimit) {
				return;
			}

			transformOffset += new Vector3(0.05f, 0f, 0f);
			transform.localPosition = new Vector3(transform.localPosition.x + 0.05f, transform.localPosition.y, transform.localPosition.z);

			SaveOffsetSettings();
		}

		public virtual void OffsetLeft() {
			if (Mathf.Abs(initialTransformOffset.x - transformOffset.x) >= offsetLimit) {
				return;
			}
			transformOffset += new Vector3(-0.05f, 0f, 0f);
			transform.localPosition = new Vector3(transform.localPosition.x - 0.05f, transform.localPosition.y, transform.localPosition.z);

			SaveOffsetSettings();
		}

		public virtual void OffsetFwd() {
			if (Mathf.Abs(initialTransformOffset.z - transformOffset.z) >= offsetLimit) {
				return;
			}
			transformOffset += new Vector3(0f, 0f, 0.05f);
			transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + 0.05f);

			SaveOffsetSettings();
		}

		public virtual void OffsetBack() {
			if (Mathf.Abs(initialTransformOffset.z - transformOffset.z) >= offsetLimit) {
				return;
			}

			transformOffset += new Vector3(0f, 0f, -0.05f);
			transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - 0.05f);

			SaveOffsetSettings();
		}

		public void TriggerOffset() {
			offsetControls.SetActive(!offsetControls.activeSelf);

			if (offsetControls.activeSelf) {
				offsetControlsButton.image.sprite = activeOffsetControlsButton;
			} else {
				offsetControlsButton.image.sprite = inactiveOffsetControlsButton;
			}
		}

		public void ResetOffset() {
			transformOffset = initialTransformOffset;
			transform.localPosition = transformOffset;
			SetupMenuPositining();

			SaveOffsetSettings();
		}

		protected virtual void SaveOffsetSettings() {
			offsetSettings.MiniMenuTransformOffset = transformOffset;
		}
	}
}