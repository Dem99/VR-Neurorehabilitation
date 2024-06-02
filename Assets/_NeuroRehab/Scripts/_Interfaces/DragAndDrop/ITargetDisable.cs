namespace NeuroRehab.Interfaces {
	public interface ITargetDisable {
		public bool ShowRange { get; }
		public void CmdDisableDrag();
		public void RpcDisableDrag();
		public void CmdEnableDrag();
		public void RpcEnableDrag();
	}
}