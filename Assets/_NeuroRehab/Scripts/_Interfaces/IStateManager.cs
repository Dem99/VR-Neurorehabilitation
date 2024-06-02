using System;

namespace NeuroRehab.Interfaces {
	public interface IStateManager<Type, State>
		where Type : Enum
		where State : Enum {
		public bool Start(State startState);
		public bool Stop();
		public bool ProgressState(State newState);

		public State GetCurrentState();
		public Type GetMachineType();
	}
}
