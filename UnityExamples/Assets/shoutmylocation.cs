using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shoutmylocation : MonoBehaviour {

	// Use this for initialization
	void Start () {
		//
	}
	
	// Update is called once per frame
	void Update () {
		Util.JLog(transform.position.ToString());
	}
}