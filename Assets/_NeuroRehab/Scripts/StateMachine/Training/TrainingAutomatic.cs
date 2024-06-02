using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeuroRehab.Core;

namespace NeuroRehab.StateMachine.Training {
	public class TrainingAutomatic : TrainingBase {
		private DateTime m_lastTrainingStepTrigger;

		private int m_currentRepetitions = 0;

		private readonly List<Coroutine> m_coroutines = new();

		public TrainingAutomatic(MonoBehaviour anchor, string roomID) : base(TrainingType.AUTOMATIC, anchor, roomID, "TRAINING - AUTOMATIC") {
			// empty
		}

		/// <summary>
		/// Moves state to the next one
		/// </summary>
		/// <param name="newState"></param>
		/// <returns>True if succesfully transitioned into new state. False otherwise</returns>
		public override bool ProgressState(ExerciseState newState) {
			if (newState == ExerciseState.MOVE && m_exerciseState != ExerciseState.EVAL) {
				return false;
			}
			// customDebug.Log($"Transitioning from step: {m_exerciseState}");
			switch (m_exerciseState) {
				case ExerciseState.N_A: return HandleRestTransition();	// N_A 	-> Rest
				case ExerciseState.REST: return HandleEvalTransition();	// Rest	-> Eval
				case ExerciseState.EVAL: return HandleMoveTransition();	// Eval	-> Move
				case ExerciseState.MOVE: return HandleRestTransition();	// Move	-> Rest
				default: return false;
			}
		}

		/// <summary>
		/// We start exercise and automatically progress by one step, we ignore 'startState' in automatic
		/// </summary>
		/// <param name="startState"></param>
		/// <returns></returns>
		public override bool Start(ExerciseState startState = ExerciseState.N_A) {
			if (!CanTrainingStart()) {
				return false;
			}
			m_lastTrainingStepTrigger = DateTime.Now;

			StopAllCoroutines();

			bool retVal = ProgressState(ExerciseState.REST);
			if (retVal) {
				m_animServer.RpcStartTraining(CountdownMessageOp.COUNTDOWN, m_animSettings.Net_restDuration, "0/" + m_animSettings.Net_repetitions, Globals.LocalizedMessages.Rest, true, m_trainingType);

				m_customDebug.LogOk("Training - STARTED");
			}

			return retVal;
		}

		public override bool Stop() {
			StopAllCoroutines();
			return true;
		}

		protected override bool CanTrainingStart() {
			if (m_exerciseState != ExerciseState.N_A) {
				return false;
			}

			return true;
		}

		private void StopAllCoroutines() {
			foreach (var coroutine in m_coroutines) {
				if (coroutine != null) {
					m_anchor.StopCoroutine(coroutine);
				}
			}
			m_coroutines.Clear();
		}

		private bool HandleRestTransition() {
			if (!(m_exerciseState == ExerciseState.MOVE || m_exerciseState == ExerciseState.N_A)) {
				return false;
			}
			// Repetition count exceeded num of max repetitions
			if (m_exerciseState == ExerciseState.MOVE && m_currentRepetitions >= m_animSettings.Net_repetitions) {
				return false;
			}
			m_exerciseState = ExerciseState.REST;
			m_customDebug.Log($"New step: {m_exerciseState}");

			if (m_currentRepetitions > 0) {
				m_animServer.RpcStartCountdown(m_animSettings.Net_restDuration, $"{m_currentRepetitions}/{m_animSettings.Net_repetitions}", Globals.LocalizedMessages.Rest);
			}
			m_coroutines.Add(m_anchor.StartCoroutine(RestDurationCoroutine(m_animSettings.Net_restDuration)));
			return true;
		}

		private bool HandleEvalTransition() {
			if (m_exerciseState != ExerciseState.REST) {
				return false;
			}
			m_exerciseState = ExerciseState.EVAL;
			m_customDebug.Log($"New step: {m_exerciseState}");

			m_coroutines.Add(m_anchor.StartCoroutine(EvalDurationCoroutine(m_animSettings.Net_evalDuration)));

			m_animServer.RpcStartCountdown(m_animSettings.Net_evalDuration, $"{m_currentRepetitions}/{m_animSettings.Net_repetitions}", Globals.LocalizedMessages.Move);
			return true;
		}

		private bool HandleMoveTransition() {
			if (m_exerciseState != ExerciseState.EVAL) {
				return false;
			}

			DateTime currentTime = DateTime.Now;
			m_customDebug.Log($"Seconds since last animation step: {(currentTime - m_lastTrainingStepTrigger).TotalSeconds}s; wait duration: {m_animSettings.Net_evalDuration}s");

			// Should not happen, but checking just in case...
			if ((currentTime - m_lastTrainingStepTrigger).TotalSeconds > m_animSettings.Net_evalDuration) {
				m_animServer.CancelTraining();
				return false;
			}

			m_customDebug.LogOk($"Starting arm animation in room: {m_roomID}");
			m_exerciseState = ExerciseState.MOVE;
			m_customDebug.Log($"New step: {m_exerciseState}");

			StopAllCoroutines();
			m_animServer.RpcStartActualAnimation(false, $"{m_currentRepetitions + 1}/{m_animSettings.Net_repetitions}", true);

			m_currentRepetitions++;

			return true;
		}

		private IEnumerator RestDurationCoroutine(float duration) {
			m_lastTrainingStepTrigger = DateTime.Now;

			m_animSettings.SetAnimType(m_animSettings.Net_AnimType, AnimationType.Off, true);

			m_animSettings.SpawnTarget(m_animSettings.Net_PrevAnimType, m_animSettings.Net_AnimType, m_roomID);

			m_animServer.StartRestPeriod(m_roomID);
			yield return new WaitForSecondsRealtime(duration);

			m_animServer.StopRestPeriod(m_roomID);

			m_animSettings.SetAnimType(m_animSettings.Net_AnimType, m_animSettings.Net_PrevAnimType, true);

			m_animSettings.SpawnTarget(m_animSettings.Net_PrevAnimType, m_animSettings.Net_AnimType, m_roomID);

			yield return new WaitForSecondsRealtime(.3f); // Artificial slowdown - countdown looks more natural (otherwise it's too fast)
			ProgressState(ExerciseState.EVAL);
		}

		private IEnumerator EvalDurationCoroutine(float duration) {
			m_lastTrainingStepTrigger = DateTime.Now;

			m_animServer.StartEegEvaluation(m_roomID);
			yield return new WaitForSecondsRealtime(duration);

			// After wait duration passed we won't be waiting for 'Move' anymore
			m_exerciseState = ExerciseState.CANCELLED;
			m_animServer.StopEegEvaluation(m_roomID);

			m_customDebug.CustomLogColor(m_customDebug.color_orange, "Listening to animation events - Canceled - no activity!");

			m_animServer.CancelTraining();
		}
	}
}