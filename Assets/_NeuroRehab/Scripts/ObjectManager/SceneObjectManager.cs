using UnityEngine;

using NeuroRehab.Interfaces;

namespace NeuroRehab.ObjectManagers {
	public class SceneObjectManager : MonoBehaviour, ISceneObjectManager {
		[SerializeField] private Canvas[] rootCanvases;
		[SerializeField] private Transform mirrorPlane;
		[SerializeField] private IAnimationSettingsManager animSettings;
		[SerializeField] private IAnimationServerManager animServer;

		private ITableHelper table;

		private ICharacter activePatient;
		private ICharacter localChar;
		private INetworkCharacter localNetChar;

		public Transform MirrorPlane { get => mirrorPlane; set => mirrorPlane = value; }
		public IAnimationSettingsManager AnimSettings { get => animSettings; set => animSettings = value; }
		public IAnimationServerManager AnimServer { get => animServer; set => animServer = value; }

		public ICharacter ActivePatient { get => activePatient; set => activePatient = value; }
		public ICharacter LocalChar { get => localChar; set => localChar = value; }
		public INetworkCharacter LocalNetChar { get => localNetChar; set => localNetChar = value; }
		public ITableHelper Table { get => table; set => table = value; }
		public Canvas[] RootCanvases { get => rootCanvases; }
	}
}