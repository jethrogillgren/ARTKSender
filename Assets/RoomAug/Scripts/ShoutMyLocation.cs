using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shoutmylocation : MonoBehaviour {

	private int c = 0;
	public int m_step = 100;

	// Use this for initialization
	void Start () {
		//
	}
	
	// Update is called once per frame
	void Update () {
		if( c % m_step == 0 )
			Util.JLog( gameObject.name + "'s pos: " + transform.position.ToString());
		++c;
	}
}