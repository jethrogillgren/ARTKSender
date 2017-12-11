using System;
using System.Collections.Generic;
using UnityEngine;
using Tango;
using UnityEngine.Networking;

//Cubes send IMU data direct to server
//Client alerts server with Marker visual positions
//Server fuses the IMU and Marker position updates, and sends back to Clients.
public class PandaCubeController : NetworkBehaviour {

    ////MARKER STUFF
    ///// <summary>
    ///// The prefabs of marker.
    ///// </summary>
    //public GameObject m_markerPrefab;

    ///// <summary>
    ///// The objects of all markers.
    ///// </summary>
    //private Dictionary<String, PandaCubeGameplayObject> m_PandaCubes;

	public RoomController roomController;

    //public HashSet<PandaCubeGameplayObject> m_PandaCubes;
    public PandaCubeGameplayObject cube1;
    public PandaCubeGameplayObject cube2;
    public PandaCubeGameplayObject cube3;
    public PandaCubeGameplayObject cube4;



	// Use this for initialization
	void Start () {
        //m_PandaCubes = new Dictionary<String, PandaCubeGameplayObject>();

		roomController = GetComponentInParent<RoomController> ();

        if ( !cube1 || !cube2 || !cube3 || !cube4 )
            Debug.LogError( name + ": DID NOT FIND ALL CUBE GAMEOBJECTS" );

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ClickPullCube( string cubeContentName, string targetRoomName ) {
		PandaCubeGameplayObject c = GetCube ( cubeContentName );
		GameplayRoom r = roomController.GetGameplayRoomByName ( targetRoomName );
		if (c && r)
			c.Svr_TeleportTo ( r );
	}

    //Recieve a Tango marker sighting from a Client Player
    public void RecieveMarker( TangoSupport.Marker marker ) {

        //Set the cubes position, overwriting and IMU values
        //TODO if the cube is being rotated, the 6identical markers will make it jump upright again...
		PandaCubeGameplayObject c = GetCube ( marker );
		if (c)
			c.Svr_SetMarker ( marker );
		else
			Debug.LogError (name + " Could not get a cube for Tango marker: " + marker);
    }

	//Recieve an ARToolkit marker signhting
	public void OnMarkerTracked ( ARMarker marker )
	{
		PandaCubeGameplayObject c = GetCube ( marker );
		if (c)
			c.Svr_SetMarker (marker, 1);
		else
			Debug.LogError (name + " Could not get a cube for ARToolkit marker: " + marker);
	}

	private PandaCubeGameplayObject GetCube( ARMarker marker ) {
		if ( marker.Tag.Contains( cube1.cubeContentName ) )
			return cube1;
		else if ( marker.Tag.Contains( cube2.cubeContentName ) )
			return cube2;
		else if ( marker.Tag.Contains( cube3.cubeContentName ) )
			return cube3;
		else if ( marker.Tag.Contains( cube4.cubeContentName ) )
			return cube4;

		return null;
	}

	public PandaCubeGameplayObject GetCube( TangoSupport.Marker marker ) {
		return ( GetCube ( marker.m_content ) );

	}

	public PandaCubeGameplayObject GetCube( string cubeContentName ) {
		if ( cubeContentName == cube1.cubeContentName )
			return cube1;
		else if ( cubeContentName == cube2.cubeContentName )
			return cube2;
		else if ( cubeContentName == cube3.cubeContentName )
			return cube3;
		else if ( cubeContentName == cube4.cubeContentName )
			return cube4;

		return null;
	}
}
