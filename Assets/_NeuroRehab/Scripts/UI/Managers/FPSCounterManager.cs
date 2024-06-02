using TMPro;
using UnityEngine;
using System.IO;

using NeuroRehab.ScriptObjects;

namespace NeuroRehab.UI.Managers {
	public class FPSCounterManager : MonoBehaviour {
		[SerializeField] GeneralSettings generalSettings;
		[SerializeField] CustomXRSettings xrSettings;
		[SerializeField] private TMP_Text textField;
		[SerializeField][Range(0.001f, 10f)] private float refreshRate = 1f;

		private float m_uiUpdateTime;
		private string m_fpsCounterFilePath;
		private int m_frameRate;

		private bool initialized = false;

		void OnApplicationQuit() {
			if (generalSettings.MeasureFps && generalSettings.WriteFpsToFile && Application.platform != RuntimePlatform.Android) {
				System.Diagnostics.Process.Start(m_fpsCounterFilePath);
			}
		}

		void Start() {
			m_fpsCounterFilePath = Application.persistentDataPath + "/fps.txt";

			if (!generalSettings.MeasureFps) {
				enabled = false;
			}

			StreamWriter writer = new(m_fpsCounterFilePath, false);
			writer.WriteLine("");
			writer.Close();
		}

		private void OnEnable() {
			if (initialized) {
				return;
			}

			initialized = true;
			generalSettings.OnMeasureFpsChange += TriggerElement;
		}

		private void OnDestroy() {
			generalSettings.OnMeasureFpsChange -= TriggerElement;
		}

		private void OnDisable() {
			if (textField) {
				textField.text = "";
			}
		}

		private void TriggerElement() {
			enabled = generalSettings.MeasureFps;
		}

		private void Update() {
			if (Time.unscaledTime > m_uiUpdateTime) {
				m_frameRate = (int)(1f / Time.unscaledDeltaTime);
				textField.text = $"FPS: {m_frameRate}";
				m_uiUpdateTime = Time.unscaledTime + refreshRate;
				if (generalSettings.WriteFpsToFile) {
					WriteFps();
				}
			}
		}

		protected void OnGUI() {
			if (xrSettings.XrActive) return;
			if (!generalSettings.MeasureFps) return;

			GUIStyle style = new();
			style.normal.textColor = Color.black;
			style.fontSize = 18;
			GUI.Label(new Rect(Screen.width - 80, 10, 70, 25), $"FPS: {m_frameRate}", style);
		}

		private void WriteFps() {
			StreamWriter writer = new(m_fpsCounterFilePath, true);
			writer.WriteLine($"{m_frameRate}");
			writer.Close();
		}
	}
}