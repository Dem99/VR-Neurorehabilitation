using System.Collections;
using UnityEngine;

namespace NeuroRehab.UI {
	public class CustomLog : MonoBehaviour {
		private string myLog;
		private readonly Queue myLogQueue = new();
		[SerializeField] private TMPro.TMP_Text text;

		void OnEnable () {
			Application.logMessageReceived += HandleLog;
			Debug.Log("Log initialized");
		}

		void OnDisable () {
			Application.logMessageReceived -= HandleLog;
		}

		void HandleLog(string logString, string stackTrace, LogType type){
			myLog = logString;
			string newString = "\n [" + type + "] : " + myLog;
			myLogQueue.Enqueue(newString);
			if (type == LogType.Exception) {
				newString = "\n" + stackTrace;
				myLogQueue.Enqueue(newString);
			}
			myLog = string.Empty;
			int logCount = myLogQueue.Count;
			for (int i = Mathf.Max(logCount - 15, 0); i < logCount; i++) {
				myLog += myLogQueue.ToArray()[i];
			}
			text.text = myLog;
		}
	}
}