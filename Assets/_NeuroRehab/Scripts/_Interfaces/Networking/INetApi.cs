namespace NeuroRehab.Interfaces {
	public interface INetApi {
		public void StartRestPeriod(string roomID);
		public void StopRestPeriod(string roomID);

		public void StartEegEvaluation(string roomID);
		public void StopEegEvaluation(string roomID);
	}
}