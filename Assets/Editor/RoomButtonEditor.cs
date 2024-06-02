using UnityEditor;
using UnityEditor.UI;
using TMPro;

using NeuroRehab.UI;

[CustomEditor(typeof(RoomButton))]
public class RoomButtonEditor : ButtonEditor {
	public override void OnInspectorGUI() {
		RoomButton component = (RoomButton)target;

		base.OnInspectorGUI();

		component.RoomNameField = (TMP_Text)EditorGUILayout.ObjectField("Room Name Field", component.RoomNameField, typeof(TMP_Text), true);
		component.UserCountField = (TMP_Text)EditorGUILayout.ObjectField("User Count Field", component.UserCountField, typeof(TMP_Text), true);
		component.RoomIDField = (TMP_Text)EditorGUILayout.ObjectField("Room ID Field", component.RoomIDField, typeof(TMP_Text), true);
	}
}