using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Tango;

//Controlls the Tango lifecycle of a player.  Local to the player object.  All networking happens through RoomAugPlayerController
public class TangoApplicationController : MonoBehaviour, ITangoLifecycle, ITangoVideoOverlay {
    //ITangoEvent, ITangoPose, ITangoDepth {

    public string m_areaDescriptionName = "JethroTestAreaDescription";//For dev when we have multiple potential descriptions.
    public GameObject m_relocalizationOverlay;


    [HideInInspector]
    public AreaDescription m_areaDescription = null;//Kept so we can relocalise after any Pause.

    [HideInInspector]
    public ApplicationStateMachine state;
    [HideInInspector]
    public TangoApplication m_tangoApplication;
    [HideInInspector]
    public GameplayController m_gameplayController;
    [HideInInspector]
    public TangoARPoseController m_poseController;
    [HideInInspector]
    public RoomAugPlayerController m_roomAugPlayerController;

    [HideInInspector]
    public TangoDynamicMesh m_dynamicMesh;
    //[HideInInspector]
    //public MeshRenderer m_dynamicMeshRenderer;



    //MARKER STUFF
    /// <summary>
    /// Length of side of the physical AR Tag marker in meters.
    /// </summary>
    private const double MARKER_SIZE = 0.1397;

    /// <summary>
    /// The list of markers detected in each frame.
    /// </summary>
    private List<TangoSupport.Marker> m_markerList;



    // Use this for initialization
    void Start() {
        Util.JLog( " Application Starting Up" );
        state = new ApplicationStateMachine();

        m_markerList = new List<TangoSupport.Marker>();

        m_tangoApplication = FindObjectOfType<TangoApplication>();
        m_gameplayController = FindObjectOfType<GameplayController>();
        m_poseController = FindObjectOfType<TangoARPoseController>();
        m_roomAugPlayerController = GetComponent<RoomAugPlayerController>();

        m_dynamicMesh = FindObjectOfType<TangoDynamicMesh>();//Just for debug - remove for prod
                                                             //m_dynamicMeshRenderer = FindObjectOfType<MeshRenderer>();

        if ( m_tangoApplication != null ) {
            m_tangoApplication.Register( this );
            m_tangoApplication.RequestPermissions();
        }

        DebugEnable3DR( false );
        //		OnMeshViewToggle (false);
    }

    // Update is called once per frame
    void Update() {
        if ( !state.isTangoConnected() ) {
            return;
        }


    }

    public void OnGUI() {

        //Toggle Relocatilisation Graphic depending on state
        if ( state.CurrentState == ProcessState.Localised ) {
            m_relocalizationOverlay.SetActive( false );
        } else if ( state.CurrentState == ProcessState.Active ) {
            m_relocalizationOverlay.SetActive( true );
        }
    }

    public bool LoadADFFromDevice() {
        AreaDescription[] list = AreaDescription.GetList();

        Util.JLog( " There are " + ( list == null ? 0 : list.Length ) + " Area Descriptions Available: ", true );
        if( list != null && list.Length != 0 )
            Util.JLogArrErr( list, x => x.GetMetadata().m_name, true );

        if ( list != null && list.Length != 0 ) {

            // Find our area Description
            foreach ( AreaDescription areaDescription in list ) {
                AreaDescription.Metadata metadata = areaDescription.GetMetadata();

                if ( metadata.m_name == m_areaDescriptionName ) {
                    m_areaDescription = areaDescription;

                    m_tangoApplication.Startup( m_areaDescription );
                    return true;
                }
            }
        }

        return false;
    }

    public void OnTangoPermissions( bool permissionsGranted ) {
        if ( permissionsGranted ) {
            state.setPermitted();

            //Try to load the ADF, but if it fails we try alternatives
            if ( !LoadADFFromDevice() ) {

                //We should only waste time installing a new ADF the first time we see it in the APK.
                Util.JLogErr( "No AreaDescription Found matching " + m_areaDescriptionName + " on Device.  Loading in " + System.IO.Path.Combine( Application.streamingAssetsPath, m_areaDescriptionName + ".adf" ) + " from the APK." );

                if ( AreaDescription.ImportFromFile( System.IO.Path.Combine( Application.streamingAssetsPath, m_areaDescriptionName + ".adf" ) ) ) {
                    Util.JLog( "Imported new ADF file from the APK! " + m_areaDescriptionName, true );
                    if ( LoadADFFromDevice() ) //TODO this seems to always fail?  had to manually use the TangoExamples manager.
                        return;
                    else
                        Util.JLogErr("Failed to load the APK ADF", true);

                    //No ADF on Device or APK... time to give up and accept horrendous drift?
                }

                Util.JLogErr( "No area descriptions available.  Starting without one in Drift corrected Motion tracking mode.", true );
                m_tangoApplication.EnableAreaDescriptions = false;
                m_tangoApplication.EnableMotionTracking = true;
                m_tangoApplication.EnableDriftCorrection = true;

                m_tangoApplication.Startup( null );
                state.MoveNext( Command.Localise );

            }
        } else {
            Util.JLogErr( "Motion Tracking and Area Learning Permissions Needed", true );
            state.MoveNext( Command.Exit );
            Application.Quit();
        }
    }


