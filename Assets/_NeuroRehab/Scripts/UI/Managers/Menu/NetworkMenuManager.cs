using System.Collections;
using System.Collections.Generic;
using TMPro;
using Mirror;
using UnityEngine;
using Unity.Services.Vivox;
using UnityEngine.SceneManagement;

using NeuroRehab.Core;
using NeuroRehab.ScriptObjects;

namespace NeuroRehab.UI.Managers.Menu {
	public class NetworkMenuManager : MonoBehaviour {
		private CustomDebug customDebug = new("NETWORK_MENU_MANAGER");
		[Header("Scriptable objects")]
		[SerializeField] private CustomXRSettings xrSettings;
		[SerializeField] private NetworkSettings networkSettings;
		[SerializeField] private RoleSettings roleSettings;
		[SerializeField] private SceneChangeEventsSO sceneChangeEvents;

		[Header("Dependencies")]
		[SerializeField, NonReorderable] private LoadingElement[] loadingElements;

		public void DisconnectClient() {
			StartCoroutine(DisconnectCoroutine());
		}

		private IEnumerator DisconnectCoroutine() {
			sceneChangeEvents.StartSceneChange();
			yield return new WaitForSeconds(sceneChangeEvents.SceneChangeDuration);

			networkSettings.ClientDisconnect();
		}

		public void JoinTherapist() {
			roleSettings.CharacterRole = UserRole.Therapist;
			networkSettings.ClientJoin();

			ActivateLoadingElements();
		}

		public void JoinPatient() {
			xrSettings.XrActive = true;

			roleSettings.CharacterRole = UserRole.Patient;
			networkSettings.ClientJoin();

			ActivateLoadingElements();
		}

		private void ActivateLoadingElements() {
			foreach (LoadingElement item in loadingElements) {
				item.enabled = true;
			}
		}

		public void LoginToVivox() {
			if (VivoxService.Instance == null) {
				return;
			}
			VivoxService.Instance.JoinGroupChannelAsync(networkSettings.GetRoomId(), ChatCapability.AudioOnly);
		}


		public async void LogoutOfVivoxAsync() {
			if (VivoxService.Instance == null) {
				return;
			}
			if (VivoxService.Instance.IsLoggedIn) {
				await VivoxService.Instance.LogoutAsync();
			}
		}

		public void LeaveGroupChannel() {
			if (VivoxService.Instance == null) {
				return;
			}
			VivoxService.Instance.LeaveAllChannelsAsync();
		}

	}
}