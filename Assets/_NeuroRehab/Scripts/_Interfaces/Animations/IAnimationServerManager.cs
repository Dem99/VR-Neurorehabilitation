using Mirror;

using NeuroRehab.Core;

namespace NeuroRehab.Interfaces {
	public interface IAnimationServerManager {
		public bool IsTrainingRunning { get; set; }

		/// <summary>
		/// Starts specific training with specific start state
		/// </summary>
		/// <returns>True if we succesfully started Training. False otherwise</returns>
		[Server]
		public bool StartTraining(INetworkCharacter sender, TrainingType trainingType, ExerciseState startState);

		/// <summary>
		/// Cancels current training.
		/// </summary>
		[Server]
		public void CancelTraining();

		/// <summary>
		/// Stops training completely and ends all animations, informs connected websocket clients.
		/// </summary>
		[Server]
		public void StopTraining();

		/// <summary>
		/// Progresses step
		/// </summary>
		[Server]
		public bool ProgressStep(INetworkCharacter sender, ExerciseState newState);

		/// <summary>
		/// Progresses step
		/// </summary>
		[Server]
		public bool ProgressStep(ExerciseState newState);


		public void StartRestPeriod(string roomID);

		public void StopRestPeriod(string roomID);

		public void StartEegEvaluation(string roomID);

		public void StopEegEvaluation(string roomID);

		/*
		*
		* CLIENT RPC CALLS
		*
		*/

		[ClientRpc]
		public void RpcStartActualAnimation(bool isShowcase, string extraText, bool isTraining);

		[ClientRpc]
		public void RpcStopActualAnimation();

		[ClientRpc]
		public void RpcStartTraining(CountdownMessageOp messageOp, float countdownDuration, string extraText, string prefix, bool localizeString, TrainingType trainingType);

		[ClientRpc]
		public void RpcStartCountdown(float countdownDuration, string extraText, string prefix);

		[ClientRpc]
		public void RpcStartCountdown(float countdownDuration, string extraText, string prefix, bool localized);

		[ClientRpc]
		public void RpcStopTraining(string message, MessageType messageType, bool localized, Arg[] args);

		[ClientRpc]
		public void RpcStopTraining();
	}
}