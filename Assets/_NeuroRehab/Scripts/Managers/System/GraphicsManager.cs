using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using NeuroRehab.ScriptObjects;
using NeuroRehab.Core;

namespace NeuroRehab.Managers {
	public class GraphicsManager : MonoBehaviour {
		private readonly CustomDebug customDebug = new("GraphicsManager");
		[Header("Scriptable objects")]
		[SerializeField] private CustomGraphicsSettings m_graphicsSettings;

		[Header("Dependencies")]
		[SerializeField] private Volume postprocessingVolume;

		// -------------------------------------------------------
		// POST-PROCESSING PROFILES
		private UniversalRenderPipelineAsset cachedRenderPipeline;

		private ColorAdjustments colorAdjustmentsProfile;
		private Tonemapping tonemappingProfile;
		private void Start() {
			InitPostprocessingVolume();
		}

		private void InitPostprocessingVolume() {
			postprocessingVolume.profile.TryGet(out colorAdjustmentsProfile);
			postprocessingVolume.profile.TryGet(out tonemappingProfile);

			cachedRenderPipeline = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;

			tonemappingProfile.active = m_graphicsSettings.Tonemapping;
		}

		private void OnEnable() {
			m_graphicsSettings.OnExposureChanged += UpdateExposure;
			m_graphicsSettings.OnTonemappingChanged += UpdateTonemapping;
			m_graphicsSettings.OnScreenResolutionChanged += UpdateScreenResolution;
			m_graphicsSettings.OnRenderScaleChanged += UpdateRenderScale;
			m_graphicsSettings.OnFramerateChanged += RefreshFrameRate;
		}

		private void OnDisable() {
			m_graphicsSettings.OnExposureChanged -= UpdateExposure;
			m_graphicsSettings.OnTonemappingChanged -= UpdateTonemapping;
			m_graphicsSettings.OnScreenResolutionChanged -= UpdateScreenResolution;
			m_graphicsSettings.OnRenderScaleChanged -= UpdateRenderScale;
			m_graphicsSettings.OnFramerateChanged -= RefreshFrameRate;
		}

		private void UpdateExposure() {
			colorAdjustmentsProfile.postExposure.value = m_graphicsSettings.Exposure;
		}

		private void UpdateTonemapping() {
			tonemappingProfile.active = m_graphicsSettings.Tonemapping;
		}

		private void UpdateScreenResolution() {
			Screen.SetResolution(m_graphicsSettings.ScreenWidth, m_graphicsSettings.ScreenHeight, m_graphicsSettings.Fullscreen);
		}

		private void RefreshFrameRate() {
			QualitySettings.vSyncCount = m_graphicsSettings.VSync ? 1 : 0;
			Application.targetFrameRate = m_graphicsSettings.FrameRate;
		}

		private void UpdateRenderScale() {
			if (!VerifyCachedRenderPipeline()) {
				customDebug.LogError("(UpdateRenderScale): Current Pipeline is null");
				return;
			}
			cachedRenderPipeline.renderScale = m_graphicsSettings.RenderScale;
		}

		private bool VerifyCachedRenderPipeline() {
			if ((UniversalRenderPipelineAsset)QualitySettings.renderPipeline == null) {
				return false;
			}

			if (cachedRenderPipeline != (UniversalRenderPipelineAsset)QualitySettings.renderPipeline) {
				cachedRenderPipeline = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
			}
			return true;
		}
	}
}