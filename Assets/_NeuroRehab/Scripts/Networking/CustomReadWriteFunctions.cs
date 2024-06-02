using Mirror;
using UnityEngine;

using NeuroRehab.Utility;
using NeuroRehab.Core;

namespace NeuroRehab.Networking
{
	/// <summary>
	/// Custom Serialization methods for more complex objects, needed because we use it in SyncList <br/>
	/// Refer to https://mirror-networking.gitbook.io/docs/guides/serialization for more details.
	/// </summary>
	public static class CustomReadWriteFunctions {

		/*
		*
		* PosRotMapping
		*
		*/

		public static void WritePosRotMapping(this NetworkWriter writer, PosRotMapping value) {
			if (value == null) {
				writer.WriteVector3Nullable(null);
				return;
			}
			writer.WriteVector3Nullable(value.Position);
			writer.WriteVector3Nullable(value.Rotation);
		}

		public static PosRotMapping ReadPosRotMapping(this NetworkReader reader) {
			Vector3? pos = reader.ReadVector3Nullable();
			if (pos == null) {
				return null;
			}
			Vector3 rot = reader.ReadVector3Nullable() ?? new Vector3();
			return new PosRotMapping(pos ?? new Vector3(), rot);
		}

		/*
		*
		* Arg
		*
		*/

		public static void WriteArg(this NetworkWriter writer, Arg value) {
			if (value == null) {
				writer.WriteString(null);
				return;
			}
			(string name, string valStr, int valInt, float valFloat, bool initStr, bool initInt, bool initFloat) = value.QueryValues();

			writer.WriteString(name);

			writer.WriteString(valStr);
			writer.WriteInt(valInt);
			writer.WriteFloat(valFloat);

			writer.WriteBool(initStr);
			writer.WriteBool(initInt);
			writer.WriteBool(initFloat);
		}

		public static Arg ReadArg(this NetworkReader reader) {
			string name = reader.ReadString();
			if (name == null) {
				return null;
			}

			// we don't read 'name' anymore, it's already been read earlier
			(string valStr, int valInt, float valFloat, bool initStr, bool initInt, bool initFloat) = (
				reader.ReadString(), reader.ReadInt(), reader.ReadFloat(),

				reader.ReadBool(), reader.ReadBool(), reader.ReadBool()
			);
			return new Arg(name, valStr, valInt, valFloat, initStr, initInt, initFloat);
		}
	}
}