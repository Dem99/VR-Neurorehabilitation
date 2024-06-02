using UnityEngine;

using NeuroRehab.Core;
using NeuroRehab.Interfaces;

namespace NeuroRehab.StateMachine.Training {
	public abstract class TrainingBase : IStateManager<TrainingType, ExerciseState> {
		protected ExerciseState m_exerciseState;
		protected readonly TrainingType m_trainingType;
		protected readonly MonoBehaviour m_anchor;
		protected readonly IAnimationServerManager m_animServer;
		protected readonly IAnimationSettingsManager m_animSettings;
		protected readonly string m_roomID;

		protected readonly CustomDebug m_customDebug;

		public TrainingBase(TrainingType trainingType, MonoBehaviour anchor, string roomID, string _customDebug) {
			m_customDebug = new(_customDebug);

			m_trainingType = trainingType;
			m_exerciseState = ExerciseState.N_A;

			m_anchor = anchor;
			m_animServer = anchor.GetComponent<IAnimationServerManager>();
			m_animSettings = anchor.GetComponent<IAnimationSettingsManager>();

			m_roomID = roomID;
		}

		public ExerciseState GetCurrentState() {
			return m_exerciseState;
		}

		public TrainingType GetMachineType() {
			return m_trainingType;
		}

		public abstract bool ProgressState(ExerciseState newState);

		public abstract bool Start(ExerciseState startState = ExerciseState.N_A);

		public abstract bool Stop();

		protected abstract bool CanTrainingStart();
	}
}