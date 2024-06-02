using System.Collections.Generic;
using UnityEngine;
using Mirror;

using NeuroRehab.Core;
using NeuroRehab.Networking;
using NeuroRehab.StateMachine.Training;
using NeuroRehab.ScriptObjects;
using NeuroRehab.ObjectProvider;
using NeuroRehab.Interfaces;

namespace NeuroRehab.Managers.Animations {

	[RequireComponent(typeof(AnimationSettingsManager))]
	public class AnimationServerManager : NetworkBehaviour, IAnimationServerManager {
		private readonly CustomDebug m_customDebug = new("ANIM_SERVER");

		[Header("Scriptable objects")]
		[SerializeField] private RoleSettings roleSettings;
		[SerializeField] private NetworkSettings networkSettings;
		[SerializeField] private AnimationSettingsSO animSettings;
		[SerializeField] private MessageEventsSO messageEvents;
		[SerializeField] private CountdownEventsSO countdownEvents;

		[Header("Dependencies")]
		[SerializeField] private AnimationSettingsManager m_animSettingsMan;
		// ---------------------------------------------------------------
		private IObjectProvider objectProvider;
		private SceneHelper sceneHelper;
		private ISceneObjectManager sceneObjectManager;
		private string roomID;

		private INetApi webSocketHandler;

		// not a syncvar intentionally, so that server and clients can use the value separately, on server just info for developers
		private TrainingBase m_currentTraining = null;
		// only usable on Server
		private bool m_isTrainingRunning = false;

		public bool IsTrainingRunning { get => m_isTrainingRunning; set => m_isTrainingRunning = value; }

		private void Awake() {
			sceneHelper = new(gameObject.scene);
			sceneObjectManager = sceneHelper.FindRootObject<ISceneObjectManager>();

			sceneObjectManager.AnimServer = this;
		}

		private void Start() {
			webSocketHandler = ((CustomNetworkManager)NetworkManager.singleton).WebSocketHandler;

			objectProvider = new NetworkObjectProvider(isServer);

			if (isServer) {
				roomID = networkSettings.GetRoomIdByScene(gameObject.scene);
			} else {
				roomID = networkSettings.GetRoomId();
			}
		}

		/// <summary>
		/// Starts specific training with specific start state
		/// </summary>
		/// <returns>True if we succesfully started Training. False otherwise</returns>
		[Server]
		public bool StartTraining(INetworkCharacter sender, TrainingType trainingType, ExerciseState startState) {
			switch (trainingType) {
				case TrainingType.AUTOMATIC:
					if (m_currentTraining != null) {
						sender.RpcMessageClients(Globals.LocalizedMessages.TrainingAlreadyStarted, MessageType.WARNING, true);
						return false;
					}
					if (!CanTrainingStart(sender)) {
						return false;
					}
					m_currentTraining = new TrainingAutomatic(this, roomID);
					m_customDebug.LogOk("Training - STARTED");

					m_isTrainingRunning = true;

					break;
				case TrainingType.MANUAL:
					if (m_currentTraining == null) {
						if (!CanTrainingStart(sender)) {
							return false;
						}
						m_currentTraining = new TrainingManual(this, roomID); // if (m_currentTraining == null) then set value
						m_customDebug.LogOk("Training - STARTED");

						m_isTrainingRunning = true;
					}
					break;
				default:
					sender.RpcMessageClients(Globals.LocalizedMessages.TrainingCantStart, MessageType.WARNING, true);
					return false;
			}

			return m_currentTraining.Start(startState);
		}

		/// <summary>
		/// Cancels current training.
		/// </summary>
		[Server]
		public void CancelTraining() {
			if (m_currentTraining == null) {
				return;
			}

			RpcStopTraining(Globals.LocalizedMessages.TrainingCancelled, MessageType.WARNING, true, null);
			m_customDebug.CustomLogColor(m_customDebug.color_orange, "Training - CANCELLED");

			m_currentTraining.Stop();
			m_currentTraining = null;

			m_isTrainingRunning = false;
		}

		/// <summary>
		/// Stops training completely and ends all animations, informs connected websocket clients.
		/// </summary>
		[Server]
		public void StopTraining() {
			if (m_currentTraining == null) {
				return;
			}
			if (m_currentTraining.GetCurrentState() == ExerciseState.REST) {
				webSocketHandler.StopRestPeriod(roomID);
			} else if (m_currentTraining.GetCurrentState() == ExerciseState.EVAL) {
				webSocketHandler.StopEegEvaluation(roomID);
			}
			m_customDebug.LogInfo("Training - STOPPED");
			RpcStopTraining();

			m_currentTraining.Stop();
			m_currentTraining = null;

			m_isTrainingRunning = false;
		}

		/// <summary>
		/// Progresses step
		/// </summary>
		[Server]
		public bool ProgressStep(INetworkCharacter sender, ExerciseState newState) {
			if (m_currentTraining == null) {
				sender.RpcMessageClients(Globals.LocalizedMessages.TrainingNone, MessageType.WARNING, true);
				return false;
			}
			return ProgressStep(newState);
		}

		/// <summary>
		/// Progresses step
		/// </summary>
		[Server]
		public bool ProgressStep(ExerciseState newState) {
			if (m_currentTraining == null) {
				// TODO does this need to work here? if so, how to make it work
				//sender.RpcInformClients(Globals.LocalizedMessages.TrainingNone, MessageType.WARNING, true);
				return false;
			}
			bool isOk = m_currentTraining.ProgressState(newState);

			// if move fails we don't stop training - maybe we received request too early
			if (!isOk && newState != ExerciseState.MOVE) {
				StopTraining();
			}

			return isOk;
		}

