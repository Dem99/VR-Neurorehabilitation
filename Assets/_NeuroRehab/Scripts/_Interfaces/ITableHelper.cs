using UnityEngine;

namespace NeuroRehab.Interfaces {
	public interface ITableHelper {
		public Transform TherapistSitPosition { get; }
		public Transform PatientSitPosition { get; }
		public Transform LeftArmRestHelper { get; }
		public Transform RightArmRestHelper { get; }
		public Transform TargetSpawnArea { get; }
	}
}