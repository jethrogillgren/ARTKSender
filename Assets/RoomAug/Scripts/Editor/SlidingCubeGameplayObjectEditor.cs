using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SlidingCubeGameplayObject))] 
public class SlidingCubeGameplayObjectEditor : Editor {

	public override void OnInspectorGUI()
	{
		SlidingCubeGameplayObject c = (SlidingCubeGameplayObject)target;
		if (c == null)
			return;

		c.collectSlidingFloor ();
		if( c.floor == null )
			EditorGUILayout.HelpBox("CUBE DID NOT FIND FLOOR", MessageType.Error);
		else 
			EditorGUILayout.LabelField ( "Floor: ", c.floor == null ? "NO" : c.floor.name );

	}

}