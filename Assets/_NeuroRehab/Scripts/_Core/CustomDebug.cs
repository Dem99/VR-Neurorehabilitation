using UnityEngine;

namespace NeuroRehab.Core {
	public class CustomDebug {
		// TODO Unity offers way to hide methods in Stack Trace in Console, but only in 2022+ version. After upgrade should be added to methods
		public static LogLevel GlobalLogLevel = LogLevel.ALL;

		private string tag = "";
		private LogLevel localLogLevel;
		public readonly string color_red = "#ff7979";
		public readonly string color_white = "#FFFFFF";
		public readonly string color_yellow = "#FFF200";
		public readonly string color_blue = "#379bff";
		public readonly string color_green = "#00a400";
		public readonly string color_orange = "#e08725";
		public readonly string color_blue_neon = "#5bd7d7";
		public readonly string color_purple = "#850f88";

		public CustomDebug() {
			localLogLevel = LogLevel.ALL;
		}

		public CustomDebug(string _tag) {
			tag = _tag;
			localLogLevel = LogLevel.ALL;
		}

		public CustomDebug(string _tag, LogLevel _localLogLevel) {
			tag = _tag;
			localLogLevel = _localLogLevel;
		}

		/*
		*
		* MAIN LOG FUNCTIONS
		*
		*/

		public void LogError(string message) {
			if (localLogLevel < LogLevel.ERROR || GlobalLogLevel < LogLevel.ERROR) {
				return;
			}
			Debug.LogError(FormatOutput(color_red, message));
		}

		public void LogWarning(string message) {
			if (localLogLevel < LogLevel.WARNING || GlobalLogLevel < LogLevel.WARNING) {
				return;
			}
			Debug.LogWarning(FormatOutput(color_yellow, message));
		}

		public void CustomLogColor(string color, string message) {
			if (localLogLevel < LogLevel.ALL || GlobalLogLevel < LogLevel.ALL) {
				return;
			}
			Debug.Log(FormatOutput(color, message));
		}

		/*
		*
		* HELPER FUNCTIONS (these call main functions)
		*
		*/
		public void Log(string message) {
			CustomLogColor(color_white, message);
		}

		public void Log(object obj) {
			Log(obj.ToString());
		}

		public void Log(Object obj) {
			Log(obj.ToString());
		}

		public void LogWarning(object message) {
			LogWarning(message.ToString());
		}

		public void LogWarning(Object message) {
			LogWarning(message.ToString());
		}

		public void LogError(object message) {
			LogError(message.ToString());
		}

		public void LogError(Object message) {
			LogError(message.ToString());
		}

		public void LogOk(string message) {
			CustomLogColor(color_green, message);
		}

		public void LogInfo(string message) {
			CustomLogColor(color_blue, message);
		}

		public void CustomLogColor(string color, object message) {
			CustomLogColor(color, message.ToString());
		}

		private string FormatOutput(string color, string message) {
			message = tag.Length > 0 ? $"[{tag}] - {message}" : message;
#if UNITY_EDITOR
			return $"<color={color}>{message}</color>";
#else
			return $"{message}";
#endif
		}

		private string FormatOutput(string color, object message) {
			message = tag.Length > 0 ? $"[{tag}] - {message}" : message;
#if UNITY_EDITOR
			return $"<color={color}>{message}</color>";
#else
			return $"{message}";
#endif
		}

		/// <summary>
		/// Helper method for printing vectors with more decimal places
		/// </summary>
		/// <param name="message">Vector printed</param>
		/// <param name="type">Not mandatory: 1 - Log; 2 - Warning; 3 - Error</param>
		public static void PrintVector3(Vector3 message, int type = 1) {
			if (type == 1)
				Debug.Log("X: " + message.x + " Y: " + message.y + " Z:" + message.z);
			if (type == 2)
				Debug.LogWarning("X: " + message.x + " Y: " + message.y + " Z:" + message.z);
			if (type == 3)
				Debug.LogError("X: " + message.x + " Y: " + message.y + " Z:" + message.z);
		}
	}
}
