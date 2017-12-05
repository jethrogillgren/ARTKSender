using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerDistanceCollider : MonoBehaviour {


	public void OnTriggerEnter ( Collider collision )
	{
		Debug.Log ( name + " Collided with " + collision.name );
	}
}
