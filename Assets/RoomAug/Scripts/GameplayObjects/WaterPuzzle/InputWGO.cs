using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputWGO : WGO {

	// Use this for initialization
	public override void Start () {
        base.Start();
	}

    //Start/Stop creating water
    public override bool Act()
    {
        Util.JLog( "Input Act" );
        if (acting)
        {
            DebugSetColour( Color.white );
            //Turning water creating off
            foreach (WGO go in outputs)
            {
                go.StopWater(this);
            }
        }
        else
        {
            DebugSetColour( Color.cyan );
            //Turning water creating on
            foreach (WGO go in outputs)
            {
                go.FeedWater(this);
            }
        }

        acting = !acting;
        return true;
    }
}