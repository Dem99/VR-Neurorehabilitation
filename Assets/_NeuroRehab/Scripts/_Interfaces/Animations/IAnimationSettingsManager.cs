using System.Collections.Generic;
using Mirror;
using UnityEngine;

using NeuroRehab.Core;

namespace NeuroRehab.Interfaces {
	public interface IAnimationSettingsManager {
		public AnimationType Net_AnimType { get; }
		public AnimationType Net_PrevAnimType { get; }
		public float Net_armMoveDuration { get; }
		public float Net_handMoveDuration { get; }
		public float Net_evalDuration { get; }
		public float Net_restDuration { get; }
		public float Net_moveDuration { get; }
		public int Net_repetitions { get; }
		public uint Net_TargetId { get; }
		public uint Net_TargetLockId { get; }
		public SyncList<uint> Net_TargetLockIds { get; }
		public GameObject TargetFake { get; }
		// ---------------------------------------------------------------

		public SyncList<PosRotMapping> GetCurrentAnimSetup();

		public List<PosRotMapping> GetCurrentAnimSetupList();

		public SyncList<PosRotMapping> GetAnimSetupByAnimType(AnimationType AnimType);

		public List<SyncList<PosRotMapping>> GetAllAnimationSetups();

		/*
		* COMMANDS FOR SERVER - SYNCVARs
		*
		*/
		public void CmdUpdateArmDuration(float value);

		public void CmdUpdateHandDuration(float value);

		public void CmdUpdateEvalDuration(float value);

		public void CmdUpdateRestDuration(float value);

		public void CmdUpdateMoveDuration(float value);

		public void CmdUpdateRepetitions(int value);

		public void CmdUpdateAnimType(AnimationType _oldAnimType, AnimationType _newAnimType, NetworkConnectionToClient sender = null);

		public void TargetSetAnimTypeValue(NetworkConnection connection, AnimationType animationType);

		/// <summary>
		/// Changes animation type only if (activePatient && animation not playing)
		/// </summary>
		/// <param name="_old">Previous (current) animation type</param>
		/// <param name="_new">New animation type</param>
		/// <returns>Bool value depending on whether animation type was changed or not</returns>
		public bool SetAnimType(AnimationType _old, AnimationType _new, bool force = false);

		public void SpawnTarget(AnimationType _oldAnimType, AnimationType _newAnimType, string roomID = "");

		public void RpcSpawnCorrectTargetFakes(AnimationType _oldAnimType, AnimationType _newAnimType);

		/// <summary>
		/// Method called on server, old objects are then destroyed and new spawned on all clients. Refer to https://mirror-networking.gitbook.io/docs/guides/gameobjects/spawning-gameobjects for more details.
		/// </summary>
		/// <param name="_oldAnimType"></param>
		/// <param name="_newAnimType"></param>
		public void SpawnCorrectTarget(AnimationType _oldAnimType, AnimationType _newAnimType);

		/// <summary>
		/// Function used to destroy old objects and spawn new. Used only on client.
		/// </summary>
		/// <param name="_oldAnimType"></param>
		/// <param name="_newAnimType"></param>
		public void SpawnCorrectTargetFakes(AnimationType _oldAnimType, AnimationType _newAnimType);

		/// <summary>
		/// Function used for changing or adding start position for current chosen setup. Uses current position and rotation of target object
		/// </summary>
		/// <param name="forceNew">Used to determine whether we want to force rewrite first position no matter what</param>
		/// <param name="targetPosRotMapping">Custom position and rotation</param>
		public void SetAnimationStartPosition(bool forceNew, PosRotMapping targetPosRotMapping = null, NetworkConnectionToClient sender = null);


		/// <summary>
		/// Sets the lock position at the end of synclist
		/// </summary>
		/// <param name="lockTargetPosRot"></param>
		public void SetLockPosition(PosRotMapping lockTargetPosRot, uint lockId, NetworkConnectionToClient sender = null);

		/// <summary>
		/// Checks if object is above table AND in range of arm. If there is no active patient only checks if object is above table
		/// </summary>
		/// <param name="targetPosRotMapping"></param>
		/// <returns></returns>
		public bool IsTargetInBounds(PosRotMapping targetPosRotMapping, NetworkConnectionToClient sender = null);

		/// <summary>
		/// Helper method for getting posRot of current target object
		/// </summary>
		/// <returns></returns>
		public PosRotMapping GetPosRotFromCurrentObject();
	}
}