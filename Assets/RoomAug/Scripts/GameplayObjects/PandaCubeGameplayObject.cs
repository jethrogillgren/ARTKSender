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

	protected Vector3 corner3DP0;
	protected Vector3 corner3DP1;
	protected Vector3 corner3DP2;
	protected Vector3 corner3DP3;

	[HideInInspector]
	public GameplayRoom gameplayRoom; //The gameplayRoom that the cube is in
	[HideInInspector]
	public RoomController roomController;

	[HideInInspector]
	public LineRenderer lineRenderer;
	[HideInInspector]
	public Collider myCollider;

	public GameObject textMeshPrefab;
	[HideInInspector]
	public GameObject textMesh;
	[HideInInspector]
	public LinkedList<Vector3> textPopupOffsets; //Relative positions of each Text SLot
	Vector3 currentPositionOffset;
	protected const float distanceAbove = 0.05f;
	protected const float distanceAside = 0.1f;
	protected float[] xOffsets = new float[]{ 0f, +0.5f * distanceAside, -0.5f * distanceAside, distanceAside, -distanceAside, +0.5f * distanceAside, -0.5f * distanceAside, 0f };
	protected float[] zOffsets = new float[]{ +distanceAside, +0.5f * distanceAside, +0.5f * distanceAside, 0f, 0f, -0.5f * distanceAside, -0.5f * distanceAside, -distanceAside };



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
	public bool syn_isTrackingGood = true;
	[SyncVar]
	[HideInInspector]
	public bool syn_isTangoTrackingGood = false;
	[SyncVar]
	[HideInInspector]
	public bool syn_isARToolkitTrackingGood = false;

	[SyncVar]
	public Util.ElementalType syn_cubeType;



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

		lineRenderer = GetComponentInChildren<LineRenderer> ();
		if (!lineRenderer)
			Debug.LogError (name + " did not find it's Line Renderer");

		FindGameplayRoom ();
		BuildTextPopupOffset ();
	}

	//Servers always draw full.
	public override void OnStartServer ()
	{
		DrawFull ();
		Invoke ( "D", 1 );
	}

	private void D(){Svr_ShowDebugLabel = true;} //Getting around OnStartServer being called before Object has Started

	private bool svr_ShowingDebugLabels = false;
	public bool Svr_ShowDebugLabel
	{
		set{
			svr_ShowingDebugLabels = value;
			if(value)
			{
				DisplayTextAtPopupPosition ( ChooseTextPopupOffset ( true ), syn_cubeType.ToString () + " in " + gameplayRoom.roomName );
				StartCoroutine(TrackThenDestroyTextMesh(1f, 0)); // Or whatever delay we want.
			} else {
				StopCoroutine ( "TrackThenDestroyTextMesh" );
				ClearPopupPositionText ();
			}
		}
		get{
			return svr_ShowingDebugLabels;
		}
	}

	//Clients show hint icons over the cube to ClickPull
	public override void OnStartClient ()
	{
		InvokeRepeating ( "Cnt_CheckIfINeedToDisplayClickPullHint", 1, 1 );
	}

	public void Update() {
		if (isClient)
			Cnt_CheckInputTouch ();

		RerotatePopupText ();

	}
	public void LateUpdate()  //svr only
	{
		if (!isClient)
			Svr_ApplyTransformations ();
	}






	//Cubes are special, they are always active across rooms/clients.
	//This overrides the usual UpdateVisibility()
	public override void UpdateVisibility ()
	{
		base.UpdateVisibility ();

		//Clients see either wireframe or full versions depending on their gameplayRoom
		if (isClient)
		{
			if (gameplayRoom.cnt_roomActive)
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
		SetMaterialAndColor ( defaultMaterial, Util.GetColor ( syn_cubeType ) );
	}

	//Use to draw the cube when it is not in the same gameplay room as you, locally
	public void DrawAsWireframe ()
	{
//		Debug.Log ( name + " Rendering Wireframe " + cubeType );
		SetMaterialAndColor ( wireframeMaterial, Util.GetColor ( syn_cubeType ) );
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

		if (syn_isTangoTrackingGood)
			lineRenderer.startColor = lineRenderer.endColor = Color.blue;
		else if (syn_isTangoTrackingGood && syn_isARToolkitTrackingGood)
			lineRenderer.startColor = lineRenderer.endColor = Color.green;
		else
			return;

		lineRenderer.SetPosition ( 0, corner3DP0 );
		lineRenderer.SetPosition ( 1, corner3DP1 );
		lineRenderer.SetPosition ( 2, corner3DP2 );
		lineRenderer.SetPosition ( 3, corner3DP3 );
		lineRenderer.SetPosition ( 4, corner3DP0 );
	}

	//Register any new Gameplayroom we are in, and return it.
	//Client and Server both call this
	public GameplayRoom FindGameplayRoom ()
	{
		return GetComponentInParent<GameplayRoom> ();
	}

	protected void SetNewParent ( Transform newParent )
	{
		transform.SetParent ( newParent, true );
		FindGameplayRoom ();

		UpdateAll ();//This will change visibility (which includes mesh/solid, and the color)
	}
		
	public GameObject DEBUGgizmoPrfab;
	protected void BuildTextPopupOffset()
	{
		textPopupOffsets = new LinkedList<Vector3> ();

		//Eight Slots
		for(int i = 0; i < 8; i++)
		{
			Vector3 pos = new Vector3 ( xOffsets[i], distanceAbove, zOffsets[i] );
//			Debug.Log ( pos );
			textPopupOffsets.AddLast ( pos );

			GameObject debugGizmo = (GameObject)Instantiate(DEBUGgizmoPrfab, this.gameObject.transform);
			debugGizmo.transform.localPosition = pos;
		}

	}

	protected Vector3 ChooseTextPopupOffset( bool force = false )
	{
		if (textPopupOffsets == null)
			BuildTextPopupOffset ();

		//For each Position we consider
		foreach(Vector3 posOffset in textPopupOffsets)
		{
			//Calc the positions in world space
			Vector3 worldPoint = transform.position + posOffset;


			//Raycast to the Position
			Camera c = Camera.main;
			if(c)
			{
				Vector3 direction = worldPoint - c.transform.position;
//				Vector3 castShape = new Vector3(0.1f,0.1f,0.1f);//TODO
				float distance = Vector3.Distance ( c.transform.position, worldPoint );// * 1.2f;



				//TODO Use a Sphere Cast
//				bool hit = Physics.BoxCast ( c.transform.position, castShape, direction, Quaternion.identity, distance);
//				RaycastHit hitInfo;
//				hit = Physics.SphereCast(c.transform.position, 0.001f, direction, out hitInfo, distance);
				bool hit = Physics.Raycast ( c.transform.position, direction, distance);



				Debug.DrawRay(c.transform.position, posOffset);
				if (!hit)
					return posOffset;
			}
		}

		//If forced to, we would choose the first one.
		if (force)
			return textPopupOffsets.First.Value;
		
		return Vector3.zero;
	}

	public void DisplayTextAtPopupPosition( Vector3 positionOffset, string text )
	{
		if(positionOffset.magnitude==0 || text.Length==0)
		{
			Debug.LogWarning ("Unable to Display Text (" + text + ") at " + positionOffset );
			return;
		}

		if (textMesh != null)
			Destroy ( textMesh );

		textMesh = ( GameObject )Instantiate ( textMeshPrefab, this.gameObject.transform );
		textMesh.GetComponentInChildren<TextMesh> ().text = text;
		textMesh.transform.localPosition = positionOffset;
		currentPositionOffset = positionOffset;

		RerotatePopupText ();
	}

	//If appropiate, move an existing TextMesh to a better Posiiton, and Rotate too
	public void RepositionPopupText()
	{
		if (textMesh)
		{
			Vector3 newPosition = ChooseTextPopupOffset (true);
			if( ! newPosition.Equals( currentPositionOffset) )
			{
				DisplayTextAtPopupPosition ( newPosition, textMesh.GetComponentInChildren<TextMesh> ().text );
			}
		}
			
	}

	public void RerotatePopupText()
	{

		//		textMesh.transform.localPosition = currentPopupTextPositionOffset;
		if (textMesh && Camera.main)
		{
			textMesh.transform.LookAt ( Camera.main.transform );
		}
	}

	public void ClearPopupPositionText()
	{
		if (textMesh)
			Destroy ( textMesh );
		currentPositionOffset = Vector3.zero;
	}
	public void TrackThenDestroyTextMesh()
	{
		StopCoroutine("TrackThenDestroyTextMesh");
		StartCoroutine(TrackThenDestroyTextMesh(3f, 9)); // Or whatever delay we want.
	}

	//Update posiiton of the Text intermittently, and eventually hide it
	IEnumerator TrackThenDestroyTextMesh(float secondsBetweenChecks, float secondsUntilSelfHide)
	{
		float cnt = 0;
		// Repeat until keepSpawning == false or this GameObject is disabled/destroyed.
		while(cnt <= secondsUntilSelfHide && textMesh != null)
		{
			// Put this coroutine to sleep until the next spawn time.
			yield return new WaitForSeconds(secondsBetweenChecks);

			// Now it's time to spawn again.
			RepositionPopupText ();

			if(secondsUntilSelfHide!=0)
				cnt += secondsBetweenChecks;
		}

		//Now we are Finished, destroy the textMesh if it still exists
		ClearPopupPositionText ();
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
		{
			Debug.LogError ( name + ": CLIENT INITIATED CUBE TELEPORT" );
			return;
		}

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


		corner3DP0 =  marker.m_corner3DP0 ;
		corner3DP1 =  marker.m_corner3DP1 ;
		corner3DP2 =  marker.m_corner3DP2 ;
		corner3DP3 =  marker.m_corner3DP3 ;

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
		syn_isTrackingGood = false;
		syn_isTangoTrackingGood = false;
		syn_isARToolkitTrackingGood = false;

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

				syn_isTrackingGood = true;
				if( i < tangoOffset ) syn_isARToolkitTrackingGood = true;
				if( i >= tangoOffset ) syn_isTangoTrackingGood = true;

				addAmount++; //Amount of separate values so far

				//Rotation
				result = Util.AverageQuaternion (ref culminativeQuaternion, svr_transformRotations [ i ], transform.rotation, addAmount);
				//				transform.eulerAngles = ( transformRotations [ i ] );

				//Position
				culminativePosition += svr_transformPositions[i]; //We could divide by addAmount at this stage to get a valid reading without going through the whole array

			}
		}

		if (syn_isTrackingGood)
		{
			transform.localPosition = culminativePosition / ( float )addAmount;
			transform.localRotation = result;
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
	protected bool cnt_IsClickPullHinting = false;


	public void Cnt_CheckIfINeedToDisplayClickPullHint()
	{
		if (!isClient)
			return;

		//Check if the Object is good for ClickPulling
		if( !textMesh && syn_isTrackingGood && !gameplayRoom.cnt_roomActive && Util.Cnt_IsObjectInMainCamerasFOV ( this.transform ))
		{
			Util.JLogErr ("DisplayClickPullHint saw it", false);
			//Wait a bit if we're being pushy
			cnt_delaySoFar++;
			if (cnt_delaySoFar < cnt_timeToDelay)
				return;

			Vector3 posOffset = ChooseTextPopupOffset ();
			if (posOffset != Vector3.zero)
			{
				DisplayTextAtPopupPosition ( posOffset, "Tap cube to Pull" );
				TrackThenDestroyTextMesh ();
			} else {
				Debug.LogWarning ("We throught we could display the ClickPull hint but did not get one from ChooseTextPopupOffset() ");
			}


			//If we didn't get anything, wait for next time as player may have moved

		} else {
			cnt_delaySoFar = 0;
		}
	}

	protected void Cnt_CheckInputTouch() 
	{
		Debug.Log ( "Touch Count: " + Input.touchCount + "   TouchPhase: " + ( Input.touchCount > 0 ? Input.GetTouch ( 0 ).phase.ToString() : "N/A") );

		if ( syn_isTrackingGood && !gameplayRoom.cnt_roomActive && Input.touchCount > 0  &&  Input.GetTouch(0).phase == TouchPhase.Began  &&  Util.Cnt_IsObjectInMainCamerasFOV ( this.transform ) )
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
					ClearPopupPositionText ();
					Cnt_PlayerController.CmdClickPullCube( cubeContentName, Util.GetCurrentMainGameplayRoom().roomName );
					CmdTest ();
				}
			}
		}
	}
}
