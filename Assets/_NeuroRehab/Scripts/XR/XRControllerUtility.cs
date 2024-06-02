using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

using NeuroRehab.Core;
using NeuroRehab.ScriptObjects;

/// <summary>
/// Class used to determine whether XR Controller Object is used for Left or Right arm, so that we don't have to rely on names of objects.
/// </summary>
namespace NeuroRehab.XR {
	public class XRControllerUtility : MonoBehaviour {
		[SerializeField] private CustomXRSettings xrSettings;
		[SerializeField] private XRBaseController m_controller;
		[SerializeField] private bool m_isLeftHandController;

		private void OnEnable() {
			m_controller.enabled = true;

			m_controller.enableInputTracking = true;
			m_controller.enableInputActions = true;
		}

		private void OnDisable() {
			m_controller.enabled = false;

			m_controller.enableInputTracking = false;
			m_controller.enableInputActions = false;
		}

		public void ChangeModel(ControllerType controllerType) {
			if (m_controller.modelParent == null) {
				return;
			}

			m_controller.modelPrefab = xrSettings.GetControllerPrefab(m_isLeftHandController, controllerType).transform;

			if (m_controller.model != null) {
				m_controller.model.gameObject.SetActive(false);
			}

			m_controller.model = Instantiate(m_controller.modelPrefab, m_controller.modelParent.position, m_controller.modelParent.rotation, m_controller.modelParent);
		}

		public bool IsLeftHand() {
			return m_isLeftHandController;
		}
	}
}