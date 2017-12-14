// Credit to damien_oconnell from http://forum.unity3d.com/threads/39513-Click-drag-camera-movement
// for using the mouse displacement for calculating the amount of camera movement and panning code.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Click Toggles, and Camera Control
//SERVER ONLY
public class ServerCamera : MonoBehaviour {

	public Util.ElementalType elementalType;

	private Camera cam;
	private GameplayRoom gameplayRoom;
	private RoomController roomController;

	private Rect originalRect; //Don't edit these or reference them, make copies
	private Rect fullscreenRect;

	private bool pleaseToggleFocus = false;	//We do the actual focusing in LateUpdate(), so all cameras have a chance to register the Update Click.  Otherwise in one click, a unfocus happens and a different camera will pounce on the chance to immedietly focus itself
	public bool focused = false;

	public bool focusToSecondMonitor = false;//As opposed to default Fullscreeen

	protected float turnSpeed = 3f;		// Speed of camera turning when mouse moves in along an axis
	protected float panSpeed = 0.2f;	// Speed of the camera when being panned
	protected float zoomSpeed = 0.1f;	// Speed of the camera going back and forth

	private Vector3 mouseOrigin;	// Position of cursor when mouse dragging starts
	private bool isPanning = false;		// Is the camera being panned?
	private bool isRotating = false;	// Is the camera being rotated?

	private bool BUG_DELAY_STARTED = false;

	// Use this for initialization
	void Start () {
		Invoke ( "BUG_DELAY_START", 1 );
	}

	void BUG_DELAY_START() //TODO - on Start() other objects were not awake yet?
	{
		cam = GetComponent<Camera>();//Not the depth camera, the main one

		//We adopt ourselves to the Room we are Watching so we track it's teleporting
		roomController = GameObject.FindObjectOfType<RoomController> ();
		if (roomController && elementalType != Util.ElementalType.none)
			gameplayRoom = roomController.GetGameplayRoomByType ( elementalType );
		else
			Debug.LogError ("Camera Unable to Track its Room");
		if (gameplayRoom)
			this.transform.SetParent ( gameplayRoom.transform, false );

		originalRect = new Rect ( cam.rect );//
		fullscreenRect = new Rect ( 0, 0, Screen.width, Screen.height );

		if (focused)
		{
			focused = false;
			pleaseToggleFocus = true;
		}
		BUG_DELAY_STARTED = true;
	}

	
	//We do the actual focusing in LateUpdate(), so all cameras have a chance to register the Update Click
	void Update () {
		if (!BUG_DELAY_STARTED)
			return;
		
		//Focussing / Unfocussing
		if( Input.GetKeyDown(KeyCode.Space) ) 
		{
			//Get the click Position
			Vector3 mouse = Input.mousePosition;

			//Check if it is within mainCamera.pixelRect;
			if( cam.pixelRect.Contains(mouse) )
			{

				if( focusToSecondMonitor )
				{
					/*/TODO pleaseToggleFocus ();*/

				} else { // 
					pleaseToggleFocus = true; //LateUpdate does the actual change
				}
					
			}

			//If Yes:  Focus();
		}

		if (focused)
		{
			// Get the left mouse button
			if (Input.GetMouseButtonDown ( 0 ))
			{
				// Get mouse origin
				mouseOrigin = Input.mousePosition;
				isRotating = true;
			}

			// Get the right mouse button
			if (Input.GetMouseButtonDown ( 1 ))
			{
				// Get mouse origin
				mouseOrigin = Input.mousePosition;
				isPanning = true;
			}

			if( Input.mouseScrollDelta.y != 0 )
			{
				Vector3 move = zoomSpeed * Input.mouseScrollDelta.y * transform.forward; 
				transform.Translate ( move, Space.World );
			}

			// Disable movements on button release
			if (!Input.GetMouseButton ( 0 ))
				isRotating = false;
			if (!Input.GetMouseButton ( 1 ))
				isPanning = false;
			

			// Rotate camera along X and Y axis
			if (isRotating)
			{
				Vector3 pos = cam.ScreenToViewportPoint ( Input.mousePosition - mouseOrigin );
//				pos = ( Input.mousePosition - mouseOrigin );
				transform.RotateAround ( transform.position, transform.right, -pos.y * turnSpeed );
				transform.RotateAround ( transform.position, Vector3.up, pos.x * turnSpeed );
			}

			// Move the camera on it's XY plane
			if (isPanning)
			{
				Vector3 pos = cam.ScreenToViewportPoint ( Input.mousePosition - mouseOrigin );

				Vector3 move = new Vector3 ( pos.x * panSpeed, pos.y * panSpeed, 0 );
				transform.Translate ( move, Space.Self );
			}

		}
	}

	void LateUpdate()
	{
		if (!BUG_DELAY_STARTED)
			return;
		
		if( pleaseToggleFocus )
		{
			ToggleFocus ();
			pleaseToggleFocus = false;
		}
	}

	//Make this camera the main viewed one, or back again
	//Either make it fullscreen, or if available put it on the second monitor
	public void ToggleFocus()
	{
		if (!BUG_DELAY_STARTED)
			return;
		
		if( focused ) //We are shrinking back
		{
			cam.rect = new Rect( originalRect );
//			cam.tag = "Untagged"; //Untagged by the Camera which takes the tag over
			DecreaseDepth ();
		}
		else //We are Focusing in
		{
			IncreaseDepth ();
			cam.tag = "MainCamera";
			roomController.SwapRoomInMainRoom(gameplayRoom); //Camera moves with the swap, so we can see Physical Room Wide Elements

			if (focusToSecondMonitor)
			{
				//TODO
			}
			else
			{
				cam.rect = new Rect ( fullscreenRect );
			}
		}

		focused = !focused;
	}


	private void IncreaseDepth()
	{
		cam.depth+=5;
		GetComponentInChildren<Camera>().depth += 5;
	}
	private void DecreaseDepth()
	{
		cam.depth-=5;

		GetComponentInChildren<Camera>().depth -= 5;
	}

	protected void UntagOtherCameraMains()
	{
		foreach( GameObject otherCam in GameObject.FindGameObjectsWithTag("MainCamera") )
		{
			if(otherCam != this.cam)
				cam.tag = "Untagged";
		}
	}

}
