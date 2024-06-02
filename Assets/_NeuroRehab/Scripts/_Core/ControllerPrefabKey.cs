using System;

namespace NeuroRehab.Core {
	[Serializable]
	public class ControllerPrefabKey {
		public ControllerType controllerType;
		public bool isLeftHand;

		public ControllerPrefabKey(ControllerType _controllerType, bool _isLeft) {
			controllerType = _controllerType;
			isLeftHand = _isLeft;
		}
		public override int GetHashCode() {
			return controllerType.GetHashCode() + isLeftHand.GetHashCode();
		}

		public override bool Equals(object obj) {
			if (obj == null) return false;

			ControllerPrefabKey tmpObj = (ControllerPrefabKey) obj;
			if (tmpObj == null) return false;

			return tmpObj.controllerType == controllerType && tmpObj.isLeftHand == isLeftHand;
		}
	}

}