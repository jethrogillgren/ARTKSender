using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Block any and all water until opened
//Acting means blocking
public class PerfectGateWGO : WGO {

    // Use this for initialization
    public override void Start() {
        base.Start();
    }

    // Update is called once per frame
    void Update() {

    }

    //Toggle switched state.  Pass to outputs
    public override bool Act() {
        Debug.Log( "PerfectGate Act" );
        if ( acting ) {
            
            //Opening Gate.  If there is water pressure, pass it on
            DebugSetColour( Color.white );

            if ( WaterIn > 0 ) {
                foreach ( WGO go in outputs ) {
                    
                    for ( int i = 0; i < WaterIn; i++ ) //For each current WaterIn
                        go.FeedWater( this );
                }
            }

        } else {
            //Closing Gate.  If there was water pressure, cut it off
            DebugSetColour( Color.black );

            if ( WaterIn > 0 ) {
                foreach ( WGO go in outputs ) {

                    for ( int i = 0; i < WaterIn; i++ ) //For each current WaterIn
                        go.StopWater( this );
                }
            }
        }

        acting = !acting;
        return true;
    }

    public override bool FeedWater( WGO from ) {
        WaterIn++;
        if ( !acting ) {//If we are open
            foreach ( WGO go in outputs )//Give every output
                go.FeedWater( this );//The same water increase
        }
        return true;
    }

    public override bool StopWater( WGO from ) {
        WaterIn--;
        if ( !acting ) {//If we are open
            foreach ( WGO go in outputs )//Give every output
                go.FeedWater( this );//The same water decrease
        }
        return true;
    }
}
