using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RotateTest))]
class RotateEditorHelper : Editor {

	SerializedProperty offset;
	SerializedProperty Plane;
	SerializedProperty mapping;
	SerializedProperty Cube;

	void OnEnable() {
		offset = serializedObject.FindProperty("offset");
		Plane = serializedObject.FindProperty("Plane");
		mapping = serializedObject.FindProperty("mapping");
		Cube = serializedObject.FindProperty("Cube");
	}
	public override void OnInspectorGUI() {

		serializedObject.Update();
		EditorGUILayout.PropertyField(offset);
		EditorGUILayout.PropertyField(Plane);
		EditorGUILayout.PropertyField(mapping);
		EditorGUILayout.PropertyField(Cube);
		serializedObject.ApplyModifiedProperties();

		var rotScript = target as RotateTest;
		if(GUILayout.Button("Rotate")) {
			rotScript.RotateObj();
		} else if(GUILayout.Button("Mirror")) {
			rotScript.MirrorObject();
		}
	}
}
