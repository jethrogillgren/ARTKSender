using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowMoTriggerGameplayObject : BaseGameplayObject {

	[Space]

	public Collider targetCollider;
	public MonoBehaviour targetObj;
	public string targetInvoke;

	public void OnTriggerEnter( Collider collision ) {

		//Deer Teleporting
		if (collision == targetCollider)
		{
			Debug.Log ( name + " Triggering slowmo for " + collision.name );
			Trigger ();
		}
	}

	public void Trigger() {
		targetObj.SendMessage(targetInvoke, SendMessageOptions.RequireReceiver);
	}
}
