using Mirror;
using UnityEngine;

using NeuroRehab.Core;

namespace NeuroRehab.Interfaces {
	public interface INetworkCharacter {

		/*
		*
		* MESSAGING
		*
		*/

		[ClientRpc]
		public void RpcMessageClients(string message, MessageType messageType, bool localizeString, Arg[] args);

		[ClientRpc]
		public void RpcMessageClients(string message, MessageType messageType, bool localizeString);

		/*
		*
		* ITEM PICKUP
		*
		*/
		[Client]
		public bool RequestOwnership(GameObject obj);

		/// <summary>
		/// Command wrapper to change item authority
		/// </summary>
		/// <param name="item"></param>
		/// <param name="sender"></param>
		[Command]
		public void CmdSetItemAuthority(NetworkIdentity item, NetworkConnectionToClient sender = null);

		/*
		*
		* SYNCING START / END POSITIONS
		*
		*/

		[Command]
		public void CmdSetAnimationStartPosition(bool forceNew, PosRotMapping mapping, NetworkConnectionToClient sender = null);

		/// <summary>
		/// Adds new move position at the end of synclist. Currently server assigns the value. In case there is an issue with correct position (server jitter), we could send the value over network from client.
		/// </summary>
		[Command]
		public void CmdAddMovePosition(NetworkConnectionToClient sender = null);

		/// <summary>
		/// Command wrapper for setting lock position
		/// </summary>
		/// <param name="lockTargetPosRot"></param>
		[Command]
		public void CmdSetLockPosition(PosRotMapping lockTargetPosRot, uint lockId, NetworkConnectionToClient sender = null);


		[Command]
		public void CmdClearMovePositions();

		/// <summary>
		/// Deletes last move position
		/// </summary>
		[Command]
		public void CmdDeleteMovePosition();

		/**
		*
		* TARGET OBJECT SPAWNING
		*
		*/

		/// <summary>
		/// Command wrapper to spawn correct target object
		/// </summary>
		/// <param name="_oldAnimType"></param>
		/// <param name="_newAnimType"></param>
		[Command]
		public void CmdSpawnCorrectTarget(AnimationType _oldAnimType, AnimationType _newAnimType, NetworkConnectionToClient sender = null);

		/**
		*
		* CALLING ANIMATIONS ON CLIENTS
		* server calls starts animation on every client separately, clients decide what to do with animation
		*
		*/

		[Command]
		public void CmdStartAnimationShowcase();

		[Command]
		public void CmdStartSingleAnimation();

		[Command]
		public void CmdStartTraining(TrainingType trainingType, ExerciseState state, NetworkConnectionToClient sender = null);

		[Command]
		public void CmdStopTraining();

		[Command]
		public void CmdCancelTraining();

		[Command]
		public void CmdProgressAnimationStep(ExerciseState state, NetworkConnectionToClient sender = null);


		/// <summary>
		/// Alternates arm resting state SyncVar, which calls hook on clients and causes other events
		/// </summary>
		/// <param name="patientId"></param>
		[Command]
		public void CmdSetArmRestPosition(uint patientId);

		[Command]
		public void CmdSetAnimationState(ArmAnimationState animationState, NetworkConnectionToClient sender = null);

		/*
		*
		* Setting up positioning
		*
		*/

		[Command]
		public void CmdMovePatientToSit(uint patientId);

		/// <summary>
		/// Method to teleport patient behind table AND rotate them to look in the direction 'PatientSitPositionObject' is rotated
		/// </summary>
		/// <param name="connection"></param>
		[TargetRpc]
		public void TargetMovePatientToSit(NetworkConnection connection);

		/// <summary>
		/// Method to move table up or down. On top of moving table we also have to change ALL markers and target object as well
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="sender"></param>
		[Command]
		public void CmdMoveTable(Vector3 offset, NetworkConnectionToClient sender = null);

		/// <summary>
		/// Moves rest position objects by offset
		/// </summary>
		/// <param name="offset"></param>
		[Command]
		public void CmdMoveArmRest(Vector3 offset);

		/// <summary>
		/// Moving target object(s) on client
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="offset"></param>
		[TargetRpc]
		public void TargetMoveObjects(NetworkConnection connection, Vector3 offset);

		/// <summary>
		/// Method to move patient object by offset
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="patientId"></param>
		[Command]
		public void CmdMovePatient(Vector3 offset, uint patientId);

		/*
		*
		* Setting active arm
		*
		*/

		/// <summary>
		/// Changes patients active arm. Also changes current objects rotation if necessary
		/// </summary>
		/// <param name="isLeftAnimated"></param>
		/// <param name="patientId"></param>
		/// <param name="sender"></param>
		[Command]
		public void CmdSetActiveArm(bool isLeftAnimated, uint patientId, NetworkConnectionToClient sender = null);

		[Command]
		public void CmdFixArms(bool isFix, uint patientId, NetworkConnectionToClient sender = null);
	}
}