		[Server]
		private bool CanTrainingStart(INetworkCharacter sender) {
			if (sceneObjectManager.ActivePatient == null) {
				m_customDebug.LogWarning("No patient present! Can't start training.");
				sender.RpcMessageClients(Globals.LocalizedMessages.NoPatientPresent, MessageType.WARNING, true);
				return false;
			}

			if (m_animSettingsMan.Net_AnimType == AnimationType.Off) {
				m_customDebug.LogError("No animationy type chosen! Can't start training.");
				sender.RpcMessageClients(Globals.LocalizedMessages.NoAnimationChosen, MessageType.WARNING, true);
				return false;
			}

			List<PosRotMapping> currentAnimationSetup = m_animSettingsMan.GetCurrentAnimSetupList();
			if (currentAnimationSetup.Count < 1) {
				m_customDebug.LogError($"Too few animation positions set: '{currentAnimationSetup.Count}'!");
				sender.RpcMessageClients(Globals.LocalizedMessages.TooFewPositions, MessageType.WARNING, true, new Arg[] { new("0", currentAnimationSetup.Count) });
				return false;
			}
			if (m_animSettingsMan.Net_AnimType == AnimationType.Key && currentAnimationSetup.Count != 2) {
				m_customDebug.LogError("'Key' animation requires '2' positions set!");
				sender.RpcMessageClients(Globals.LocalizedMessages.TooFewPositionsForKey, MessageType.WARNING, true);
				return false;
			}
			return true;
		}

		public void StartRestPeriod(string roomID) {
			webSocketHandler.StartRestPeriod(roomID);
		}

		public void StopRestPeriod(string roomID) {
			webSocketHandler.StopRestPeriod(roomID);
		}

		public void StartEegEvaluation(string roomID) {
			webSocketHandler.StartEegEvaluation(roomID);
		}

		public void StopEegEvaluation(string roomID) {
			webSocketHandler.StopEegEvaluation(roomID);
		}

		/*
		*
		* CLIENT RPC CALLS
		*
		*/

		[ClientRpc]
		public void RpcStartActualAnimation(bool isShowcase, string extraText, bool isTraining) {
			if (sceneObjectManager.ActivePatient == null) {
				messageEvents.ShowMessage(new(Globals.LocalizedMessages.NoPatientPresent, MessageType.WARNING, true));
				return;
			}

			GameObject targetObj;
			if (isShowcase) {
				targetObj = m_animSettingsMan.TargetFake;
			} else {
				targetObj = objectProvider.GetObject(m_animSettingsMan.Net_TargetId);
			}
			GameObject lockObj = objectProvider.GetObject(m_animSettingsMan.Net_TargetLockId);
			bool animationExecuted = sceneObjectManager.ActivePatient.StartAnimation(isShowcase, isTraining, targetObj.transform, lockObj == null ? null : lockObj.transform);

			if (!isShowcase && !animationExecuted) {
				ClientStopTraining();

				if (roleSettings.CharacterRole == UserRole.Patient) {
					sceneObjectManager.LocalNetChar.CmdCancelTraining();
				}
			}

			countdownEvents.PauseCountdown(extraText);
		}

		[ClientRpc]
		public void RpcStopActualAnimation() {
			if (sceneObjectManager.ActivePatient == null) {
				return;
			}

			sceneObjectManager.ActivePatient.StopAnimation();
		}

		[ClientRpc]
		public void RpcStartTraining(CountdownMessageOp messageOp, float countdownDuration, string extraText, string prefix, bool localizeString, TrainingType trainingType) {
			if (messageOp == CountdownMessageOp.COUNTDOWN) {
				countdownEvents.StartCountdown(countdownDuration, extraText, prefix, localizeString);
			} else if (messageOp == CountdownMessageOp.MESSAGE_ONLY) {
				countdownEvents.SetCountdownMessage(prefix, localizeString);
			}
			if (!m_isTrainingRunning) {
				messageEvents.ShowMessage(new(Globals.LocalizedMessages.TrainingStarted, MessageType.SUCCESS, true));
				m_isTrainingRunning = true;
			}

			animSettings.TrainingStarted(trainingType);
		}

		[ClientRpc]
		public void RpcStartCountdown(float countdownDuration, string extraText, string prefix) {
			countdownEvents.StartCountdown(countdownDuration, extraText, prefix, true);
		}

		[ClientRpc]
		public void RpcStartCountdown(float countdownDuration, string extraText, string prefix, bool localized) {
			countdownEvents.StartCountdown(countdownDuration, extraText, prefix, localized);
		}

		[ClientRpc]
		public void RpcStopTraining(string message, MessageType messageType, bool localized, Arg[] args) {
			ClientStopTraining(message, messageType, localized, args);
		}

		[ClientRpc]
		public void RpcStopTraining() {
			ClientStopTraining(Globals.LocalizedMessages.TrainingStopped, MessageType.NORMAL, true, null);
		}

		[Client]
		private void ClientStopTraining(string message, MessageType messageType, bool localized, Arg[] args) {
			ClientStopTraining();

			messageEvents.ShowMessage(new(message, messageType, localized, args));
		}

		[Client]
		private void ClientStopTraining() {
			countdownEvents.StopCountdown();

			animSettings.TrainingStopped();

			m_isTrainingRunning = false;
		}

	}
}