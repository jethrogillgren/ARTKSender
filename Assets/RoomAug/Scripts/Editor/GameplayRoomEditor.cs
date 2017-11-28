using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameplayRoom))] 
public class GameplayRoomEditor : Editor {


	public override void OnInspectorGUI()
	{
		GameplayRoom gr = (GameplayRoom)target;
		if (gr == null)
			return;

//		gr.roomName = EditorGUILayout.TextField("Room Name", gr.roomName);

		gr.RegisterAnyParentPhysicalRoom ();
		gr.UpdateAllGameplayObjectsVisibility ();
        gr.SetAppropiateLayers();
		EditorGUILayout.LabelField("Active", gr.roomActive ? "yes" : "no");

		DrawDefaultInspector();

	}
}
