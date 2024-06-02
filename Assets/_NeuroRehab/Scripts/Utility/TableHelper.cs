using UnityEngine;

using NeuroRehab.Interfaces;
using NeuroRehab.Core;

namespace NeuroRehab.Utility {
	public class TableHelper : MonoBehaviour, ITableHelper {
		[SerializeField] private Transform leftArmRestHelper;
		[SerializeField] private Transform rightArmRestHelper;
		[SerializeField] private Transform therapistSitPosition;
		[SerializeField] private Transform patientSitPosition;
		[SerializeField] private Transform targetSpawnArea;

		public Transform TherapistSitPosition { get => therapistSitPosition; }
		public Transform PatientSitPosition { get => patientSitPosition; }
		public Transform LeftArmRestHelper { get => leftArmRestHelper; }
		public Transform RightArmRestHelper { get => rightArmRestHelper; }
		public Transform TargetSpawnArea { get => targetSpawnArea; }

		void Awake() {
			SceneHelper sceneHelper = new(gameObject.scene);
			ISceneObjectManager sceneObjectManager = sceneHelper.FindRootObject<ISceneObjectManager>();

			sceneObjectManager.Table = this;
		}
	}
}