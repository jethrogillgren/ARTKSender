using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//In or Out always the same way due to gravity.
public class UphillTunnelWGO : WGO {

    //State transitions finished (consistant throughput etc..) instead of filling up etc...
    public bool equalibrium = true;

    private bool overflowing = false;

    //1 = 100% to capacity with water.
    private double fillPercent = 0;

    public double speed = 0.1;

	// Use this for initialization
    public override void Start() {
        base.Start();
	}
	
    //Word way towards equalibrium.
	void Update () {
        //Debug.Log();
        //Flow in
        if( WaterIn > 0 ) {

            if (fillPercent < 1) {//Not full yet
                fillPercent += WaterIn * speed;//Filling
                equalibrium = false;
            } else {//Full
                overflow(true);
                equalibrium = true;
            }

        //Drin out
        } else if ( WaterIn < 0 ) {

            //A full tunnel which is now draining
            if( fillPercent >=1 )
                overflow(false);//Stops overflowing water to outputs
            
            if(fillPercent > 0 ) {
                fillPercent += WaterIn * speed;//Draining
                equalibrium = false;
            } else {//Empty
                equalibrium = true;
            }

        //Stable
        } else {
            equalibrium = true;//Settled at X% full 
        }

        if( fillPercent <=0 )
            DebugSetColour( Color.white );
        else if ( fillPercent <0.5 )
            DebugSetColour( Color.cyan );
        else if ( fillPercent <=1 )
            DebugSetColour( Color.blue );
        else 
            DebugSetColour( Color.black );
	}

    public override bool FeedWater(WGO from)
    {
        Debug.Log( "UphillTunnel FeedWater");
        WaterIn++;

        if (overflowing) {//If we are already overflowing
            foreach ( WGO go in outputs )//Give every output
                go.FeedWater( this ); //More water
        }

        return true;
    }

    public override bool StopWater(WGO from)
    {
        Debug.Log( "UphillTunnel StopWater" );
        WaterIn--;
        if ( overflowing ) {//If we were overflowing
            foreach ( WGO go in outputs )//Give every output
                go.FeedWater( this ); //Less water.  Because by the time we trigger overflow(stop), we will only decrement the outflow by the then amount
        }
        return true;
    }


    //Spill out water to outputs
    private void overflow( bool start ) {
        Debug.Log( "UphillTunnel Overflow: " + start );
        foreach (WGO go in outputs)
        {
            if ( start ) { //We have just filled up enough to overflow.  Waterin must be positive
                for ( int i = 0; i < WaterIn && WaterIn>0; i++ ) //For each current WaterIn
                    go.FeedWater( this );//Flow it out
            } else { //We have just stopped overflowing.  WaterIn must be negative
                for ( int i = 0; i > WaterIn && WaterIn < 0; i++ ) //For eachcurrent  Water Out
                    go.StopWater( this );//Flow it out.

            }
        }

        overflowing = start;

    }

}
