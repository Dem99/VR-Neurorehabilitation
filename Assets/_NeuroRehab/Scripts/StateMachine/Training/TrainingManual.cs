using UnityEngine;

using NeuroRehab.Core;

namespace NeuroRehab.StateMachine.Training {
	public class TrainingManual : TrainingBase {
		public TrainingManual(MonoBehaviour anchor, string roomID) : base(TrainingType.MANUAL, anchor, roomID, "TRAINING - MANUAL") {
			// empty
		}

		public override bool ProgressState(ExerciseState newState) {
			// customDebug.Log($"Transitioning from step: {m_exerciseState}");
			switch (newState) {
				case ExerciseState.REST: return HandleRestTransition();
				case ExerciseState.EVAL: return HandleEvalTransition();
				case ExerciseState.MOVE: return HandleMoveTransition();
				default: return false;
			}
		}

		public override bool Start(ExerciseState startState = ExerciseState.N_A) {
			if (!CanTrainingStart()) {
				return false;
			}

			bool retVal = ProgressState(startState);

			return retVal;
		}

		public override bool Stop() {
			return true;
		}

		protected override bool CanTrainingStart() {
			if (m_exerciseState == ExerciseState.MOVE) {
				return false;
			}

			return true;
		}

		private bool HandleRestTransition() {
			if (!(m_exerciseState == ExerciseState.EVAL || m_exerciseState == ExerciseState.N_A)) {
				return false;
			}

			if (m_animSettings.Net_AnimType != AnimationType.Off) {
				if (!m_animSettings.SetAnimType(m_animSettings.Net_AnimType, AnimationType.Off, true)) {
					m_customDebug.LogError("Failed to start Manual rest");
					// sender.RpcInformClients(Globals.LocalizedMessages.CantStartManual, MessageType.WARNING, true);
					return false;
				}
				m_animSettings.SpawnTarget(m_animSettings.Net_PrevAnimType, m_animSettings.Net_AnimType, m_roomID);
			}

			m_exerciseState = ExerciseState.REST;
			m_customDebug.Log($"New step: {m_exerciseState}");

			m_animServer.RpcStartTraining(CountdownMessageOp.MESSAGE_ONLY, 0f, "", Globals.LocalizedMessages.Rest, true, m_trainingType);
			m_animServer.StartRestPeriod(m_roomID);
			return true;
		}

		private bool HandleEvalTransition() {
			if (m_exerciseState == ExerciseState.EVAL) {
				return false;
			}

			if (m_animSettings.Net_AnimType == AnimationType.Off) {
				if (!m_animSettings.SetAnimType(m_animSettings.Net_AnimType, m_animSettings.Net_PrevAnimType, true)) {
					m_customDebug.LogError("Failed to start Manual evaluation");
					// sender.RpcInformClients(Globals.LocalizedMessages.CantStartManual, MessageType.WARNING, true);
					return false;
				}
				m_animSettings.SpawnTarget(m_animSettings.Net_PrevAnimType, m_animSettings.Net_AnimType, m_roomID);
			}
			m_exerciseState = ExerciseState.EVAL;
			m_customDebug.Log($"New step: {m_exerciseState}");

		 	m_animServer.RpcStartTraining(CountdownMessageOp.MESSAGE_ONLY, 0f, "", Globals.LocalizedMessages.Move, true, m_trainingType);

			m_animServer.StopRestPeriod(m_roomID);
			m_animServer.StartEegEvaluation(m_roomID);
			return true;
		}

		private bool HandleMoveTransition() {
			if (m_exerciseState != ExerciseState.EVAL) {
				return false;
			}
			m_exerciseState = ExerciseState.MOVE;
			m_customDebug.Log($"New step: {m_exerciseState}");

			m_customDebug.LogOk($"Starting arm animation in room: {m_roomID}");
			m_animServer.RpcStartActualAnimation(false, "", true);
			return true;
		}
	}
}