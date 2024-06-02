using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

using NeuroRehab.Core;

namespace NeuroRehab.ScriptObjects {

	[CreateAssetMenu(menuName = "ScriptObjects/AnimationSettings")]
	public class AnimationSettingsSO : ScriptableObject {
		[SerializeField, NonReorderable] private List<AnimTypeDropdownListItem> animTypeOptions;
		[SerializeField, NonReorderable] private SerializedDictionary<AnimationType, TargetPrefab> targetPrefabs;

		private AnimationType animType = AnimationType.Block;
		private AnimationType prevAnimType = AnimationType.Block;
		private float armMoveDuration;
		private float handMoveDuration;
		private float evalDuration;
		private float restDuration;
		private float moveDuration;
		private int repetitions;
		private float waitDuration;
		private float keyTurnDuration;

		public event Action OnChangeArmMoveDuration;
		public event Action OnChangeHandMoveDuration;
		public event Action OnChangeEvalDuration;
		public event Action OnChangeRestDuration;
		public event Action OnChangeMoveDuration;
		public event Action OnChangeRepetitions;
		public event Action OnChangeAnimTypeValue;

		public event Action<Transform, Transform> OnLockPickedUp;
		public event Action<Transform> OnTargetPickedUp;
		public event Action<Transform> OnTargetReleased;

		public event Action<TrainingType> OnTrainingStarted;
		public event Action OnTrainingStopped;

		public List<PosRotMapping> CurrentSetup { get; set; } = new List<PosRotMapping>();
		public AnimationType AnimType { get => animType; set {
				if (animType == value) {
					return;
				}
				if (animType != AnimationType.Off) {
					PrevAnimType = animType;
				}
				animType = value;
				OnChangeAnimTypeValue?.Invoke();
			}
		}
		public AnimationType PrevAnimType { get => prevAnimType; set => prevAnimType = value; }

		public float ArmMoveDuration { get => armMoveDuration;
			set {
				if (armMoveDuration == value) {
					return;
				}
				armMoveDuration = value;
				OnChangeArmMoveDuration?.Invoke();
			}
		}
		public float HandMoveDuration { get => handMoveDuration;
			set {
				if (handMoveDuration == value) {
					return;
				}
				handMoveDuration = value;
				OnChangeHandMoveDuration?.Invoke();
			}
		}
		public float EvalDuration { get => evalDuration;
			set {
				if (evalDuration == value) {
					return;
				}
				evalDuration = value;
				OnChangeEvalDuration?.Invoke();
			}
		}
		public float RestDuration { get => restDuration;
			set {
				if (restDuration == value) {
					return;
				}
				restDuration = value;
				OnChangeRestDuration?.Invoke();
			}
		}
		public float MoveDuration { get => moveDuration;
			set {
				if (moveDuration == value) {
					return;
				}
				moveDuration = value;
				OnChangeMoveDuration?.Invoke();
			}
		}
		public int Repetitions { get => repetitions;
			set {
				if (repetitions == value) {
					return;
				}
				repetitions = value;
				OnChangeRepetitions?.Invoke();
			}
		}
		public float WaitDuration { get => waitDuration;
			set {
				waitDuration = value;
			}
		}
		public float KeyTurnDuration { get => keyTurnDuration;
			set {
				keyTurnDuration = value;
			}
		}

		public List<AnimTypeDropdownListItem> AnimTypeOptions { get => animTypeOptions; }
		public SerializedDictionary<AnimationType, TargetPrefab> TargetPrefabs { get => targetPrefabs; }

		public void TrainingStarted(TrainingType trainingType) {
			OnTrainingStarted?.Invoke(trainingType);
		}
		public void TrainingStopped() {
			OnTrainingStopped?.Invoke();
		}

		public void LockPickUp(Transform lockTransform, Transform keyPosTransform) {
			OnLockPickedUp?.Invoke(lockTransform, keyPosTransform);
		}

		public void TargetPickUp(Transform objTransform) {
			OnTargetPickedUp?.Invoke(objTransform);
		}
		public void TargetRelease(Transform objTransform) {
			OnTargetReleased?.Invoke(objTransform);
		}
	}
}