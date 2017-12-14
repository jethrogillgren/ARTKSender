using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tango;

public class GhostPandaCubeGameplayObject : PandaCubeGameplayObject
{
	[HideInInspector]
	public PandaCubeGameplayObject realLivingCube;

	protected GameplayRoom myGhostingGameplayRoom; //The room we are Ghosting Into.

	public override void Start ()
	{
		myGhostingGameplayRoom = GetComponentInParent<EditorOnlyGameplayGhostTracker> ().roomToTrack;
		return;
	}
	public override void OnStartServer ()
	{
		return;
	}
	public override void OnStartClient ()
	{
		return;
	}
	public override void Update()
	{
		return;
	}
	public void LateUpdate()
	{
		return;
	}
	public override void UpdateVisibility ()
	{
		base.UpdateVisibility ();

		//Both CLient and Servers see either wireframe or full versions depending on their Ghosted gameplayRoom
		if ( base.gameplayRoom == myGhostingGameplayRoom ) //If we are the ghost for hte room the cube is Active In
			DrawFull (); // Not Overridden
		else //We we are ghosting for a room which the cube is not active in.
			DrawAsWireframe (); // Not Overridden
	}

	protected override void SetNewParent ( Transform newParent )
	{
		return;
	}

	protected override void BuildTextPopupOffset()
	{
		return;
	}

	public override void DisplayTextAtPopupPosition( Vector3 positionOffset, string text )
	{
		return;
	}

	public override void RepositionPopupText()
	{
		return;
	}
	public override void RerotatePopupText()
	{
		return;
	}
	public 	override void ClearPopupPositionText()
	{
		return;
	}
	public 	override void TrackThenDestroyTextMesh()
	{
		return;
	}
	public override void Svr_TeleportTo ( GameplayRoom dest )
	{
		return;
	}

	public override void Svr_SetMarker ( TangoSupport.Marker marker )
	{
		return;
	}
	public override void Svr_SetMarker ( ARMarker marker, int roomCameraNumber )
	{
		return;
	}
	public override void Svr_RecieveIMU ( Vector3 imuReadings )
	{
		return;
	}
	protected override void Svr_ApplyTransformations() 
	{
		return;
	}

	public override void RpcTeleportTo ( string name )
	{
		
	}
	public override void Cnt_CheckIfINeedToDisplayClickPullHint()
	{
		return;
	}

	protected override void Cnt_CheckInputTouch() 
	{
		return;
	}

}