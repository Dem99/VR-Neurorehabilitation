using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

using NeuroRehab.Core;
using NeuroRehab.Interfaces;

namespace NeuroRehab.ScriptObjects {

	[CreateAssetMenu(menuName = "ScriptObjects/Settings/General")]
	public class GeneralSettings : ScriptableObject, IColorContainer {
		[SerializeField] private bool m_measureFps = true;
		[SerializeField] private bool m_writeFpsToFile = false;

		[SerializeField] private float m_reticleScale = 1f;
		[SerializeField] private Color m_reticleColor = Color.white;
		[SerializeField] private ReticleStyle m_reticleStyle = ReticleStyle.EMPTY;
		[SerializeField] private CursorStyle m_cursorStyle = CursorStyle.SIMPLE;

		[Tooltip("List of reticle gameObject prefabs used by XR player objects")]
		[SerializeField] private SerializedDictionary<ReticleStyle, GameObject> m_reticlePrefabs;

		[SerializeField] private SerializedDictionary<ReticleStyle, LocalizedDropdownOption> reticleOptions;

		[SerializeField] private SerializedDictionary<CursorStyle, LocalizedDropdownOption> cursorOptions;

		public event Action OnMeasureFpsChange;
		public event Action OnReticleChange;
		public event Action OnCursorChange;

		public void Init(bool _measureFps, bool _writeToFile, Color _reticleColor, float _reticleScale, ReticleStyle _reticleStyle, CursorStyle _cursorStyle) {
			MeasureFps = _measureFps;
			WriteFpsToFile = _writeToFile;
			ReticleScale = _reticleScale;
			ReticleColor = _reticleColor;
			ReticleStyle = _reticleStyle;
			CursorStyle = _cursorStyle;
		}

		private void OnEnable() {
			OnCursorChange += SetCursor;
		}

		private void SetCursor() {
			Cursor.SetCursor(cursorOptions[CursorStyle].simpleSprite, Vector2.zero, CursorMode.Auto);
		}

		public GameObject GetReticlePrefab() {
			return m_reticlePrefabs[ReticleStyle];
		}

		public LocalizedDropdownOption GetReticleOption() {
			return reticleOptions[ReticleStyle];
		}

		public LocalizedDropdownOption GetCursorOption() {
			return cursorOptions[CursorStyle];
		}

		public bool MeasureFps {
			get => m_measureFps;
			set {
				m_measureFps = value;
				OnMeasureFpsChange?.Invoke();
			}
		}

		public bool WriteFpsToFile {
			get => m_writeFpsToFile;
			set {
				m_writeFpsToFile = value;
			}
		}

		public float ReticleScale {
			get => m_reticleScale;
			set {
				m_reticleScale = value;
				OnReticleChange?.Invoke();
			}
		}

		public Color ReticleColor {
			get => m_reticleColor;
			set {
				m_reticleColor = value;
				OnReticleChange?.Invoke();
			}
		}

		public ReticleStyle ReticleStyle {
			get => m_reticleStyle;
			set {
				m_reticleStyle = value;
				OnReticleChange?.Invoke();
			}
		}

		public CursorStyle CursorStyle {
			get => m_cursorStyle;
			set {
				m_cursorStyle = value;
				OnCursorChange?.Invoke();
			}
		}

		public SerializedDictionary<ReticleStyle, LocalizedDropdownOption> ReticleOptions { get => reticleOptions;}
		public SerializedDictionary<CursorStyle, LocalizedDropdownOption> CursorOptions { get => cursorOptions;}

		public List<LocalizedDropdownOption> GetReticles() {
			return reticleOptions.Values.ToList();
		}

		public List<LocalizedDropdownOption> GetCursors() {
			return cursorOptions.Values.ToList();
		}

		public void SetColor(Color color) {
				ReticleColor = color;
		}

		public Color GetColor() {
			return ReticleColor;
		}
	}
}