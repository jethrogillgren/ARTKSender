using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameplayController))] 
public class GameplayControllerEditor : Editor {

	public override void OnInspectorGUI()
	{
		GameplayController gc = (GameplayController)target;
		if (gc == null)
			return;

		gc.collectGameplayObjects ();
		EditorGUILayout.LabelField("Gameplay Objects", gc.m_gameplayObjects == null ? "0" : gc.m_gameplayObjects.Count.ToString() );

		gc.collectPhysicalRooms ();
		EditorGUILayout.LabelField("Physical Rooms", gc.m_physicalRooms == null ? "0" : gc.m_physicalRooms.Count.ToString() );
	}
}
