using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

using NeuroRehab.ScriptObjects;
using NeuroRehab.Core;

namespace NeuroRehab.UI {
	public class VersionError : MonoBehaviour {
		private readonly CustomDebug customDebug = new("VERSION_ERROR");
		[SerializeField] private NetworkSettings networkSettings;

		[SerializeField] private GameObject errorText;
		[SerializeField] private GameObject[] buttons;

		private int calls = 0;

		private void Start() {
			StartCoroutine(CheckVersion("http://" + networkSettings.IpAddress + ":" + networkSettings.RestPort + "/serverinfo"));
			errorText.SetActive(false);
		}

		IEnumerator CheckVersion(string uri) {
			yield return null; // wait one frame

			ServerInfo serverInfo = new();
			while (calls < 10) {
				calls++;
				using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)) {
					yield return webRequest.SendWebRequest();

					string[] pages = uri.Split('/');
					int page = pages.Length - 1;

					bool successfullRequest = false;
					try {
						switch (webRequest.result) {
							case UnityWebRequest.Result.ConnectionError:
							case UnityWebRequest.Result.DataProcessingError:
								customDebug.LogWarning($"{pages[page]} : Error: {webRequest.error} - src: '{gameObject.name}'");
								break;
							case UnityWebRequest.Result.ProtocolError:
								customDebug.LogWarning($"{pages[page]} : HTTP Error: {webRequest.error} - src: '{gameObject.name}'");
								break;
							case UnityWebRequest.Result.Success:
								serverInfo = JsonUtility.FromJson<ServerInfo>(webRequest.downloadHandler.text);

								successfullRequest = true;
								break;
						}
					} catch (System.Exception ex) {
						customDebug.LogError("Error occured while trying to parse response. See log trace:");
						Debug.LogError(ex);
					}

					if (successfullRequest) {
						break;
					}
				}

				yield return new WaitForSecondsRealtime(60f);
			}

			if (!serverInfo.Equals(default(ServerInfo))) {
				if (!serverInfo.serverVersion.Equals(Application.version)) {
					errorText.SetActive(true);
					foreach (var item in buttons) {
						item.SetActive(false);
					}
				}
			}
		}
	}
}