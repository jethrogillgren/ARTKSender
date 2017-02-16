using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackOtherObject : MonoBehaviour {

	public GameObject other;

	public bool alsoLookInSameDirection = false;

	// Update is called once per frame
	void LateUpdate () {
		if (other && other.gameObject.activeInHierarchy) {
//			this.gameObject.SetActive (true);
			this.gameObject.transform.position = other.gameObject.transform.position;

			if( alsoLookInSameDirection ) {
//				this.gameObject.transform.forward = other.gameObject.transform.forward;
				this.gameObject.transform.rotation = other.gameObject.transform.rotation;

			}
		}



	}
}