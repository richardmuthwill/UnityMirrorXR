using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NetworkXRGrab))]
public class NetworkXRGrabEditor : Editor
{
	override public void OnInspectorGUI()
	{
		var networkXRGrab = target as NetworkXRGrab;

		EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

		networkXRGrab.grabbableLayerName = EditorGUILayout.TextField("Grabbable layer name:", networkXRGrab.grabbableLayerName);
		networkXRGrab.grabbableOnStart = EditorGUILayout.Toggle("Grabbable on start", networkXRGrab.grabbableOnStart);

		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
		EditorGUILayout.LabelField("Public Variables", EditorStyles.boldLabel);

		networkXRGrab.grabbable = EditorGUILayout.Toggle("Currently grabbable", networkXRGrab.grabbable);

	}
}
