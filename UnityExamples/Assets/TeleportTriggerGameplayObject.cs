using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportTriggerGameplayObject : BaseGameplayObject {

	public GameObject m_playerCollider;
	public bool m_originalSide = true; //True means we are behind the BLue FOrward line of the teleport to start.

	// Use this for initialization
	void Start () {
	}


//	TODO - Does not handle well if user enters trigger form one side, but exits it on the same side!
//	void OnTriggerExit (Collider collision) {
//		if( gameObject.transform.position.z > collision.transform.position.z )
//		m_originalSide = !m_originalSide;

//	}

	void OnTriggerEnter (Collider collision) {
		
		if (m_playerCollider && collision.name == m_playerCollider.name) {
			Util.JLog ( gameObject.name + " has been triggered by " + collision.name);

			Util.JLog ("Collission Forward: " + collision.transform.forward);
			Util.JLog ("Teleport Forward  : " + gameObject.transform.forward);

			float angle = Vector3.Angle(collision.transform.forward, gameObject.transform.forward);
			Util.JLog ( "Angle: " + angle + "  We are on the " + (m_originalSide?"original":"other") + "side" );

			if( (m_originalSide && angle < 90)   ||   (!m_originalSide && angle > 90) ) {
				Util.JLog ("Passing Forward");
			} else {
				Util.JLog ("Passing Backwards!  Cheeky!");
			}

			m_originalSide = !m_originalSide;
		}
	}
}