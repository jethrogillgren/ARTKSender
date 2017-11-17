using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tango;

//Represents one of 4 real life cube markers, which has the IMU and the same tag for all 6 sides.
//Always started, but 
public class PandaCubeGameplayObject : BaseGameplayObject {

    public string cubeContentName; //1, 2, .. 16

    public bool isTrackingGood = true;

    //A cube is always active in only one room.  It will appear translucent in others and not interact.
    //public GameplayRoom room;

    // Use this for initialization
    public override void Start() {
        base.Start();
    }
	
	// Update is called once per frame
	void Update () {
		
	}


    /// <summary>
    /// The bounding box LineRenderer object.
    /// </summary>
    public LineRenderer m_rect;

    /// <summary>
    /// Update the object with a new marker.
    /// </summary>
    /// <param name="marker">
    /// The input marker.
    /// </param>
    public void SetMarker( TangoSupport.Marker marker ) {
        m_rect.SetPosition( 0, marker.m_corner3DP0 );
        m_rect.SetPosition( 1, marker.m_corner3DP1 );
        m_rect.SetPosition( 2, marker.m_corner3DP2 );
        m_rect.SetPosition( 3, marker.m_corner3DP3 );
        m_rect.SetPosition( 4, marker.m_corner3DP0 );

        // Apply the pose of the marker to the prefab.
        // This also applies implicitly to the axis and cube objects.
        transform.position = marker.m_translation;
        transform.rotation = marker.m_orientation;
    }

    public void RecieveIMU( Vector3 imuReadings ) {
        //Set the cubes position
    }
}
