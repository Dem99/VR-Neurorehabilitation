using System;

namespace NeuroRehab.Core {

	[Serializable]
	public class Arg{
		private readonly string name;
		private readonly string valStr;
		private readonly int valInt;
		private readonly float valFloat;
		private readonly bool initStr;
		private readonly bool initInt;
		private readonly bool initFloat;

		public Arg() {}

		public Arg(string _name, string _valStr) {
			name = _name;
			valStr = _valStr;
			initStr = true;
			initInt = false;
			initFloat = false;
		}

		public Arg(string _name, int _valInt) {
			name = _name;
			valInt = _valInt;
			initStr = false;
			initInt = true;
			initFloat = false;
		}

		public Arg(string _name, float _valFloat) {
			name = _name;
			valFloat = _valFloat;
			initStr = false;
			initInt = false;
			initFloat = true;
		}

		public Arg(string _name, string _valStr, int _valInt, float _valFloat, bool _initStr, bool _initInt, bool _initFloat) {

			name = _name;

			valStr = _valStr;
			valInt = _valInt;
			valFloat = _valFloat;

			initStr = _initStr;
			initInt = _initInt;
			initFloat = _initFloat;
		}

		public bool TryGetReadableNameValuePair(out (string name, string value) nameValuePair) {
			nameValuePair = (name, null);
			if (initStr) {
				nameValuePair.value = valStr;
			} else if (initInt) {
				nameValuePair.value = "" + valInt;
			} else if (initFloat) {
				nameValuePair.value = "" + valFloat;
			}

			return nameValuePair.value != null;
		}

		public int GetIntValue() {
			return valInt;
		}

		public float GetFloatValue() {
			return valFloat;
		}

		public string GetStringValue() {
			return valStr;
		}

		public string GetName() {
			return name;
		}

		public (string, string, int, float, bool, bool, bool) QueryValues() {
			return (name, valStr, valInt, valFloat, initStr, initInt, initFloat);
		}
	}
}