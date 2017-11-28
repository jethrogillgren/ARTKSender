using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class RotateY : NetworkBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(!isClient)
			this.gameObject.transform.Rotate (new Vector3 (0, 1, 0));
	}
}
