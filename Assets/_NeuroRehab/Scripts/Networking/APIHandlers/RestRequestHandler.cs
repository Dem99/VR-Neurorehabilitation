using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using Newtonsoft.Json.Linq;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts.Server;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts.Server.Extensions;
using System.Net;
using System.Collections.Generic;

using NeuroRehab.Core;
using NeuroRehab.ScriptObjects;
using NeuroRehab.Interfaces;

namespace NeuroRehab.Networking.API {
	/// <summary>
	/// Contains Rest Requests Handler methods.
	/// </summary>
	public class RestRequestHandler : MonoBehaviour {
		[Header("Scriptable objects")]
		[SerializeField] private NetworkSettings networkSettings;

		[Header("Dependencies")]
		[SerializeField] private SimpleEventServerScript restServerScript;
		private readonly CustomDebug customDebug = new("REST");
		private static bool isRestServerRunning = false;

		private void Start() {
			if (isRestServerRunning) {
				return;
			}
			// first we change port that we're using
			foreach (var item in restServerScript.ListeningAddresses) {
				item.Port = networkSettings.RestPort;
			}

			restServerScript.enabled = true;
			isRestServerRunning = true;

			customDebug.LogInfo($"Rest server running on port {networkSettings.RestPort}");
		}

		[Server]
		[SimpleEventServerRouting(HttpConstants.MethodPost, "/training/move")]
		public void PostTrainingMoveEndpoint(HttpListenerContext context) {
			customDebug.Log("[POST] - /training/move");
			var jsonObject = context.Request.GetJsonBody<RoomIDJsonBody>();
			if (jsonObject == null) {
				customDebug.LogWarning("Invalid Body!");
				return;
			}
			customDebug.Log($"{jsonObject}");

			Scene sceneToUse = networkSettings.GetSceneByRoomId(jsonObject.Value.roomID);
			SceneHelper sceneHelper = new(sceneToUse);
			IAnimationServerManager animServer = sceneHelper.FindRootObject<IAnimationServerManager>();
			if (animServer == null) {
				return;
			}
			bool returnVal = animServer.ProgressStep(ExerciseState.MOVE);

			context.Response.JsonResponse(new JObject() {
				new JProperty("result", returnVal)
			});
		}

		[Server]
		[SimpleEventServerRouting(HttpConstants.MethodPost, "/move")]
		public void PostMoveEndpoint(HttpListenerContext context) {
			customDebug.Log("[POST] - /move");
			var jsonObject = context.Request.GetJsonBody<RoomIDJsonBody>();
			if (jsonObject == null) {
				customDebug.LogWarning("Invalid Body!");
				return;
			}
			customDebug.Log($"{jsonObject}");

			Scene sceneToUse = networkSettings.GetSceneByRoomId(jsonObject.Value.roomID);
			SceneHelper sceneHelper = new(sceneToUse);
			IAnimationServerManager animServer = sceneHelper.FindRootObject<IAnimationServerManager>();
			if (animServer == null) {
				return;
			}

			bool returnVal;
			if (animServer.IsTrainingRunning) {
				returnVal = false;
			} else {
				animServer.RpcStartActualAnimation(false, "", false);
				returnVal = true;
			}

			context.Response.JsonResponse(new JObject() {
				new JProperty("result", returnVal)
			});
		}

		[Server]
		[SimpleEventServerRouting(HttpConstants.MethodPost, "/rest")]
		public void PostRestingEndpoint(HttpListenerContext context) {
			customDebug.Log("[POST] - /rest");
			bool returnVal = false;

			var jsonObject = context.Request.GetJsonBody<RoomIDJsonBody>();
			if (jsonObject == null) {
				customDebug.LogWarning("Invalid Body!");
				return;
			}
			customDebug.Log($"{jsonObject}");

			Scene sceneToUse = networkSettings.GetSceneByRoomId(jsonObject.Value.roomID);
			SceneHelper sceneHelper = new(sceneToUse);
			IAnimationServerManager animServer = sceneHelper.FindRootObject<IAnimationServerManager>();
			if (animServer == null) {
				return;
			}

			if (animServer.IsTrainingRunning) {
				returnVal = false;
			} else {
				IAnimationSettingsManager animSettings = sceneHelper.FindRootObject<IAnimationSettingsManager>();
				if (animServer == null) {
					return;
				}
				if (animSettings.SetAnimType(animSettings.Net_AnimType, AnimationType.Off)) {
					returnVal = true;

					animSettings.SpawnTarget(animSettings.Net_PrevAnimType, animSettings.Net_AnimType, jsonObject.Value.roomID);
				}
			}

			context.Response.JsonResponse(new JObject() {
				new JProperty("result", returnVal)
			});
		}

		[Server]
		[SimpleEventServerRouting(HttpConstants.MethodPost, "/spawn")]
		public void PostSpawnEndpoint(HttpListenerContext context) {
			customDebug.Log("[POST] - /spawn");
			bool returnVal = false;

			var jsonObject = context.Request.GetJsonBody<RoomIDJsonBody>();
			if (jsonObject == null) {
				customDebug.LogWarning("Invalid Body!");
				return;
			}
			customDebug.Log($"{jsonObject}");

			Scene sceneToUse = networkSettings.GetSceneByRoomId(jsonObject.Value.roomID);
			SceneHelper sceneHelper = new(sceneToUse);
			IAnimationServerManager animServer = sceneHelper.FindRootObject<IAnimationServerManager>();
			if (animServer == null) {
				return;
			}

			if (animServer.IsTrainingRunning) {
				returnVal = false;
			} else {
				IAnimationSettingsManager animSettings = sceneHelper.FindRootObject<IAnimationSettingsManager>();
				if (animServer == null) {
					return;
				}
				AnimationType _oldAnimType = animSettings.Net_AnimType;
				if (animSettings.SetAnimType(animSettings.Net_AnimType, animSettings.Net_PrevAnimType)) {
					returnVal = true;

					animSettings.SpawnTarget(_oldAnimType, animSettings.Net_AnimType, jsonObject.Value.roomID);
				}
			}

			context.Response.JsonResponse(new JObject() {
				new JProperty("result", returnVal)
			});
		}

		[Server]
		[SimpleEventServerRouting(HttpConstants.MethodGet, "/rooms")]
		public void GetRoomsEndpoint(HttpListenerContext context) {
			customDebug.Log("[GET] - /rooms");
			List<RoomData> rooms = networkSettings.GetRooms();

			RoomDataSerialized[] roomsSerialized = new RoomDataSerialized[rooms.Count];
			for (int i = 0; i < rooms.Count; i++) {
				roomsSerialized[i] = new RoomDataSerialized(rooms[i]);
			}

			// customDebug.Log(JsonHelper.ToJson(roomsSerialized));
			context.Response.JsonResponse(new JObject() {
				new JProperty("rooms", JsonHelper.ToJson(roomsSerialized))
			});
		}

		[Server]
		[SimpleEventServerRouting(HttpConstants.MethodGet, "/serverinfo")]
		public void GetServerInfoEndpoint(HttpListenerContext context) {
			customDebug.Log("[GET] - /serverinfo");
			ServerInfo serverInfo = new() {
				serverVersion = Application.version
			};

			context.Response.JsonResponse(new JObject() {
				new JProperty("serverVersion", serverInfo.serverVersion)
			});
		}
	}
}