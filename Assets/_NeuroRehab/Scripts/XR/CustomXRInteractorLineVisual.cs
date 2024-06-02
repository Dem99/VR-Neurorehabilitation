using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

using NeuroRehab.ScriptObjects;

namespace NeuroRehab.XR {
	/// <summary>
	/// Custom class that we use to make better Reticle, that is scaled depending on distance of your camera from reticle.
	/// </summary>
	public class CustomXRInteractorLineVisual : XRInteractorLineVisual {

		[Header("Custom reticle")]
		[Header("Scriptable objects")]
		[SerializeField] private GeneralSettings generalSettings;

		[Header("Dependencies")]
		[SerializeField] private XRRayInteractor XRRayInteractor;
		[SerializeField] private Transform _camera;
		[SerializeField] private bool scaleReticleWithDistance;
		[SerializeField] private float scaleFactor = 10;
		private float distance;

		new protected void Awake() {
			base.Awake();

			if (reticle == null && gameObject.scene.isLoaded) {
				reticle = Instantiate(reticle);
			}
		}

		new protected void OnEnable() {
			base.OnEnable();

			UpdateReticleStyle();

			generalSettings.OnReticleChange += UpdateReticleStyle;
		}

		new protected void OnDisable() {
			base.OnDisable();
			generalSettings.OnReticleChange -= UpdateReticleStyle;

			if (reticle != null && gameObject.scene.isLoaded) {
				Destroy(reticle);
			}
		}

		private void UpdateReticleStyle() {
			if (!gameObject.scene.isLoaded) {
				return;
			}
			if (reticle != null) {
				Destroy(reticle);
			}
			reticle = generalSettings.GetReticlePrefab();
			reticle.GetComponentInChildren<SpriteRenderer>().color = generalSettings.ReticleColor;
		}

		// https://forum.unity.com/threads/reticle-crosshair.374076/
		// https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.2/api/UnityEngine.XR.Interaction.Toolkit.XRRayInteractor.html#UnityEngine_XR_Interaction_Toolkit_XRRayInteractor_TryGetCurrent3DRaycastHit_UnityEngine_RaycastHit__
		/// <summary>
		/// We have to implement our own scaling for reticle
		/// </summary>
		private void FixedUpdate() {
			if (scaleReticleWithDistance && XRRayInteractor != null) {
				bool isHit = XRRayInteractor.TryGetHitInfo(out Vector3 _position, out _, out _, out _);
				if (isHit) {
					if (_camera != null) {
						distance = Vector3.Distance(_camera.position, _position);
					} else {
						distance = Vector3.Distance(transform.position, _position);
					}

					reticle.transform.localScale = scaleFactor * generalSettings.ReticleScale * distance * Vector3.one;
				}
			}
		}
	}
}