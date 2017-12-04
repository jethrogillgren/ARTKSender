using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomAugFraggedWallExplodeOnCollider : MonoBehaviour {

	public Collider target1;

	public void OnTriggerEnter ( Collider collision ) {//Handle usual teleport collission with player trigger
		Debug.Log ( name + " Triggered by: " + collision.name );

		if (collision == target1)
		{
			Debug.LogError ( "FRAGMENT  Calling Damage on " + gameObject.name );

			gameObject.SendMessage("Damage", 25f, SendMessageOptions.DontRequireReceiver);			

		}
	}
}