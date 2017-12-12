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
		if (gr.cnt_PhysicalRoom)
			gr.transform.localPosition = Vector3.zero;
//			gr.GetComponentInParent<RoomController>().ActivateInMainRoom(gr);
		else
			gr.transform.localPosition = gr.offsetLocalPosition;

		gr.UpdateAllGameplayObjectsVisibility ();
        gr.SetAppropiateLayers();
		EditorGUILayout.LabelField("Active", gr.cnt_roomActive ? "yes" : "no");

		DrawDefaultInspector();
	}
}
