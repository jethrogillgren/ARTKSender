using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingCubeGameplayObject : BaseGameplayObject {

	public SlidingFloorGameplayObject floor;

	public Face currentForce = Face.None;//A force is pushing us still.
	public bool sliding {
		get {
			return (floor!=null  && currentForce != Face.None );
		}
	}

	public const float speed = 1f;

	public enum Face
	{
		None,
//		Up,
//		Down,
		East,
		West,
		North,
		South
	}

	// Use this for initialization
    public override void Start() {
        base.Start();

		collectSlidingFloor ();
//		pushableFaces = new Face[]{Face.North, Face.West, Face.South, Face.East};
	}

	void Update() {
	
		if ( gameplayState == GameplayState.Started ) {

			if (sliding) {
				Debug.Log ("Sliding");
				//Do the move
				float step = speed * Time.deltaTime;
				transform.position = Vector3.MoveTowards(transform.position, floor.transform.position, step);

				//Check if we arrived
				if (transform.position == floor.transform.position) {
					Debug.Log ("We have arrived... continuing?");
					//Carry on travelling?
					applyPush (currentForce);
				}

			} else {

				//On Touching a still cube, start a move
				if (Input.touchCount == 1) {
					//One finger is touching screen
					Touch t = Input.GetTouch (0);

					//If we have started a touch
					if (t.phase == TouchPhase.Began) {
						//And it was on a pushable cube face
						Face f = GetHitFace (t);
						Debug.Log ("Touch on face: " + f);

						if (f != Face.None)
							applyPush (f);


						//If we are dragging from this object
					} else if (t.phase == TouchPhase.Moved) {

						//If we have Ended the touch
					} else if (t.phase == TouchPhase.Ended) {
							
						//If the touch was cancelled
					} else if (t.phase == TouchPhase.Canceled) {
						
					}
				}
			}

		}

	} //Else we are not active - do nothing


	//Check if the push will result in a slide
	public bool applyPush(Face f) {
		Debug.Log ( "Pushing cube " + name + " On floor " + floor.name + " From Face: " + f );

		SlidingFloorGameplayObject next = null;
		if (f == Face.South)
			next = floor.north;
		else if (f == Face.East)
			next = floor.west;
		else if (f == Face.West)
			next = floor.east;
		else if (f == Face.North)
			next = floor.south;


		if (next == null || next.blocked) {
			currentForce = Face.None;
			Debug.Log ("Can't Push.  " + (next == null ? "No floors in that direction" : "Floor is blocked") );
			return false;
		} else {
			applySlide (f, next);
			return true;
		}
		
	}

	//When we decide we will slide, we count as instantly owning the next cube.  Others can take our square now.
	//This assumes that the next has been validated already
	private void applySlide(Face f, SlidingFloorGameplayObject next) {
		Debug.Log ("Applying slide from " + f + " to " + next.name);
		next.cube = this;
		floor.cube = null;

		floor = next;
		currentForce = f;
	}


	private Face GetHitFace(Touch t) {
		RaycastHit hitInfo;

		//If ray hit, and that hit was me or my kid
		if (Physics.Raycast (Camera.main.ScreenPointToRay (t.position), out hitInfo )
			&&  ( hitInfo.collider.gameObject == gameObject  || hitInfo.collider.gameObject == transform.GetChild(0).gameObject ) ) {
			return GetHitFace (hitInfo);
		} else {
			return Face.None;
		}
	}

	private Face GetHitFace(RaycastHit hit)
	{
		Vector3 incomingVec = hit.normal - Vector3.up;

		if (incomingVec == new Vector3(0, -1, -1))
			return Face.South;

		if (incomingVec == new Vector3(0, -1, 1))
			return Face.North;

//		if (incomingVec == new Vector3(0, 0, 0))
//			return Face.Up;
//
//		if (incomingVec == new Vector3(1, 1, 1))
//			return Face.Down;

		if (incomingVec == new Vector3(-1, -1, 0))
			return Face.West;

		if (incomingVec == new Vector3(1, -1, 0))
			return Face.East;

		return Face.None;
	}


	public void collectSlidingFloor() {
		floor = null;
		foreach (SlidingFloorGameplayObject f in getAllSlidingFloors()) {
			//If in the same position in 2D axis
			if( f.transform.localPosition.x == transform.localPosition.x  
				&& f.transform.localPosition.z == transform.localPosition.z ) {
				floor = f;
			}
		}
	}

	private SlidingFloorGameplayObject[] getAllSlidingFloors() {
		return FindObjectsOfType( typeof(SlidingFloorGameplayObject) ) as SlidingFloorGameplayObject[];
	}
}
