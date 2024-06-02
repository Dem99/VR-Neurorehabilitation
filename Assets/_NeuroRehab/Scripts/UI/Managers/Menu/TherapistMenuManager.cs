using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using NeuroRehab.Core;
using NeuroRehab.Interfaces;
using NeuroRehab.ScriptObjects;
using NeuroRehab.UI.Localization;

namespace NeuroRehab.UI.Managers.Menu {
	/// <summary>
	/// Class containing handlers for UI elements - Therapist Menu
	/// </summary>
	public class TherapistMenuManager : MonoBehaviour {

		[SerializeField] private AnimationSettingsSO animSettings;
		[SerializeField] private MessageEventsSO messageEvents;

		[SerializeField] List<CanvasGroup> animationCanvases = new();
		[SerializeField] private CanvasGroup manualCanvasGroup;
		[SerializeField] private CanvasGroup automaticCanvasGroup;

		[Header("UI sliders")]
		[SerializeField] private TMP_Text armMoveTextValue;
		[SerializeField] private Slider armMoveSlider;

		[SerializeField] private TMP_Text handMoveTextValue;
		[SerializeField] private Slider handMoveSlider;

		[SerializeField] private TMP_Text evalDurTextValue;
		[SerializeField] private Slider evalDurSlider;

		[SerializeField] private TMP_Text restDurTextValue;
		[SerializeField] private Slider restDurSlider;

		[SerializeField] private TMP_Text moveDurTextValue;
		[SerializeField] private Slider moveDurSlider;

		[SerializeField] private TMP_Text repetitionsTextValue;
		[SerializeField] private Slider repetitionsSlider;

		[SerializeField] private Button registerMovePositionButton;

		[SerializeField] private TMP_Dropdown animTypeDropdown;

		private ISceneObjectManager sceneObjectManager;
		private SceneHelper sceneHelper;

		private void Awake() {
			sceneHelper = new(gameObject.scene);

			sceneObjectManager = sceneHelper.FindRootObject<ISceneObjectManager>();
		}

		private void Start() {
			SetAllElementsValues();
		}

		private void OnEnable() {
			animSettings.OnTrainingStarted += TrainingStarted;
			animSettings.OnTrainingStopped += TrainingStopped;

			animSettings.OnChangeAnimTypeValue += ChangeAnimTypeValue;
			animSettings.OnChangeArmMoveDuration += ChangeArmMoveDurationElements;
			animSettings.OnChangeEvalDuration += ChangeEvalDurationElements;
			animSettings.OnChangeMoveDuration += ChangeMoveDurationElements;
			animSettings.OnChangeRepetitions += ChangeRepetitionsElements;
			animSettings.OnChangeHandMoveDuration += ChangeHandMoveDurationElements;
			animSettings.OnChangeRestDuration += ChangeRestDurationElements;

			armMoveSlider.onValueChanged.AddListener(ArmDurationSliderHandler);
			handMoveSlider.onValueChanged.AddListener(HandDurationSliderHandler);
			evalDurSlider.onValueChanged.AddListener(WaitDurationSliderHandler);
			restDurSlider.onValueChanged.AddListener(RestDurationSliderHandler);
			moveDurSlider.onValueChanged.AddListener(MoveDurationSliderHandler);
			repetitionsSlider.onValueChanged.AddListener(RepetitionsSliderHandler);

			animTypeDropdown.onValueChanged.AddListener(delegate {
				AnimationTypeDropdownHandler(animTypeDropdown);
			});
		}

		private void OnDisable() {
			animSettings.OnTrainingStarted -= TrainingStarted;
			animSettings.OnTrainingStopped -= TrainingStopped;

			animSettings.OnChangeAnimTypeValue -= ChangeAnimTypeValue;
			animSettings.OnChangeArmMoveDuration -= ChangeArmMoveDurationElements;
			animSettings.OnChangeEvalDuration -= ChangeEvalDurationElements;
			animSettings.OnChangeMoveDuration -= ChangeMoveDurationElements;
			animSettings.OnChangeRepetitions -= ChangeRepetitionsElements;
			animSettings.OnChangeHandMoveDuration -= ChangeHandMoveDurationElements;
			animSettings.OnChangeRestDuration -= ChangeRestDurationElements;

			armMoveSlider.onValueChanged.RemoveListener(ArmDurationSliderHandler);
			handMoveSlider.onValueChanged.RemoveListener(HandDurationSliderHandler);
			evalDurSlider.onValueChanged.RemoveListener(WaitDurationSliderHandler);
			restDurSlider.onValueChanged.RemoveListener(RestDurationSliderHandler);
			moveDurSlider.onValueChanged.RemoveListener(MoveDurationSliderHandler);
			repetitionsSlider.onValueChanged.RemoveListener(RepetitionsSliderHandler);

			animTypeDropdown.onValueChanged.RemoveListener(delegate {
				AnimationTypeDropdownHandler(animTypeDropdown);
			});
		}

