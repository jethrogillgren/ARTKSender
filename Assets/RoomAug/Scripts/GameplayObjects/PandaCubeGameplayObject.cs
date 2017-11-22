using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using Tango;

//Represents one of 4 real life cube markers, which has the IMU and the same tag for all 6 sides.
//Always started during the game.
public class PandaCubeGameplayObject : BaseGameplayObject
{

	public string cubeContentName;
	//1, 2, .. 16

	[HideInInspector]
	[SyncVar ( hook = "OnGameplayRoomSync" )]
	public GameplayRoom gameplayRoom;

	public LineRenderer m_rect;

	public bool isTrackingGood = true;

	//A cube is always active in only one room.  It will appear translucent in others and not interact.
	//public GameplayRoom room;

	// Use this for initialization
	public override void Start ()
	{
		base.Start ();

		FindGameplayRoom ();
	}

	//Servers always draw full.
	public override void OnStartServer ()
	{
		DrawFull ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		//
	}

	public void OnGameplayRoomSync ( GameplayRoom gr )
	{
		Util.JLog ( "SYNCVAR OnGameplayRoomSync: " + gr );
		gameplayRoom = gr;
		UpdateAll ();
	}

	//Server and Client
	public override void UpdateAll ()
	{
		base.UpdateAll ();

		Util.JLog ( name + " is Updating All" );

		UpdateVisibility ();
	}

	//Cubes are special, they are always active across rooms/clients.
	//This overrides the usual UpdateVisibility() which does client roomBased enabling
	public override void UpdateVisibility ()
	{
		gameObject.SetActive ( true );

		//Clients see either wireframe or full versions depending on their gameplayRoom
		if (isClient)
		{
			if (gameplayRoom.roomActive)
				DrawFull ();
			else
				DrawAsWireframe ();
		}

		//Servers rely on the layer being updated (automatic) so their CullingMask shows the cube in only the right room.
			
	}

	public void DrawFull ()
	{
		foreach (MeshRenderer m in gameObject.GetComponentsInChildren<MeshRenderer>())
		{
			m.material.color = Color.white;
		}
	}

	public void DrawAsWireframe ()
	{
		foreach (MeshRenderer m in gameObject.GetComponentsInChildren<MeshRenderer>())
		{
			m.material.color = Color.Lerp(Color.white, Color.clear, 0.5f);
		}
	}


	//Register any new Gameplayroom we are in, and return it.
	public GameplayRoom FindGameplayRoom ()
	{
        
		gameplayRoom = GetComponentInParent<GameplayRoom> ();
		if (!gameplayRoom)
			Util.JLogErr ( name + " : " + cubeContentName + " has escaped outside of any Gameplayroom!  Not supported." );

		return gameplayRoom;
	}


	//SERVER calls this.
	//TODO this looks wrong... how do clients get updated?
	public void TeleportTo ( GameplayRoom dest )
	{
		if (!isClient && dest)
		{
			transform.SetParent ( dest.transform, true );
			FindGameplayRoom ();
			UpdateAll ();//This will change visibility
			Util.JLog ( name + " teleported into " + gameplayRoom.roomName );
		}
	}

	//Server
	public void SetMarker ( TangoSupport.Marker marker )
	{
		m_rect.SetPosition ( 0, marker.m_corner3DP0 );
		m_rect.SetPosition ( 1, marker.m_corner3DP1 );
		m_rect.SetPosition ( 2, marker.m_corner3DP2 );
		m_rect.SetPosition ( 3, marker.m_corner3DP3 );
		m_rect.SetPosition ( 4, marker.m_corner3DP0 );

		// Apply the pose of the marker to the prefab.
		// This also applies implicitly to the axis and cube objects.
		transform.position = marker.m_translation;
		transform.rotation = marker.m_orientation;
	}

	//Server
	public void RecieveIMU ( Vector3 imuReadings )
	{
		//Set the cubes position
	}
}
