using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SlidingFloorGameplayObject))] 
public class SlidingFloorGameplayObjectEditor : Editor {

	public override void OnInspectorGUI()
	{
		SlidingFloorGameplayObject sf = (SlidingFloorGameplayObject)target;
		if (sf == null)
			return;
		

		sf.collectAdjoiningFloors();
		EditorGUILayout.LabelField ( "North: ", sf.north == null ? "NO" : sf.north.name );
		EditorGUILayout.LabelField ( "South: ", sf.south == null ? "NO" : sf.south.name  );
		EditorGUILayout.LabelField ( "West: ", sf.west == null ? "NO" : sf.west.name  );
		EditorGUILayout.LabelField ( "East: ", sf.east == null ? "NO" : sf.east.name  );

		if( sf.north == null  && sf.south == null  && sf.east == null && sf.west == null )
			EditorGUILayout.HelpBox("FLOOR DID NOT FIND CONJOINING FLOOR", MessageType.Error);
		


		EditorGUILayout.Separator();


		sf.collectSlidingCube ();
		EditorGUILayout.LabelField ( "Cube: ", sf.cube == null ? "NO" : sf.cube.name );

		sf.blockingObject = (GameObject)EditorGUILayout.ObjectField("Blocker:", sf.blockingObject, typeof(GameObject), true);

//		sf.blockingObject = (GameObject) EditorGUILayout.ObjectField("Blocking Object:", sf.blockingObject, typeof(GameObject), true);

	}

}