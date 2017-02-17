using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PhysicalRoom))] 
public class PhysicalRoomEditor : Editor {

	public override void OnInspectorGUI()
	{
		PhysicalRoom pr = (PhysicalRoom)target;
		if (pr == null)
			return;

		pr.roomName = EditorGUILayout.TextField("Room Name", pr.roomName);

		pr.registerAnyChildGameplayRoom ();
		EditorGUILayout.LabelField("Got GameplayRoom", pr.gameplayRoom == null ? "no" : "yes");

	}
}
