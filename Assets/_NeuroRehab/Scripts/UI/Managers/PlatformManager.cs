using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

using NeuroRehab.Core;
using NeuroRehab.ScriptObjects;

namespace NeuroRehab.UI.Managers {
	public class PlatformManager : MonoBehaviour {
		[SerializeField] private CustomXRSettings xrSettings;
		[SerializeField][NonReorderable] private List<PlatformMapping> platformMappings;

		void Awake() {
			foreach (PlatformMapping item in platformMappings) {
				if (item.gameObject.transform.parent.TryGetComponent<LayoutGroup>(out _)) {
					item.updateLayoutGroup = true;
					continue;
				}
				item.updateLayoutGroup = false;
			}
		}

		private void OnEnable() {
			xrSettings.OnHmdChange += UpdateUIVisibility;

			UpdateUIVisibility();
		}

		private void OnDisable() {
			xrSettings.OnHmdChange -= UpdateUIVisibility;
		}

		private void UpdateUIVisibility() {
			foreach (PlatformMapping item in platformMappings) {
				if (item.allowedHmdXrCombinations.TryGetValue(xrSettings.XrActive, out List<HMDType> hmdTypes) && hmdTypes.Contains(xrSettings.HmdType)) {
					item.gameObject.SetActive(true);
				} else {
					item.gameObject.SetActive(false);
				}

				if (item.updateLayoutGroup) {
					StartCoroutine(RebuildLayout((RectTransform) item.gameObject.transform.parent.transform));
				}
			}
		}

		private IEnumerator RebuildLayout(RectTransform rectTransform) {
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
			yield return new WaitForEndOfFrame();
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		}

		[Serializable]
		public class PlatformMapping {
			public GameObject gameObject;
			// essentially a 2D grid with variable length for BOTH rows and columns, better solution than writing custom OnGUI and we can search using Key
			[SerializedDictionary("XR status", "HMD")] public SerializedDictionary<bool, List<HMDType>> allowedHmdXrCombinations;
			public bool updateLayoutGroup = false;
		}
	}
}