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

		gc.collectTeleportTriggers();
		EditorGUILayout.LabelField("Teleport Tiggers", gc.m_teleportTriggers == null ? "0" : gc.m_teleportTriggers.Count.ToString() );

//		if( gc.m_gameplayObjects.Count == 0  ||  gc.m_physicalRooms.Count == 0  || gc.m_teleportTriggers.Count == 0  ) {		
//			EditorGUILayout.HelpBox("SCENE IS MISSING REQUIRED ELEMENTS", MessageType.Error);
//		}

		EditorGUILayout.Separator();

//
//		gc.m_currentPhysicalRoom = (PhysicalRoom)EditorGUILayout.ObjectField("Start Physical Room:", gc.m_currentPhysicalRoom, typeof(PhysicalRoom), true);
//		gc.m_startGameplayRoom = (GameplayRoom)  EditorGUILayout.ObjectField("Start Gameplay Room:", gc.m_startGameplayRoom, typeof(GameplayRoom), true);

		// Show default inspector property editor
		DrawDefaultInspector ();
	}
}
