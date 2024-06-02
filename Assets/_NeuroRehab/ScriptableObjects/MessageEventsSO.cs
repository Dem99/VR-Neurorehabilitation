using System;
using UnityEngine;

using NeuroRehab.Core;

namespace NeuroRehab.ScriptObjects {

	[CreateAssetMenu(menuName = "ScriptObjects/MessageEvents")]
	public class MessageEventsSO : ScriptableObject {
		public event Action<CustomMessage> OnShowMessage;
		public event Action OnHideMessage;

		public void ShowMessage(CustomMessage message) {
			OnShowMessage?.Invoke(message);
		}

		public void HideMessage() {
			OnHideMessage?.Invoke();
		}
	}
}