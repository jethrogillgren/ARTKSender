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

		EditorGUI.BeginChangeCheck ();

		DrawDefaultInspector();

		if (EditorGUI.EndChangeCheck ()) {
			// Code to execute if GUI.changed
			// was set to true inside the block of code above.
			//These can live outside of GameplayRooms, so trigger Updates.
			Debug.LogError ("CHANGE");
			o.UpdateVisibility ();
		}
	}

}