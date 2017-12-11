using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using Tango;

//Represents one of 4 real life cube markers, which has the IMU and the same tag for all 6 sides.
//Always started during the game.
public class PandaCubeGameplayObject : BaseGameplayObject
{

	public string cubeContentName; //1, 2, .. 16

	public Material defaultMaterial;
	public Material wireframeMaterial;

	//Number of real world sensors
	protected const int sensors = 7;
	protected const int tangoOffset = 2;
	//Number of seconds to consider a sensor reading valid
	public const float dropoffDelay = 1.0f; 

	protected Vector3 m_corner3DP0;
	protected Vector3 m_corner3DP1;
	protected Vector3 m_corner3DP2;
	protected Vector3 m_corner3DP3;

	[HideInInspector]
	public GameplayRoom gameplayRoom; //The gameplayRoom that the cube is in
	[HideInInspector]
	public RoomController roomController;

	[HideInInspector]
	public LineRenderer m_rect;
	[HideInInspector]
	public Collider myCollider;


	[Space]
	////Player Specific Stuff
	/// These are only valid in one Client

	protected RoomAugPlayerController cnt_playerController;
	public RoomAugPlayerController Cnt_PlayerController
	{
		get
		{
			if (!cnt_playerController)
				cnt_playerController = FindObjectOfType<RoomAugPlayerController> ();
			return cnt_playerController;
		}
	}

	[HideInInspector]
	public bool cnt_isInSameRoom = false;

	public int cnt_timeToDelay = 2; //Time to wait before showing the hint so players don't feel hassled
	protected int cnt_delaySoFar = 0; //Time waited soo far



	[Space]
	////Server Authorative Stuff
	/// These are Server controlled only, and sync to all clients
	[SyncVar]
	[HideInInspector]
	public bool isTrackingGood = true;
	[SyncVar]
	[HideInInspector]
	public bool isTangoTrackingGood = false;
	[SyncVar]
	[HideInInspector]
	public bool isARToolkitTrackingGood = false;

	[SyncVar]
	public Util.ElementalType cubeType;



	////Server only Stuff

	//Store all the sensor readings
	Vector3[] svr_transformPositions = new Vector3[] {new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3()};
	Quaternion[] svr_transformRotations = new Quaternion[] {new Quaternion(), new Quaternion(), new Quaternion(), new Quaternion(), new Quaternion(), new Quaternion(), new Quaternion()};

	float[] svr_transformTimestamps = new float[sensors]; //The last times we heard from that sensor







	// Use this for initialization
	public override void Start ()
	{
		base.Start ();

		roomController = FindObjectOfType<RoomController> ();
		if (!roomController)
			Debug.LogError ( name + " was unable to find a RoomController" );

		myCollider = GetComponent<Collider> ();
		if (!myCollider)
			Debug.LogError (name + " did not find it's collider");

		m_rect = GetComponentInChildren<LineRenderer> ();
		if (!m_rect)
			Debug.LogError (name + " did not find it's Line Renderer");

//		playerController = FindObjectOfType<RoomAugPlayerController>();
//		if (!playerController)
//			Debug.LogError ( name + " was unable to find a PlayerController" );

		FindGameplayRoom ();

		InvokeRepeating ( "DisplayClickPullHint", 1, 1 );
	}

	//Client Only
	public void Update() {
		if (isClient)
			Cnt_CheckInputTouch ();
	}

	public void LateUpdate()  //svr only
	{
		if (!isClient)
			Svr_ApplyTransformations ();
	}

