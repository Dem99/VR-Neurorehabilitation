using UnityEngine;
using System;
using AYellowpaper.SerializedCollections;

using NeuroRehab.Core;

namespace NeuroRehab.ScriptObjects {

	[CreateAssetMenu(menuName = "ScriptObjects/Settings/XR")]
	public class CustomXRSettings : ScriptableObject {

		[SerializeField] private bool m_xrActive;
		[SerializeField] private HMDType m_hmdType;
		[SerializeField] private ControllerType m_controllerType = ControllerType.Quest2; // https://forum.unity.com/threads/openxr-is-it-no-longer-possible-to-get-descriptive-device-names.1051493/
		[SerializeField, NonReorderable] private SerializedDictionary<ControllerPrefabKey, GameObject> m_controllerPrefabs;

		public event Action OnHmdChange;
		public event Action OnXRActiveChange;

		public bool XrActive {
			get => m_xrActive;
			set {
				if (m_xrActive == value) return;

				m_xrActive = value;
				OnHmdChange?.Invoke();
				OnXRActiveChange?.Invoke();
			}
		}

		public HMDType HmdType {
			get => m_hmdType;
			set {
				m_hmdType = value;
				OnHmdChange?.Invoke();
			}
		}

		public ControllerType ControllerType { get => m_controllerType; set => m_controllerType = value; }

		public GameObject GetControllerPrefab(bool isLeft) {
			return m_controllerPrefabs[new (ControllerType, isLeft)];
		}

		public GameObject GetControllerPrefab(bool isLeft, ControllerType controllerType) {
			return m_controllerPrefabs[new (controllerType, isLeft)];
		}
	}
}