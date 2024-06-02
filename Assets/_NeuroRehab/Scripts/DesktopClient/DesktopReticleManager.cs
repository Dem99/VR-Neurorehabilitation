using UnityEngine;

using NeuroRehab.Utility;
using NeuroRehab.ScriptObjects;

namespace NeuroRehab.Desktop {
	public class DesktopReticleManager : MonoBehaviour {
		[SerializeField] private GeneralSettings generalSettings;
		[SerializeField] private SpriteRenderer spriteRenderer;

		private Vector3 initialScale;

		private void Awake() {
			initialScale = transform.localScale;
		}

		private void OnEnable() {
			UpdateReticle();

			generalSettings.OnReticleChange += UpdateReticle;
		}

		private void OnDisable() {
			generalSettings.OnReticleChange -= UpdateReticle;
		}

		private void UpdateReticle() {
			spriteRenderer.sprite = ConvertToSprite.ConvertTextureToSprite(generalSettings.GetReticleOption().simpleSprite);

			spriteRenderer.color = generalSettings.ReticleColor;
			transform.localScale = generalSettings.ReticleScale * initialScale;
		}
	}
}