	//Servers always draw full.
	public override void OnStartServer ()
	{
		DrawFull ();
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




	//Use to draw the cube properly, locally
	public void DrawFull ()
	{
//		Debug.Log ( name + " Rendering Full " + cubeType );
		SetMaterialAndColor ( defaultMaterial, Util.GetColor ( cubeType ) );
	}

	//Use to draw the cube when it is not in the same gameplay room as you, locally
	public void DrawAsWireframe ()
	{
//		Debug.Log ( name + " Rendering Wireframe " + cubeType );
		SetMaterialAndColor ( wireframeMaterial, Util.GetColor ( cubeType ) );
	}

	protected void SetMaterialAndColor ( Material m, Color c )
	{
		foreach ( MeshRenderer mr in gameObject.GetComponentsInChildren<MeshRenderer>() )
		{
			mr.material = m;
			mr.material.color = c;
		}
	}

	//Draw a Blue rect for Tango, and a green Rect for both.
	//Rect is at Tangos last seen point
	protected void DrawRect() {

		if (isTangoTrackingGood)
			m_rect.startColor = m_rect.endColor = Color.blue;
		else if (isTangoTrackingGood && isARToolkitTrackingGood)
			m_rect.startColor = m_rect.endColor = Color.green;
		else
			return;

		m_rect.SetPosition ( 0, m_corner3DP0 );
		m_rect.SetPosition ( 1, m_corner3DP1 );
		m_rect.SetPosition ( 2, m_corner3DP2 );
		m_rect.SetPosition ( 3, m_corner3DP3 );
		m_rect.SetPosition ( 4, m_corner3DP0 );
	}

	//Register any new Gameplayroom we are in, and return it.
	//Client and Server both call this
	public GameplayRoom FindGameplayRoom ()
	{
		gameplayRoom = GetComponentInParent<GameplayRoom> ();
		if (!gameplayRoom)
			Debug.LogError ( name + " : " + cubeContentName + " has escaped outside of any Gameplayroom!  Not supported." );

		return gameplayRoom;
	}

	protected void SetNewParent ( Transform newParent )
	{
		transform.SetParent ( newParent, true );
		FindGameplayRoom ();

		UpdateAll ();//This will change visibility (which includes mesh/solid, and the color)
	}
		









	//// Server Only Functions

	[Command]
	public void CmdTest() {
		Debug.LogError ("Documentation Lied!");
	}

	//Initiate a teleport on all Clients.
	public void Svr_TeleportTo ( GameplayRoom dest )
	{
		if (isClient)
			Debug.LogError ( name + ": CLIENT INITIATED TELEPORT" );

		SetNewParent ( dest.transform );

		//Do the same for all clients
		RpcTeleportTo ( dest.name );
	}

	//Server only
	//TODO tango index
	public void Svr_SetMarker ( TangoSupport.Marker marker )
	{
		if (isClient)
		{
			Debug.LogError ( "Client recieved a Tango Marker update" );
			return;
		}

		// Apply the pose of the marker to the prefab.
		// This also applies implicitly to the axis and cube objects.
		svr_transformPositions [ tangoOffset ] = marker.m_translation;
		svr_transformRotations [ tangoOffset ] = marker.m_orientation;

		svr_transformTimestamps [ tangoOffset ] = Time.time;


		m_corner3DP0 =  marker.m_corner3DP0 ;
		m_corner3DP1 =  marker.m_corner3DP1 ;
		m_corner3DP2 =  marker.m_corner3DP2 ;
		m_corner3DP3 =  marker.m_corner3DP3 ;

		//		Debug.LogError ( "Tango Says " + transformPositions [ tangoOffset ] );
	}

	//Server only
	//The ARMarker object is tracking realtive to the ARToolkit (Room) camera
	//TODO camera index
	//TODO OnMarkerFound OnMarkerLost
	//roomCanNum is 1 based
	public void Svr_SetMarker ( ARMarker marker, int roomCameraNumber )
	{
		if (isClient)
		{
			Debug.LogError ( "Client recieved an ARToolkit Marker update" );
			return;
		}

		Matrix4x4 pose = roomController.camera1ZeroPosition.transform.localToWorldMatrix * marker.TransformationMatrix;

		svr_transformPositions [ roomCameraNumber-1 ] = ARUtilityFunctions.PositionFromMatrix ( pose );
		svr_transformRotations [ roomCameraNumber-1 ] = ARUtilityFunctions.QuaternionFromMatrix ( pose );

		svr_transformTimestamps [ roomCameraNumber-1 ] = Time.time;


		//		Debug.Log ("ARToolkit Marker:  Marker Matrix " + marker.TransformationMatrix);
		//		Debug.Log ("ARToolkit Marker:  Transform     " + transform.position + " / " + transform.rotation);
		//		Debug.LogError ( "ARToolkit Says " + transformPositions [ 0 ] );
	}

	//Server
	public void Svr_RecieveIMU ( Vector3 imuReadings )
	{
		if (isClient)
		{
			Debug.LogError ( "Client recieved a IMU update" );
			return;
		}

		//TODO
	}
		
	//Apply transformation after regular update so we have all readings in
	//TODO sensible bounds checking
	//Handled on Server, and clients get NetworkTransform'd the position
	protected void Svr_ApplyTransformations() 
	{
		isTrackingGood = false;
		isTangoTrackingGood = false;
		isARToolkitTrackingGood = false;

		//Global variable which holds the amount of rotations which 
		//need to be averaged.
		int addAmount = 0;

		//Global variables which represents the additive values
		Vector3 culminativePosition = Vector3.zero;
		Vector4 culminativeQuaternion = Vector4.zero;
		Quaternion result = new Quaternion();

		//For each sensor reading
		for ( int i = 0; i < sensors; i++ )
		{
			float t = Time.time;
			//If we had a reading within the dropoff delay time
			if (svr_transformTimestamps [ i ] > 0 && ( svr_transformTimestamps [ i ] > ( t - dropoffDelay ) ))
			{
				//				Debug.LogError ("Set  : " + transformTimestamps[i] + " " + transformPositions[i] );

				isTrackingGood = true;
				if( i < tangoOffset ) isARToolkitTrackingGood = true;
				if( i >= tangoOffset ) isTangoTrackingGood = true;

				addAmount++; //Amount of separate values so far

				//Rotation
				result = Util.AverageQuaternion (ref culminativeQuaternion, svr_transformRotations [ i ], transform.rotation, addAmount);
				//				transform.eulerAngles = ( transformRotations [ i ] );

				//Position
				culminativePosition += svr_transformPositions[i]; //We could divide by addAmount at this stage to get a valid reading without going through the whole array

			}
		}

		if (isTrackingGood)
		{
			transform.position = culminativePosition / ( float )addAmount;
			transform.rotation = result;
			DrawRect ();
		}
	}








	//// Client Only Functions

	[ClientRpc]
	public void RpcTeleportTo ( string name )
	{
		GameObject dest = GameObject.Find ( name );//TODO - inefficient
		SetNewParent ( dest.transform );
	}

	/*
	* Displays a 3D Screen space Text hint that you can ClickPull this cube.
	* There is an ordered list of preffered positions.  The first which is not blocked is chosen.
	* The text looks like a 2D space text, ie always facing the player and flat.
	*/
	public void Cnt_DisplayClickPullHint()
	{
		if (!isClient)
			return;

		//Check if the Object is good for ClickPulling
		if(isTrackingGood && !gameplayRoom.roomActive && Util.IsObjectInMainCamerasFOV ( this.transform ))
		{
			Util.JLogErr ("DisplayClickPullHint saw it", false);
			//Wait a bit if we're being pushy
			cnt_delaySoFar++;
			if (cnt_delaySoFar < cnt_timeToDelay)
				return;

			//For each Position we consider

			//Raycast to the Position
			//Select that position if Hit

			//If we didn't get anything, wait for next time as player may have moved

		} else {
			cnt_delaySoFar = 0;
		}
	}

	protected void Cnt_CheckInputTouch() 
	{
		Debug.Log ( "Touch Count: " + Input.touchCount + "   TouchPhase: " + ( Input.touchCount > 0 ? Input.GetTouch ( 0 ).phase.ToString() : "N/A") );

		if ( isTrackingGood && !gameplayRoom.roomActive && Input.touchCount > 0  &&  Input.GetTouch(0).phase == TouchPhase.Began  &&  Util.IsObjectInMainCamerasFOV ( this.transform ) )
		{
			Debug.Log (name + " touch");

			Ray raycast = Camera.main.ScreenPointToRay( Input.GetTouch(0).position );
			RaycastHit raycastHit;
			if (Physics.Raycast(raycast, out raycastHit))
			{
				Debug.Log ("Raycast Hit on " + raycastHit.collider.name);
				if (raycastHit.collider == myCollider)
				{
					Util.JLogErr( name + " Click Pull", true );
					Cnt_PlayerController.CmdClickPullCube( cubeContentName, Util.GetCurrentMainGameplayRoom().roomName );
					CmdTest ();
				}
			}
		}
	}
}