		private void TrainingStarted(TrainingType trainingType) {
			foreach (CanvasGroup cg in animationCanvases) {
				if ((trainingType == TrainingType.MANUAL && cg.Equals(manualCanvasGroup)) ||
					(trainingType == TrainingType.AUTOMATIC && cg.Equals(automaticCanvasGroup))) {
					continue;
				}
				cg.interactable = false;
				cg.blocksRaycasts = false;
			}
		}

		private void TrainingStopped() {
			foreach (CanvasGroup cg in animationCanvases) {
				cg.interactable = true;
				cg.blocksRaycasts = true;
			}
		}

		/// <summary>
		/// Method used to initialize UI elements: sliders, text and dropdown. Needed because when user connects, we have to set synced values to elements.
		/// </summary>
		private void SetAllElementsValues() {
			armMoveTextValue.text = (Mathf.Round(animSettings.ArmMoveDuration * 10) / 10).ToString("F1") + " s";
			armMoveSlider.value = (int) (animSettings.ArmMoveDuration * 2);

			handMoveTextValue.text = (Mathf.Round(animSettings.HandMoveDuration * 10) / 10).ToString("F1") + " s";
			handMoveSlider.value = (int) (animSettings.HandMoveDuration * 2);

			evalDurTextValue.text = (Mathf.Round(animSettings.EvalDuration * 10) / 10).ToString("F1") + " s";
			evalDurSlider.value = (int) (animSettings.EvalDuration * 2);

			restDurTextValue.text = (Mathf.Round(animSettings.RestDuration * 10) / 10).ToString("F1") + " s";
			restDurSlider.value = (int) (animSettings.RestDuration * 2);

			moveDurTextValue.text = (Mathf.Round(animSettings.MoveDuration * 10) / 10).ToString("F1") + " s";
			moveDurSlider.value = (int) (animSettings.MoveDuration * 2);

			repetitionsTextValue.text = animSettings.Repetitions + " x";
			repetitionsSlider.value = animSettings.Repetitions;

			List<LocalizedDropdownOption> options = new();
			foreach (AnimTypeDropdownListItem listItem in animSettings.AnimTypeOptions) {
				options.Add(listItem.localizedDropdownOption);
			}
			animTypeDropdown.GetComponent<LocalizedDropdown>().UpdateLocalizedOptions(options);
			SetAnimTypeDropdownText(animSettings.AnimType);
		}


		/*
		*
		* UI Button Handlers
		*
		*/
		public void PlayAnimationShowcaseHandler() {
			sceneObjectManager.LocalNetChar.CmdStartAnimationShowcase();
		}

		public void PlayAnimationHandler() {
			sceneObjectManager.LocalNetChar.CmdStartSingleAnimation();
		}

		private void StartTraining(TrainingType trainingType, ExerciseState startState) {
			if (sceneObjectManager.ActivePatient == null) {
				messageEvents.ShowMessage(new (Globals.LocalizedMessages.NoPatientPresent, MessageType.WARNING, true));
				return;
			}

			if (sceneObjectManager.ActivePatient.IsAnimationPlaying()) {
				messageEvents.ShowMessage(new (Globals.LocalizedMessages.AnimationAlreadyRunning, MessageType.WARNING, true));
				return;
			}

			sceneObjectManager.LocalNetChar.CmdStartTraining(trainingType, startState);
		}

