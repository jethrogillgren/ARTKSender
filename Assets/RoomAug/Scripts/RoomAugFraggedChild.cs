using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//I never bothred putting this in
//These modifications are also done in FraggedChild.  But if we reimport that will be lost.
//Should really replace all fraggedChild with RoomAugFraggedChild but that would take ages.
public class RoomAugFraggedChild : FraggedChild {

	public void OnTriggerEnter ( Collider collision ) {//Handle usual teleport collission with player trigger

		if (( fragControl.collideMask.value & 1 << collision.gameObject.layer ) == 1 << collision.gameObject.layer)
		{
			Debug.Log ( name + "  Hit by " + collision.name );
			Damage ( fragControl.collidefragMagnitude );
		}
	}
}
