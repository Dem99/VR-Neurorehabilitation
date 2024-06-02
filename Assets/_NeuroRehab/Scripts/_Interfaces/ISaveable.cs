using NeuroRehab.Core;

namespace NeuroRehab.Interfaces {
	public interface ISaveable {
		void PopulateSaveData(SaveData saveData);
		void LoadFromSaveData(SaveData saveData);
	}
}
