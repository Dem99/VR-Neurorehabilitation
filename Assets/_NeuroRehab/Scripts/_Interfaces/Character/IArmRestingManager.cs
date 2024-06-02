namespace NeuroRehab.Interfaces {
	public interface IArmRestingManager {
		public bool IsArmResting{get; set;}
		public void ChangeAnimatedArm(bool _old, bool _new);
	}
}