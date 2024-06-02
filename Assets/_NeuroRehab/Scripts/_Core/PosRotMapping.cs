using System;
using UnityEngine;

namespace NeuroRehab.Core {

	/// <summary>
	/// Custom class used to transfer two vectors together. NOT nullable when sent over network, due to CustomReadWriteFunctions restrictions. If it is needed then Serialization functions need to be changed. Can be null when using only locally.
	/// </summary>
	[Serializable]
	public class PosRotMapping {
		[SerializeField] private Vector3 position;
		[SerializeField] private Vector3 rotation;

		public Vector3 Position { get => position; set => position = value; }
		public Vector3 Rotation { get => rotation; set => rotation = value; }

		public PosRotMapping(Vector3 _position, Vector3 _rotation) {
			Position = _position;
			Rotation = _rotation;
		}

		public PosRotMapping(Vector3 _position, Quaternion _rotation) {
			Position = _position;
			Rotation = _rotation.eulerAngles;
		}

		public PosRotMapping(Transform _object) {
			Position = _object.position;
			Rotation = _object.rotation.eulerAngles;
		}

		public override string ToString() {
			return Position + " _ " + Rotation;
		}

		public void Rotate(float x, float y, float z) {
			rotation += new Vector3(x, y, z);
		}

		public PosRotMapping Clone() {
			return new PosRotMapping(Position, Rotation);
		}

		/// <summary>
		/// In-place mirrors mapping using plane object
		/// </summary>
		/// <param name="mirrorPlane"></param>
		/// <returns></returns>
		public PosRotMapping MirrorMapping(Plane mirrorPlane) {
			PosRotMapping newMirroredMapping = Clone();
			Vector3 closestPoint;
			float distanceToMirror;
			Vector3 mirrorPos;

			closestPoint = mirrorPlane.ClosestPointOnPlane(newMirroredMapping.Position);
			distanceToMirror = mirrorPlane.GetDistanceToPoint(newMirroredMapping.Position);

			mirrorPos = closestPoint - mirrorPlane.normal * distanceToMirror;

			newMirroredMapping.Position = mirrorPos;
			newMirroredMapping.Rotation = ReflectRotation(Quaternion.Euler(Rotation), mirrorPlane.normal).eulerAngles;

			Position = newMirroredMapping.Position;
			Rotation = newMirroredMapping.Rotation;

			return newMirroredMapping;
		}


		/// <summary>
		/// Mirrors mapping using plane object
		/// </summary>
		/// <param name="mirrorPlane"></param>
		/// <returns></returns>
		public static PosRotMapping MirrorMapping(Plane mirrorPlane, PosRotMapping mapping) {
			PosRotMapping newMirroredMapping = mapping.Clone();

			Vector3 closestPoint;
			float distanceToMirror;
			Vector3 mirrorPos;

			closestPoint = mirrorPlane.ClosestPointOnPlane(newMirroredMapping.Position);
			distanceToMirror = mirrorPlane.GetDistanceToPoint(newMirroredMapping.Position);

			mirrorPos = closestPoint - mirrorPlane.normal * distanceToMirror;

			newMirroredMapping.Position = mirrorPos;
			newMirroredMapping.Rotation = ReflectRotation(Quaternion.Euler(mapping.Rotation), mirrorPlane.normal).eulerAngles;

			return newMirroredMapping;
		}

		private static Quaternion ReflectRotation(Quaternion source, Vector3 normal) {
			return Quaternion.LookRotation(Vector3.Reflect(source * Vector3.forward, normal), Vector3.Reflect(source * Vector3.up, normal));
		}
	}
}