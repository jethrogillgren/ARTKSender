using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackOtherObject : MonoBehaviour {

	public GameObject other;

	public bool trackPosition = true;
	public bool trackRotation = true;

	// Update is called once per frame
	void LateUpdate () {
		if (other && other.gameObject.activeInHierarchy) {

			if (trackPosition)
				this.gameObject.transform.position = other.gameObject.transform.position;

			if( trackRotation )
				this.gameObject.transform.rotation = other.gameObject.transform.rotation;

		}
	}
}