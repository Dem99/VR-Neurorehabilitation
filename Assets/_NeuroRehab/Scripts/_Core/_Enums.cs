// namespace with all enums used
namespace NeuroRehab.Core {

	public enum LogLevel {NONE = 0, ERROR, WARNING, ALL};

	/// <summary>
	/// Role enum, can be extended for more Roles.
	/// </summary>
	public enum UserRole {Therapist = 0, Patient = 1};

	/// <summary>
	/// Type of HMD discovered by code.
	/// </summary>
	public enum HMDType {Mock, Other, NotFound, Android};

	/// <summary>
	/// Used when changing controller model in XR.
	/// </summary>
	public enum ControllerType {DaydreamVR = 0, HPG2Reverb = 1, HTCVive = 2, Quest2 = 3, Touch = 4, ValveIndex = 5, ViveFocusPlus = 6};

	/// <summary>
	/// Patient Arm animation types.
	/// </summary>
	public enum AnimationType {Off = -1, Block = 0, Cube = 1, Cup = 2, Key = 3};

	/// <summary>
	/// Animation States.
	/// </summary>
	public enum ArmAnimationState {Playing, Stopped};

	/// <summary>
	/// Message Types used for differentiating what color background to use.
	/// </summary>
	public enum MessageType {SUCCESS, WARNING, NORMAL};

	public enum ReticleStyle {FILLED, EMPTY};

	public enum CursorStyle {SIMPLE, WHITE_GREEN, WHITE_ORANGE, BLACK_RED};

	public enum LanguageEnum {EN = 0, SK = 1, CZ = 2};

	public enum TrainingType {OFF, MANUAL, AUTOMATIC};

	// N_A = Not Applicable (end of training, after move, before rest period starts etc)
	public enum ExerciseState {N_A = 0, REST, EVAL, MOVE, CANCELLED};

	public enum CountdownMessageOp {NONE, COUNTDOWN, MESSAGE_ONLY};

	public enum AudioClipType {UI_INTERACTION};

	public enum UsedXRLoader {OPEN_XR, MOCK};
}
