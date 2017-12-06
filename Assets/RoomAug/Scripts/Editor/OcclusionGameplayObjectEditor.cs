using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OcclusionGameplayObject))] 
public class OcclusionGameplayObjectEditor : Editor {

	public override void OnInspectorGUI()
	{
		OcclusionGameplayObject o = (OcclusionGameplayObject)target;
		if (o == null)
			return;

		EditorGUILayout.HelpBox("CUBE DID NOT FIND FLOOR", MessageType.Error);

		//These can live outside of GameplayRooms, so trigger Updates.
		o.UpdateVisibility ();

		DrawDefaultInspector();
	}

}