using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

using NeuroRehab.Core;
using NeuroRehab.ScriptObjects;

namespace NeuroRehab.UI.Managers.Menu.Settings {

	public class XRSettingsMenuManager : MonoBehaviour {
		[SerializeField] private CustomXRSettings xrSettings;

		public void SetControllerPrefabs(int controller) {
			if (!Enum.IsDefined(typeof(ControllerType), controller)) {
				Debug.LogError("Wrong enum value argument - ControllerType");
				return;
			}

			xrSettings.ControllerType = (ControllerType) controller;

			GameObject controllerL = xrSettings.GetControllerPrefab(true);
			GameObject controllerR = xrSettings.GetControllerPrefab(false);

			XRBaseController rightC = GameObject.Find("RightHand Controller").GetComponent<XRBaseController>();
			XRBaseController leftC = GameObject.Find("LeftHand Controller").GetComponent<XRBaseController>();

			leftC.modelPrefab = controllerL.transform;
			rightC.modelPrefab = controllerR.transform;

			if (rightC.model != null && leftC.model != null) {
				rightC.model.gameObject.SetActive(false);
				leftC.model.gameObject.SetActive(false);
			}

			if (leftC.modelParent != null) {
				leftC.model = Instantiate(leftC.modelPrefab, leftC.modelParent.transform.position, leftC.modelParent.transform.rotation, leftC.modelParent.transform);
			}
			if (rightC.modelParent != null) {
				rightC.model = Instantiate(rightC.modelPrefab, rightC.modelParent.transform.position, rightC.modelParent.transform.rotation, rightC.modelParent.transform);
			}
		}

		public void StartXR() {
			xrSettings.XrActive = true;
		}

		public void StopXR() {
			xrSettings.XrActive = false;
		}
	}
}