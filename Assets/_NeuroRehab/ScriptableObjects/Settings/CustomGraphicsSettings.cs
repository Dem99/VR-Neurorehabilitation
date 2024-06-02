using System;
using UnityEngine;

namespace NeuroRehab.ScriptObjects {

	[CreateAssetMenu(menuName = "ScriptObjects/Settings/Graphics")]
	public class CustomGraphicsSettings : ScriptableObject {
		[SerializeField] private float renderScale = 1.0f;
		[SerializeField] private float exposure = .0f;
		[SerializeField] private bool tonemapping = false;
		[SerializeField] private bool fullscreen = true;
		[SerializeField] private int screenWidth = 1920;
		[SerializeField] private int screenHeight = 1080;

		[SerializeField] private bool vSync = false;
		[SerializeField] private int frameRate = 144;

		private readonly int[] supportedFrameRates = { 30, 60, 75, 100, 144, 165, 200 };

		public event Action OnExposureChanged;
		public event Action OnTonemappingChanged;
		public event Action OnScreenResolutionChanged;

		public event Action OnRenderScaleChanged;
		public event Action OnFramerateChanged;

		public void Init(float renderCale, float exposure, bool tonemapping, bool _fullscreen, int screenWidth, int screenHeight, bool vSync, int frameRate) {
			RenderScale = renderCale;
			Exposure = exposure;
			Tonemapping = tonemapping;

			fullscreen = _fullscreen;
			SetResolution(screenWidth, screenHeight);

			VSync = vSync;
			FrameRate = frameRate;
		}

		public float Exposure {
			get => exposure;
			set {
				exposure = value;
				OnExposureChanged?.Invoke();
			}
		}
		public bool Tonemapping {
			get => tonemapping;
			set {
				tonemapping = value;
				OnTonemappingChanged?.Invoke();
			}
		}
		public bool Fullscreen {
			get => fullscreen;
			set {
				fullscreen = value;
				OnScreenResolutionChanged?.Invoke();
			}
		}
		public int ScreenWidth { get => screenWidth; }
		public int ScreenHeight { get => screenHeight; }
		public float RenderScale {
			get => renderScale;
			set {
				renderScale = Mathf.Clamp(value, 0.5f, 2);
				OnRenderScaleChanged?.Invoke();
			}
		}
		public int[] SupportedFrameRates { get => supportedFrameRates; }
		public bool VSync {
			get => vSync;
			set {
				vSync = value;
				OnFramerateChanged?.Invoke();
			}
		}
		public int FrameRate {
			get => frameRate;
			set {
				frameRate = value;
				OnFramerateChanged?.Invoke();
			}
		}

		public void SetResolution(int width, int height) {
			screenWidth = width;
			screenHeight = height;

			OnScreenResolutionChanged?.Invoke();
		}

		// Doesn't work in Editor (because it's editor, it enforces width/height on it's own)
		public void RefreshScreenResolution() {
			OnScreenResolutionChanged?.Invoke();
		}

		public void RefreshFrameRate() {
			OnFramerateChanged?.Invoke();
		}
	}
}