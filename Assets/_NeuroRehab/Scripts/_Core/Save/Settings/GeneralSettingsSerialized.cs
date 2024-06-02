using System;
using UnityEngine;

namespace NeuroRehab.Core {
	[Serializable]
	public class GeneralSettingsSerialized {
		[SerializeField] private bool measureFps = true;
		[SerializeField] private bool writeFpsToFile = false;

		[SerializeField] private float reticleScale = 1f;
		[SerializeField] private Color reticleColor = Color.white;
		[SerializeField] private ReticleStyle reticleStyle = ReticleStyle.EMPTY;
		[SerializeField] private CursorStyle cursorStyle = CursorStyle.SIMPLE;

		public GeneralSettingsSerialized(bool _measureFps, bool _writeToFile, Color _reticleColor, float _reticleScale, ReticleStyle _reticleStyle, CursorStyle _cursorStyle) {
			measureFps = _measureFps;
			writeFpsToFile = _writeToFile;
			reticleScale = _reticleScale;
			reticleColor = _reticleColor;
			reticleStyle = _reticleStyle;
			cursorStyle = _cursorStyle;
		}

		public bool MeasureFps { get => measureFps;}
		public bool WriteFpsToFile { get => writeFpsToFile;}
		public Color ReticleColor { get => reticleColor;}
		public float ReticleScale { get => reticleScale; }
		public ReticleStyle ReticleStyle { get => reticleStyle;}
		public CursorStyle CursorStyle { get => cursorStyle;}
	}
}