    public void OnTangoServiceConnected() {
        Util.JLog( " Tango Service Connected" );
        state.MoveNext( Command.Connect );

    }

    public void OnTangoServiceDisconnected() {
        Util.JLog( " Tango Service Disconnected" );
        state.MoveNext( Command.Disconnect );
        //TODO Lifecycle reconnect
    }

    public void OnApplicationPause( bool pauseStatus ) {

        //If ther was a 3DR mesh, it is now invalid
        if ( m_dynamicMesh != null )
            m_dynamicMesh.Clear();

        // If we have just resumed
        if ( !pauseStatus ) {
            //If we have had Permissions granted already, we can reconnect.  Otherwise, the Start() code is still in control so we leave alone.
            if ( state.isPermitted() )
                m_tangoApplication.Startup( m_areaDescription );
        }
    }

    /// <summary>
    /// Detect one or more markers in the input image.
    /// </summary>
    /// <param name="cameraId">
    /// Returned camera ID.
    /// </param>
    /// <param name="imageBuffer">
    /// Color camera image buffer.
    /// </param>
    public void OnTangoImageAvailableEventHandler( TangoEnums.TangoCameraId cameraId,
        TangoUnityImageData imageBuffer ) {

        TangoCoordinateFramePair pair = new TangoCoordinateFramePair();
        pair.baseFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION;
        pair.targetFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_CAMERA_COLOR;


        TangoSupport.DetectMarkers( imageBuffer, cameraId,
            TangoSupport.MarkerType.ARTAG, MARKER_SIZE, m_markerList, pair );

        //For each marker seen by this Client
        for ( int i = 0; i < m_markerList.Count; ++i ) {
            TangoSupport.Marker marker = m_markerList[i];
            m_roomAugPlayerController.CmdSendMarkerUpdate( marker );

        }
    }

    /*
     * Functions to delete for production release.  Do not rely on these outside of debug/util stuff
     */

    //Either enable the mesh visibally, or make it occlude only.
    public void DebugSetOccludersVis( bool occluding ) {

        foreach ( OcclusionGameplayObject occluder in m_gameplayController.getOcclusionGameplayObjects() ) {

            if ( occluder.gameplayState == BaseGameplayObject.GameplayState.Started ) {
                Debug.Log( "Setting " + occluder.gameObject.name + " to Occluding: " + occluding );
                occluder.setOcclusion( occluding );
            } else {
                Debug.Log( "Ignoring Occlusion toggle " + occluding + " as " + occluder.name + " is " + occluder.gameplayState );
            }
        }
        //          Util.JLog("Setting Occluder: " + occluder.gameObject.name + " to " + occludingOnly);
        //          if (occludingOnly) {//Make it invisible but occluding
        //              RoomAugPlayerController p = GetComponent<RoomAugPlayerController>();
        //              //occluder.setOcclusion( true );
        //              //TODO this is doing the wrong thing
        //              //occluder.gameplayState = BaseGameplayObject.GameplayState.Inactive;


        //          } else { //Let's see it
        //              occluder.gameObject.SetActive(false);
        //              //occluder.gameplayState = BaseGameplayObject.GameplayState.Started;
        //              //occluder.setOcclusion( false );
        //          }

        //}
    }





    //Called by Canvas Checkbox to start mapping a new space, while Localised
    public void DebugEnable3DR( bool active ) {
        Util.JLog( " Setting 3D Reconstruction mode to " + active );

        m_dynamicMesh.Clear();
        m_dynamicMesh.enabled = active;
        //m_dynamicMeshRenderer.enabled = active;
        m_tangoApplication.Set3DReconstructionEnabled( active );
    }


    //Save current Mesh to file
    public void DebugExport3DRMesh() {

        Util.JLog( "Exporting 3DR Mesh" );

        m_dynamicMesh = FindObjectOfType<TangoDynamicMesh>();

        string filepath = "/sdcard/3DRMesh_" + m_areaDescriptionName + ".obj";
        if ( m_dynamicMesh != null ) {
            m_dynamicMesh.ExportMeshToObj( filepath );
            Util.JLog( "Exported mesh to " + filepath );
        } else {
            Util.JLogErr( "No Dynamic Mesh loaded to Export!" );
        }
    }


}