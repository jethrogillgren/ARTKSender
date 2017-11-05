using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyGameplayObject : BaseGameplayObject {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if( isServer && gameplayState == GameplayState.Started) {
			//
		}
	}



//	public override void test ()
//	{
//		throw new System.NotImplementedException ();
//	}
//	public override void test2() {
//		throw new System.NotImplementedException ();
//	}

}