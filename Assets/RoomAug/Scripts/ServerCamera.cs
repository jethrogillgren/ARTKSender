// Credit to damien_oconnell from http://forum.unity3d.com/threads/39513-Click-drag-camera-movement
// for using the mouse displacement for calculating the amount of camera movement and panning code.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Click Toggles, and Camera Control
public class ServerCamera : MonoBehaviour {

	private Camera cam;

	private Rect originalRect; //Don't edit these or reference them, make copies
	private Rect fullscreenRect;

	private bool pleaseToggleFocus = false;	//We do the actual focusing in LateUpdate(), so all cameras have a chance to register the Update Click.  Otherwise in one click, a unfocus happens and a different camera will pounce on the chance to immedietly focus itself
	public bool focused = false;

	public bool focusToSecondMonitor = false;//As opposed to default Fullscreeen

	public float turnSpeed = 0.1f;		// Speed of camera turning when mouse moves in along an axis
	public float panSpeed = 0.2f;		// Speed of the camera when being panned
	public float zoomSpeed = 1.0f;		// Speed of the camera going back and forth

	private Vector3 mouseOrigin;	// Position of cursor when mouse dragging starts
	private bool isPanning = false;		// Is the camera being panned?
	private bool isRotating = false;	// Is the camera being rotated?

	// Use this for initialization
	void Start () {
		cam = GetComponent<Camera>();//Not the depth camera, the main one
		originalRect = new Rect ( cam.rect );//

		fullscreenRect = new Rect ( 0, 0, Screen.width, Screen.height );

//		if (focused)
//			ToggleFocus ();
	}

	public void AddAnimalsToCullingMask()
	{
		
	}
	
	//We do the actual focusing in LateUpdate(), so all cameras have a chance to register the Update Click
	void Update () {

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

				} else if ( !IsOtherFocused() ) { // 
					pleaseToggleFocus = true; //LateUpdate does the actual change
				} //Else - someone else is fullscreen - don't act as the click was for them.
					
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
		if( focused ) //We are shrinking back
		{
			cam.rect = new Rect( originalRect );
			DecreaseDepth ();
		}
		else //We are Focusing in
		{
			IncreaseDepth ();

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

	private bool IsOtherFocused()
	{
		ServerCamera[] cams = transform.parent.GetComponentsInChildren<ServerCamera> ();
		foreach(ServerCamera c in cams)
		{
			//If focused, or about to focus in LateUpdate
			if ( (c.focused || c.pleaseToggleFocus)  && c.name != this.name)
				return true;
		}

		return false;
	}
}
