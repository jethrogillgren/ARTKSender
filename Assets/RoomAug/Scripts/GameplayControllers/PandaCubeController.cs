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

    //public HashSet<PandaCubeGameplayObject> m_PandaCubes;
    public PandaCubeGameplayObject cube1;
    public PandaCubeGameplayObject cube2;
    public PandaCubeGameplayObject cube3;
    public PandaCubeGameplayObject cube4;




	// Use this for initialization
	void Start () {
        //m_PandaCubes = new Dictionary<String, PandaCubeGameplayObject>();

        if ( !cube1 || !cube2 || !cube3 || !cube4 )
            Util.JLogErr( name + ": DID NOT FIND ALL CUBE GAMEOBJECTS" );

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //Recieve a marker sighting from a Client Player
    public void RecieveMarker( TangoSupport.Marker marker ) {

        //TODO sensible bounds checking (did it just teleport across the map in the last ms?)

        //Set the cubes position, overwriting and IMU values
        //TODO if the cube is being rotated, the 6identical markers will make it jump upright again...
        GetCube( marker ).SetMarker(marker);
    }

	//Recieve IMU from Cubes
	public void RecieveCube() {
		//TODO

		//TODO sensible bounds checking (did it just teleport across the map in the last ms?)
	}



    private PandaCubeGameplayObject GetCube( TangoSupport.Marker marker ) {
        
        if ( marker.m_content == cube1.cubeContentName )
            return cube1;
        else if ( marker.m_content == cube2.cubeContentName )
            return cube2;
        else if ( marker.m_content == cube3.cubeContentName )
            return cube3;
        else if ( marker.m_content == cube4.cubeContentName )
            return cube4;
        
        return null;
    }

}
