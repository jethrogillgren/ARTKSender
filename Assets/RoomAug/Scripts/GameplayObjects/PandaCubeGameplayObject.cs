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
	public GameplayRoom gameplayRoom;

	private RoomController roomController;

	public LineRenderer m_rect;

	public Material defaultMaterial;
	public Material wireframeMaterial;

	public bool isTrackingGood = true;
	public const float dropoffDelay = 1.0f;
	//Number of seconds to consider a sensor reading valid

	public Util.ElementalType cubeType;

	//Store all the sensor readings
	private const int sensors = 7;
	private const int tangoOffset = 2;
	//Pos in transformReadings that tango readings start
	Vector3[] transformPositions = new Vector3[] {new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3()};
	Quaternion[] transformRotations = new Quaternion[] {new Quaternion(), new Quaternion(), new Quaternion(), new Quaternion(), new Quaternion(), new Quaternion(), new Quaternion()};

	float[] transformTimestamps = new float[sensors];
	//The last times we heard from that sensor


	//	Transform[] arToolkitTransforms = new Transform[2]; //Each Room Camera
	//	Transform[] tangoTransforms = new Transform[5]; //Each potential Tango player
	//TODO entry for each tango and each


	//A cube is always active in only one room.  It will appear translucent in others and not interact.
	//public GameplayRoom room;

	// Use this for initialization
	public override void Start ()
	{
		base.Start ();

		roomController = FindObjectOfType<RoomController> ();
		if (!roomController)
			Debug.LogError ( name + " was unable to find a RoomController" );
		
		FindGameplayRoom ();
	}

	//Servers always draw full.
	public override void OnStartServer ()
	{
		DrawFull ();
	}

	//	//Update my state, for when rooms change
	//	public override void UpdateAll()
	//	{
	//		UpdateVisibility();
	//		SetLayer();
	//	}

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
		Debug.Log ( name + " Rendering Full " + cubeType );
		SetMaterialAndColor ( defaultMaterial, Util.GetColor ( cubeType ) );

	}
	//Use to draw the cube when it is not in the same gameplay room as you, locally
	public void DrawAsWireframe ()
	{
		Debug.Log ( name + " Rendering Wireframe " + cubeType );
		SetMaterialAndColor ( wireframeMaterial, Util.GetColor ( cubeType ) );
	}

	private void SetMaterialAndColor ( Material m, Color c )
	{
		foreach ( MeshRenderer mr in gameObject.GetComponentsInChildren<MeshRenderer>() )
		{
			mr.material = m;
			mr.material.color = c;
		}
	}
		



	//Register any new Gameplayroom we are in, and return it.
	//Client and Server both call this
	public GameplayRoom FindGameplayRoom ()
	{
		//Servers set it and SYNCVAR gives it to the clients
		gameplayRoom = GetComponentInParent<GameplayRoom> ();
		if (!gameplayRoom)
			Debug.LogError ( name + " : " + cubeContentName + " has escaped outside of any Gameplayroom!  Not supported." );

		return gameplayRoom;
	}


	//SERVER calls this.
	public void TeleportTo ( GameplayRoom dest )
	{
		if (isClient)
			Debug.LogError ( name + ": CLIENT INITIATED TELEPORT" );

		SetNewParent ( dest.transform );

		//Do the same for all clients
		RpcTeleportTo ( dest.name );
	}

	[ClientRpc]
	public void RpcTeleportTo ( string name )
	{
		GameObject dest = GameObject.Find ( name );//TODO - inefficient
		SetNewParent ( dest.transform );
	}

	private void SetNewParent ( Transform newParent )
	{
		transform.SetParent ( newParent, true );
		FindGameplayRoom ();

		UpdateAll ();//This will change visibility (which includes mesh/solid, and the color)
	}




	//Server only
	//The ARMarker object is tracking realtive to the ARToolkit (Room) camera
	//TODO camera index
	public void OnMarkerTracked ( ARMarker marker )
	{
		if (isClient)
		{
			Debug.LogError ( "Client recieved an ARToolkit Marker update" );
			return;
		}

		Matrix4x4 pose = roomController.camera1ZeroPosition.transform.localToWorldMatrix * marker.TransformationMatrix;

		transformPositions [ 0 ] = ARUtilityFunctions.PositionFromMatrix ( pose );
		transformRotations [ 0 ] = ARUtilityFunctions.QuaternionFromMatrix ( pose );

		transformTimestamps [ 0 ] = Time.time;


//		Debug.Log ("ARToolkit Marker:  Marker Matrix " + marker.TransformationMatrix);
//		Debug.Log ("ARToolkit Marker:  Transform     " + transform.position + " / " + transform.rotation);
//		Debug.LogError ( "ARToolkit Says " + transformPositions [ 0 ] );

	}

	//Server only
	//TODO tango index
	public void SetMarker ( TangoSupport.Marker marker )
	{
		if (isClient)
		{
			Debug.LogError ( "Client recieved a Tango Marker update" );
			return;
		}

		m_rect.SetPosition ( 0, marker.m_corner3DP0 );
		m_rect.SetPosition ( 1, marker.m_corner3DP1 );
		m_rect.SetPosition ( 2, marker.m_corner3DP2 );
		m_rect.SetPosition ( 3, marker.m_corner3DP3 );
		m_rect.SetPosition ( 4, marker.m_corner3DP0 );

		// Apply the pose of the marker to the prefab.
		// This also applies implicitly to the axis and cube objects.
		transformPositions [ tangoOffset ] = marker.m_translation;
		transformRotations [ tangoOffset ] = marker.m_orientation;

		transformTimestamps [ tangoOffset ] = Time.time;

//		Debug.LogError ( "Tango Says " + transformPositions [ tangoOffset ] );
	}

	//Apply transformation after regular update so we have all readings in
	public void LateUpdate ()
	{
		isTrackingGood = false;
		
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
			if (transformTimestamps [ i ] > 0 && ( transformTimestamps [ i ] > ( t - dropoffDelay ) ))
			{
//				Debug.LogError ("Set  : " + transformTimestamps[i] + " " + transformPositions[i] );

				isTrackingGood = true;
				addAmount++; //Amount of separate values so far

				//Rotation
				result = Util.AverageQuaternion (ref culminativeQuaternion, transformRotations [ i ], transform.rotation, addAmount);
//				transform.eulerAngles = ( transformRotations [ i ] );

				//Position
				culminativePosition += transformPositions[i]; //We could divide by addAmount at this stage to get a valid reading without going through the whole array

			}
		}
			
		if (isTrackingGood)
		{
			transform.position = culminativePosition / ( float )addAmount;
			transform.rotation = result;
		}
	}



	//Server
	public void RecieveIMU ( Vector3 imuReadings )
	{
		if (isClient)
		{
			Debug.LogError ( "Client recieved a IMU update" );
			return;
		}
	}
}
