using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchWGO : WGO {

    // Use this for initialization
    public override void Start() {
        base.Start();
    }

    // Update is called once per frame
    void Update() {

    }

    void OnMouseUp() {
        /*Do whatever here as per your need*/
    }

    void OnMouseDown() {
        Act();
    }

    //Toggle switched state.  Pass to outputs
    public override bool Act() {
        if ( acting ) {
            //Turning switch off
            Util.JLog("Switch off");
            DebugSetColour(Color.white);
            foreach ( WGO go in outputs ) {
                go.Act();
            }

        } else {
            //Turning Switch on
            Util.JLog( "Switch on" );
            DebugSetColour( Color.black );
            foreach ( WGO go in outputs ) {
                go.Act();
            }
        }

        acting = !acting;
        return true;
    }


}
