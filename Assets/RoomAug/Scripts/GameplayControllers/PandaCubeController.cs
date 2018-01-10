using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Tango;
using UnityEngine.Networking;
using System.Text;
using System.Linq;
using System.Net.Sockets;
using System.Net;


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
	public RoomAugNetworkController networkController;

    //public HashSet<PandaCubeGameplayObject> m_PandaCubes;
    public PandaCubeGameplayObject cube1;
    public PandaCubeGameplayObject cube2;
    public PandaCubeGameplayObject cube3;
    public PandaCubeGameplayObject cube4;


	//ARToolkitAgent connectivity stuff
	public UdpClient[] clients = new UdpClient[4];

	// Use this for initialization
	void Start () {
        //m_PandaCubes = new Dictionary<String, PandaCubeGameplayObject>();

		roomController = GetComponentInParent<RoomController> ();

		networkController = GameObject.FindObjectOfType<RoomAugNetworkController> ();

        if ( !cube1 || !cube2 || !cube3 || !cube4 )
            Debug.LogError( name + ": DID NOT FIND ALL CUBE GAMEOBJECTS" );

		if(!isClient)
			Svr_StartARToolkitAgentRecieve ();

	}

	public void Svr_StartARToolkitAgentRecieve() {
		for ( int i = 0; i < clients.Length; i++ )
		{
			clients [ i ] = new UdpClient ( Util.portARToolkitAgentBase + i );
		}

		new Thread( () => Svr_ARToolkitAgentThread(0) ).Start();
		new Thread( () => Svr_ARToolkitAgentThread(1) ).Start();
		new Thread( () => Svr_ARToolkitAgentThread(2) ).Start();
		new Thread( () => Svr_ARToolkitAgentThread(3) ).Start();
	}

	public void Svr_ARToolkitAgentThread(int camID)
	{
		Thread.CurrentThread.IsBackground = true;//These threads will not prevent application termination

		UdpClient udpClient = clients [ camID ];

		/* run your code here */ 
		while (true)
		{
			try {

				IPEndPoint ep = new IPEndPoint(IPAddress.Any,0);

				byte [] byteArray = udpClient.Receive ( ref ep );

				string returnData = Encoding.ASCII.GetString(byteArray);
				//Length 71

				//Recreate the TransformationMatrix
				float [] floatArray = new float[16]; //Manual number matching hte sender
				System.Buffer.BlockCopy ( byteArray, 0, floatArray, 0, 16*4 );
				Matrix4x4 m = new Matrix4x4 ();
				m.m00 = floatArray [ 0 ];
				m.m01 = floatArray [ 1 ];
				m.m02 = floatArray [ 2 ];
				m.m03 = floatArray [ 3 ];
				m.m10 = floatArray [ 4 ];
				m.m11 = floatArray [ 5 ];
				m.m12 = floatArray [ 6 ];
				m.m13 = floatArray [ 7 ];
				m.m20 = floatArray [ 8 ];
				m.m21 = floatArray [ 9 ];
				m.m22 = floatArray [ 10 ];
				m.m23 = floatArray [ 11 ];
				m.m30 = floatArray [ 12 ];
				m.m31 = floatArray [ 13 ];
				m.m32 = floatArray [ 14 ];
				m.m33 = floatArray [ 15 ];


				//De-encode the Tag
				string tag = Encoding.ASCII.GetString ( byteArray.Skip ( 16*4 ).ToArray () );

				OnMarkerTracked ( camID, tag, m );

			} catch(Exception e) {
				Debug.LogError ( "ARToolkitAgentThread hit exception: " + e.Message ) ;
			}
		}
	}

	public void RecieveARToolkitUpdates()
	{
		
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
	//Recieve an ARToolkit marker signhting
	public void OnMarkerTracked ( int camOffset, string tag, Matrix4x4 transformationMatrix )
	{
		PandaCubeGameplayObject c = GetCubeByTag ( tag );
		if (c)
			c.Svr_SetMarker (transformationMatrix, camOffset);
		else
			Debug.LogError (name + " Could not get a cube for ARToolkit tag: " + tag);
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

	public PandaCubeGameplayObject GetCubeByTag( string arTag ) {
		string prfx = "ARTag";
		if ( arTag == prfx + cube1.cubeContentName )
			return cube1;
		else if ( arTag == prfx +cube2.cubeContentName )
			return cube2;
		else if ( arTag == prfx +cube3.cubeContentName )
			return cube3;
		else if ( arTag == prfx +cube4.cubeContentName )
			return cube4;

		return null;
	}
}
