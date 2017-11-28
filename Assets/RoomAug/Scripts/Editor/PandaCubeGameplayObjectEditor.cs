using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PandaCubeGameplayObject))] 
public class PandaCubeGameplayObjectEditor : Editor {


	public override void OnInspectorGUI()
	{
        PandaCubeGameplayObject pc = (PandaCubeGameplayObject)target;
        if (pc == null)
			return;

        pc.FindGameplayRoom ();
		EditorGUILayout.LabelField("Gameplay Room:", pc.gameplayRoom ? pc.gameplayRoom.roomName : "None");



        DrawDefaultInspector();
	}
}
