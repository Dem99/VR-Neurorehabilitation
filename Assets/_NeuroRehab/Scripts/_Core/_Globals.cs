namespace NeuroRehab.Core {
	public static class Globals {

		/// <summary>
		/// Class holding references to localized messages used. BE CAREFUL, has to be consistent with 'messages' localization table.
		/// </summary>
		public static class LocalizedMessages
		{
			public const string NoPatientPresent = "no_patient";
			public const string TooFewPositions = "few_pos";
			public const string TooFewPositionsForKey = "key_few_pos";
			public const string MissingKey = "key_missing";
			public const string NoAnimationChosen = "no_anim_type";
			public const string AnimationCantChange = "anim_change_prohibited";
			public const string AnimationAlreadyRunning = "anim_already_running";
			public const string CantStartManual = "fail_manual";
			public const string TrainingStarted = "train_start";
			public const string TrainingStopped = "train_stop";
			public const string TrainingCancelled = "train_cancelled";
			public const string TrainingAlreadyStarted = "training_started_already";
			public const string TrainingNone = "no_training";
			public const string TrainingCantStart = "training_cant_start";
			public const string CannotGrab = "cannot_grab";
			public const string ObjectNotAboveTable = "above_table";
			public const string Rest = "rest";
			public const string Move = "move";
		}
	}
}
