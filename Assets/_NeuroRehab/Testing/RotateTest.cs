using UnityEngine;

using NeuroRehab.Core;

public class RotateTest : MonoBehaviour
{
	public Vector3 offset;
	public Transform Plane;

	public Transform Cube;

	public PosRotMapping mapping;

	public void RotateObj() {
		var plane = new Plane(Plane.up, Plane.position);

		transform.RotateAround(transform.position, transform.right, 180);
	}

	public void MirrorObject() {
		var plane = new Plane(-Plane.right, Plane.position);
		var rotPlane = new Plane(Plane.forward, Plane.position);

		var mirrorPoint = plane.ClosestPointOnPlane(transform.position);

		var distance = transform.position - mirrorPoint;
		transform.position = mirrorPoint - distance;

		var quaternion = Quaternion.LookRotation(Vector3.Reflect(transform.rotation * Vector3.forward, rotPlane.normal), Vector3.Reflect(transform.rotation * Vector3.up, rotPlane.normal));

		/*Debug.Log(plane.normal);
		Vector3 forw = transform.forward;
		Vector3 mirrored = Vector3.Reflect(forw, rotPlane.normal);
		transform.rotation = Quaternion.LookRotation(mirrored, transform.up);*/

		transform.rotation = quaternion;
		mapping.Position = transform.position;
		mapping.Rotation = transform.rotation.eulerAngles;
		/*
		PosRotMapping newMirroredObject = mapping.Clone();

		Vector3 closestPoint;
		float distanceToMirror;
		Vector3 mirrorPos;

		closestPoint = mirrorPlane.ClosestPointOnPlane(mapping.position);
		distanceToMirror = mirrorPlane.GetDistanceToPoint(position);

		mirrorPos = closestPoint - mirrorPlane.normal * distanceToMirror;

		newMirroredObject.position = mirrorPos;
		newMirroredObject.rotation = ReflectRotation(Quaternion.Euler(rotation), mirrorPlane.normal).eulerAngles;

		return newMirroredObject;*/
	}
}
