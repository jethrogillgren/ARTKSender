using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingFloorGameplayObject : BaseGameplayObject {

	private const int posStep = 1; //Distance from floor to adjoining floor.

	//floors next to me. (Auto-Found, or null)  TODO private & accessors
	public SlidingFloorGameplayObject north;
	public SlidingFloorGameplayObject south;
	public SlidingFloorGameplayObject west;
	public SlidingFloorGameplayObject east;

	public SlidingCubeGameplayObject cube;
	public GameObject blockingObject;

	public bool blocked {
		get {
			if( name == "stop" || gameObject.name == "stop" )
				Debug.Log ("STOPPER.   cube:" + cube + "   blockingObject:" + blockingObject + "     ret: " + (cube != null  || blockingObject != null) );
			return (cube != null  || blockingObject != null);
		}
	}
	
    public override void Start() {
        base.Start();

		collectAdjoiningFloors ();
		collectSlidingCube ();
	}


	public void collectAdjoiningFloors() {

		north = null;
		south = null;
		east = null;
		west = null;

		Vector3 northOne = transform.localPosition + new Vector3(0, 0, +posStep);
		Vector3 southOne = transform.localPosition +  new Vector3(0, 0, -posStep);
		Vector3 westOne = transform.localPosition + new Vector3(-posStep, 0, 0);
		Vector3 eastOne = transform.localPosition + new Vector3(posStep, 0, 0);

		foreach( SlidingFloorGameplayObject f in getAllSlidingFloors () ) {
			Debug.Log ( "Floor " + name + " (" + transform.localPosition + ") is checking " + f.name + " (" + f.transform.localPosition + ")." );
			Debug.Log ( "North one is " + (northOne) );
			Debug.Log ( "South one is " + (southOne) );
			Debug.Log ( "East one is " + (eastOne) );
			Debug.Log ( "West one is " + (westOne) );

			if (f.transform.localPosition == ( northOne))
				north = f;
			else if (f.transform.localPosition == ( southOne))
				south = f;
			else if (f.transform.localPosition == ( westOne))
				west = f;
			else if (f.transform.localPosition == ( eastOne))
				east = f;
		}
	}

	public void collectSlidingCube() {
		cube = null;
		foreach (SlidingCubeGameplayObject c in getAllSlidingCubes()) {
			//If in the same position in 2D axis
			if( c.transform.localPosition.x == transform.localPosition.x  
				&& c.transform.localPosition.z == transform.localPosition.z ) {
				cube = c;
			}
		}

	}


	private SlidingFloorGameplayObject[] getAllSlidingFloors() {
		return FindObjectsOfType( typeof(SlidingFloorGameplayObject) ) as SlidingFloorGameplayObject[];
	}
	private SlidingCubeGameplayObject[] getAllSlidingCubes() {
		return FindObjectsOfType( typeof(SlidingCubeGameplayObject) ) as SlidingCubeGameplayObject[];
	}
}
