using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using System.Collections.Generic;

using NeuroRehab.ScriptObjects;
using NeuroRehab.Core;

namespace NeuroRehab.UI {
	/// <summary>
	/// Message manager that contains list of all statusMessage objects, used to show/hide messages.
	/// </summary>
	public class MessageManager : MonoBehaviour {
		private readonly CustomDebug customDebug = new("MESSAGE_MANAGER");
		[SerializeField] private MessageEventsSO messageEvents;
		[SerializeField] private List<StatusMessage> statusMessageElements = new();

		private void OnEnable() {
			messageEvents.OnShowMessage += ShowMessage;
			messageEvents.OnHideMessage += HideMessage;
		}

		private void OnDisable() {
			messageEvents.OnShowMessage -= ShowMessage;
			messageEvents.OnHideMessage -= HideMessage;;
		}

		private void ShowMessage(CustomMessage message) {
			string messageToShow = message.MessageVal;
			if (message.LocalizeString) {
				LocalizedString locString = new("Messages", message.MessageVal);
				if (message.Args != null) {
					foreach (Arg arg in message.Args) {
						if (arg.TryGetReadableNameValuePair(out (string name, string value) nameValuePair)) {
							locString.Add(nameValuePair.name, new StringVariable {Value = nameValuePair.value});
						}
					}
				}

				messageToShow = locString.GetLocalizedString();
			}

			ShowMessageOnElements(new (messageToShow, message.MessageType));
		}


		private void ShowMessageOnElements(CustomMessage message) {
			customDebug.Log($"'{message.MessageType}' - {message.MessageVal}");
			foreach (StatusMessage statusMessageManager in statusMessageElements) {
				statusMessageManager.ShowMessage(message.MessageVal, message.MessageType);
			}
		}

		private void HideMessage() {
			foreach (StatusMessage statusMessageManager in statusMessageElements) {
				statusMessageManager.HideMessage();
			}
		}
	}
}