using UnityEngine;

namespace NeuroRehab.UI.Managers.Menu.Mini {
	public class StaticMenuManager : MiniMenuManager {
		[SerializeField] private Transform cameraTransform;

		[SerializeField] private float menuOffset = 1.25f;
		[SerializeField] private float menuOffsetVertical = -0.25f;

		protected override void Awake() {
			base.Awake();
			transform.position = cameraTransform.position + (cameraTransform.forward * menuOffset);
			transform.localRotation = cameraTransform.localRotation;
		}

		protected override void InitOffset() {
			if (offsetSettings.StaticMenuInitialized) {
				transformOffset = offsetSettings.StaticMenuTransformOffset;

				transform.localPosition = transformOffset;
			} else {
				transformOffset = transform.localPosition;
				SaveOffsetSettings();
				offsetSettings.StaticMenuInitialized = true;
			}
		}

		protected override void SetupMenuPositining() {
			base.SetupMenuPositining();
			if (menuHelper.IsMenuShowing) {
				// we set position and rotation separately, because we set rotation based on position
				transform.position = cameraTransform.position + (cameraTransform.forward * transformOffset.z);

				transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
				transform.SetParent(null);

				transform.position += new Vector3(0f, menuOffsetVertical, 0f);
			}
		}

		public override void OffsetRight() {
			if (Mathf.Abs(initialTransformOffset.x - transformOffset.x) >= offsetLimit) {
				return;
			}

			transformOffset += new Vector3(0.05f, 0f, 0f);
			transform.localPosition += transform.right * 0.05f;

			SaveOffsetSettings();
		}

		public override void OffsetLeft() {
			if (Mathf.Abs(initialTransformOffset.x - transformOffset.x) >= offsetLimit) {
				return;
			}

			transformOffset += new Vector3(-0.05f, 0f, 0f);
			transform.localPosition += transform.right * -0.05f;

			SaveOffsetSettings();
		}

		public override void OffsetFwd() {
			if (Mathf.Abs(initialTransformOffset.z - transformOffset.z) >= offsetLimit) {
				return;
			}

			transformOffset += new Vector3(0f, 0f, 0.05f);
			transform.localPosition += transform.forward * 0.05f;

			SaveOffsetSettings();
		}

		public override void OffsetBack() {
			if (Mathf.Abs(initialTransformOffset.z - transformOffset.z) >= offsetLimit) {
				return;
			}

			transformOffset += new Vector3(0f, 0f, -0.05f);
			transform.localPosition += transform.forward * -0.05f;

			SaveOffsetSettings();
		}

		protected override void SaveOffsetSettings() {
			offsetSettings.StaticMenuTransformOffset = transformOffset;
		}
	}
}