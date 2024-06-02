using UnityEngine;

namespace NeuroRehab.Interfaces {
	public interface ISceneObjectManager {
		public Transform MirrorPlane { get; set; }
		public IAnimationSettingsManager AnimSettings { get; set; }
		public IAnimationServerManager AnimServer { get; set; }

		public ICharacter ActivePatient { get; set; }
		public ICharacter LocalChar { get; set; }
		public INetworkCharacter LocalNetChar { get; set; }
		public ITableHelper Table { get; set; }
		public Canvas[] RootCanvases { get; }
	}
}