using System;
using UnityEngine;

namespace NeuroRehab.Core {
	[Serializable]
	public class GraphicsSettingsSerialized {
		[SerializeField] private float renderScale = 1.0f;
		[SerializeField] private float exposure = .0f;
		[SerializeField] private bool tonemapping = false;

		[SerializeField] private bool fullscreen = true;
		[SerializeField] private int screenWidth = 1920;
		[SerializeField] private int screenHeight = 1080;
		[SerializeField] private bool vSync = false;
		[SerializeField] private int frameRate = 144;

		public float RenderScale { get => renderScale; }
		public float Exposure { get => exposure; }
		public bool Tonemapping { get => tonemapping; }
		public bool Fullscreen { get => fullscreen; }
		public int ScreenWidth { get => screenWidth; }
		public int ScreenHeight { get => screenHeight; }
		public bool VSync { get => vSync; }
		public int FrameRate { get => frameRate; }

		public GraphicsSettingsSerialized(float renderScale, float exposure, bool tonemapping, bool fullscreen, int screenWidth, int screenHeight, bool vSync, int frameRate) {
			this.renderScale = renderScale;
			this.exposure = exposure;
			this.tonemapping = tonemapping;
			this.fullscreen = fullscreen;
			this.screenWidth = screenWidth;
			this.screenHeight = screenHeight;
			this.vSync = vSync;
			this.frameRate = frameRate;
		}
	}
}