		public void StartTrainingHandler() => StartTraining(TrainingType.AUTOMATIC, ExerciseState.N_A);
		public void StartRestPeriodManualHandler() => StartTraining(TrainingType.MANUAL, ExerciseState.REST);
		public void StartEvalPeriodManualHandler() => StartTraining(TrainingType.MANUAL, ExerciseState.EVAL);

		public void StopTrainingHandler() {
			sceneObjectManager.LocalNetChar.CmdStopTraining();
		}

		public void SetAnimationStartPositionHandler() {
			sceneObjectManager.LocalNetChar.CmdSetAnimationStartPosition(false, null);
		}

		public void SetAnimationMovePositionHandler() {
			sceneObjectManager.LocalNetChar.CmdAddMovePosition();
		}

		public void ClearAnimationEndPositionHandler() {
			sceneObjectManager.LocalNetChar.CmdClearMovePositions();
		}

		public void DeleteLastAnimationMovePositionHandler() {
			sceneObjectManager.LocalNetChar.CmdDeleteMovePosition();
		}

		/*
		*
		* Change UI elements Text / Value
		*
		*/
		private void ChangeArmMoveDurationElements() {
			armMoveTextValue.text = (Mathf.Round(animSettings.ArmMoveDuration * 10) / 10).ToString("F1") + " s";
			armMoveSlider.value = (int) (animSettings.ArmMoveDuration * 2);
		}

		private void ChangeHandMoveDurationElements() {
			handMoveTextValue.text = (Mathf.Round(animSettings.HandMoveDuration * 10) / 10).ToString("F1") + " s";
			handMoveSlider.value = (int) (animSettings.HandMoveDuration * 2);
		}

		private void ChangeEvalDurationElements() {
			evalDurTextValue.text = (Mathf.Round(animSettings.EvalDuration * 10) / 10).ToString("F1") + " s";
			evalDurSlider.value = (int) (animSettings.EvalDuration * 2);
		}

		private void ChangeRestDurationElements() {
			restDurTextValue.text = (Mathf.Round(animSettings.RestDuration * 10) / 10).ToString("F1") + " s";
			restDurSlider.value = (int) (animSettings.RestDuration * 2);
		}

		private void ChangeMoveDurationElements() {
			moveDurTextValue.text = (Mathf.Round(animSettings.MoveDuration * 10) / 10).ToString("F1") + " s";
			moveDurSlider.value = (int) (animSettings.MoveDuration * 2);
		}

		private void ChangeRepetitionsElements() {
			repetitionsTextValue.text = animSettings.Repetitions + " x";
			repetitionsSlider.value = animSettings.Repetitions;
		}

		private void ChangeAnimTypeValue() {
			SetAnimTypeDropdownText(animSettings.AnimType);

			if (animSettings.AnimType == AnimationType.Key) {
				registerMovePositionButton.interactable = false;
			} else {
				registerMovePositionButton.interactable = true;
			}
		}

		private void SetAnimTypeDropdownText(AnimationType animationType) {
			string textValue = animSettings.AnimTypeOptions.Find((x) => x.animationType == animationType).localizedDropdownOption.text.GetLocalizedString();
			animTypeDropdown.value = animTypeDropdown.options.FindIndex(option => option.text.Equals(textValue));
		}

		/*
		*
		* HANDLERS for UI elements
		*
		*/
		public void ArmDurationSliderHandler(float value) {
			animSettings.ArmMoveDuration = value / 2f;
		}

		public void HandDurationSliderHandler(float value) {
			animSettings.HandMoveDuration = value / 2f;
		}

		public void WaitDurationSliderHandler(float value) {
			animSettings.EvalDuration = value / 2f;
		}

		public void RestDurationSliderHandler(float value) {
			animSettings.RestDuration = value / 2f;
		}

		public void MoveDurationSliderHandler(float value) {
			animSettings.MoveDuration = value / 2f;
		}

		public void RepetitionsSliderHandler(float value) {
			animSettings.Repetitions = (int) value;
		}

		public void AnimationTypeDropdownHandler(TMP_Dropdown dropdown) {
			foreach (var item in animSettings.AnimTypeOptions) {
				if (item.localizedDropdownOption.text.GetLocalizedString().Equals(dropdown.options[dropdown.value].text)) {
					animSettings.AnimType = item.animationType;
					break;
				}
			}
		}
	}
}