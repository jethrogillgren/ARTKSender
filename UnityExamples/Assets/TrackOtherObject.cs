using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackOtherObject : MonoBehaviour {

	public GameObject other;

	// Update is called once per frame
	void LateUpdate () {
		if (other && other.gameObject.activeInHierarchy) {
//			this.gameObject.SetActive (true);
			this.gameObject.transform.position = other.gameObject.transform.position;
		}



	}
}