/*
 *  ARTrackedGameplayObjectEditor.cs
 *  artgoolKit for Unity
 *
 *  This file is part of artgoolKit for Unity.
 *
 *  artgoolKit for Unity is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  artgoolKit for Unity is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with artgoolKit for Unity.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  As a special exception, the copyright holders of this library give you
 *  permission to link this library with independent modules to produce an
 *  executable, regardless of the license terms of these independent modules, and to
 *  copy and distribute the resulting executable under terms of your choice,
 *  provided that you also meet, for each linked independent module, the terms and
 *  conditions of the license of that module. An independent module is a module
 *  which is neither derived from nor based on this library. If you modify this
 *  library, you may extend this exception to your version of the library, but you
 *  are not obligated to do so. If you do not wish to do so, delete this exception
 *  statement from your version.
 *
 *  Copyright 2015 Daqri, LLC.
 *  Copyright 2010-2015 ARToolkit, Inc.
 *
 *  Author(s): Philip Lamb, Julian Looser
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ARTrackedGameplayObject))] 
public class ARTrackedGameplayObjectEditor : Editor 
{
    public override void OnInspectorGUI()
    {
	ARTrackedGameplayObject artgo = (ARTrackedGameplayObject)target;
		if (artgo == null) return;


		artgo.m_IsDecorationOnly = EditorGUILayout.Toggle("Decoration Only", artgo.m_IsDecorationOnly);
		artgo.m_GameplayState = (ARTrackedGameplayObject.GameplayState) EditorGUILayout.EnumPopup ("Gameplay State", artgo.m_GameplayState);

		EditorGUILayout.Separator();


		artgo.MarkerTag = EditorGUILayout.TextField("Marker tag", artgo.MarkerTag);

		ARMarker marker = artgo.GetMarker();
		EditorGUILayout.LabelField("Got marker", marker == null ? "no" : "yes");
		if (marker != null) {
			string type = ARMarker.MarkerTypeNames[marker.MarkerType];
			EditorGUILayout.LabelField("Marker UID", (marker.UID != ARMarker.NO_ID ? marker.UID.ToString() : "Not loaded") + " (" + type + ")");	
		}
		
		EditorGUILayout.Separator();
		
		artgo.secondsToRemainVisible = EditorGUILayout.FloatField("Stay visible", artgo.secondsToRemainVisible);
		
		EditorGUILayout.Separator();
		
		artgo.eventReceiver = (GameObject)EditorGUILayout.ObjectField("Event Receiver:", artgo.eventReceiver, typeof(GameObject), true);
	}
}
