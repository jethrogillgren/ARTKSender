using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyGameplayObject : BaseGameplayObject {

	// Use this for initialization
    public override void Start() {
        base.Start();
	}
	
	// Update is called once per frame
	void Update () {
        if( !isClient && gameplayState == GameplayState.Started) {
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