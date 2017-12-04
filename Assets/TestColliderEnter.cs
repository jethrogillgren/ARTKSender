using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestColliderEnter : MonoBehaviour {

	public void OnTriggerEnter ( Collider collision ) {//Handle usual teleport collission with player trigger
		Debug.Log(name + " Triggered by: " + collision.name );
	}
}
