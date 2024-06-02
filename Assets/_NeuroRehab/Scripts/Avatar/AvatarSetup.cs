using UnityEngine;
using System.Collections.Generic;

using NeuroRehab.Core;

namespace NeuroRehab.Avatar {
	/// <summary>
	/// Components used to change meshes and armature to fit current object.
	/// </summary>
	public class AvatarSetup : MonoBehaviour {
		private readonly CustomDebug customDebug = new("AVATAR_SETUP");

		[SerializeField] private Transform rootBone;
		[SerializeField] private string[] excludedNames;
		[SerializeField] private AvatarController avatarController;

		private List<Transform> currentAvatarObjects = new();
		private GameObject modelToUse;
		private Dictionary<string, Transform> allBones;

		void Awake() {
			SetupAvatarParts();

			allBones = new Dictionary<string, Transform>();
			var childrenBones = rootBone.GetComponentsInChildren<Transform>();
			foreach(Transform b in childrenBones) {
				allBones.Add(b.name, b);
			}
		}

		void OnDisable() {
			if (modelToUse != null) {
				Destroy(modelToUse);
			}
		}

		/// <summary>
		/// Load all objects into variable, so that we don't have to do it every time
		/// </summary>
		private void SetupAvatarParts() {
			currentAvatarObjects.Clear();
			foreach (Transform item in transform) {
				if(System.Array.IndexOf(excludedNames, item.name) == -1) {
					currentAvatarObjects.Add(item);
				}
			}
		}

		/// <summary>
		/// Method to change actual Avatar model. We replace mesh instead of changing whole object, otherwise it would break bunch of stuff + we would have to make perfect setup for every single Avatar variation (ineffective).
		/// </summary>
		/// <param name="model">Model that will be used</param>
		public void SetupModel(GameObject model) {
			if(modelToUse != null) {
				Destroy(modelToUse);
			}

			modelToUse = Instantiate(model);

			Transform newModelTransform = modelToUse.GetComponent<Transform>();

			bool flag;
			foreach(Transform bodyPart in currentAvatarObjects) {
				// Debug.Log("bodypart " + bodyPart.name);
				flag = false;
				if (bodyPart.TryGetComponent(out SkinnedMeshRenderer renderer)) {
					renderer.enabled = true;
				}
				foreach (Transform item in newModelTransform) {
					// Debug.Log("part " + item.name);
					if(System.Array.IndexOf(excludedNames, item.name) != -1) {
						continue;
					}
					if(bodyPart.name.Equals(item.name) && item.GetComponent<SkinnedMeshRenderer>() != null) {
						UpdateMesh(bodyPart.GetComponent<SkinnedMeshRenderer>(), item.GetComponent<SkinnedMeshRenderer>());
						flag = item.gameObject.activeSelf;
						break;
					}

				}
				if (flag) {
					continue;
				}
				if (renderer != null) {
					renderer.enabled = false;
				}
			}
			Destroy(modelToUse);

			avatarController.RotateAvatar();

			// customDebug.Log($"{transform.root.name} - Model succesfully changed {modelToUse.name}");
		}

		/// <summary>
		/// We update mesh, bones and materials
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="target"></param>
		private void UpdateMesh(SkinnedMeshRenderer origin, SkinnedMeshRenderer target) {
			origin.sharedMesh = target.sharedMesh;
			origin.sharedMaterials = target.sharedMaterials;

			var originBones = origin.bones;
			var targetBones = new List<Transform>();
			foreach(Transform b in originBones) {
				if(allBones.TryGetValue(b.name, out var foundBone)) {
					targetBones.Add(foundBone);
				}
			}
			target.bones = targetBones.ToArray();
		}
	}
}