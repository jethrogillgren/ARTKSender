using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tango;

//Controlls the Tango lifecycle.  Local to the player object.  All networking happens through PlayerController
public class TangoPlayerApplicationController : MonoBehaviour, ITangoLifecycle {
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




	// Use this for initialization
	void Start () {
		Util.JLog(" Application Starting Up");
		state = new ApplicationStateMachine ();

		m_tangoApplication = FindObjectOfType<TangoApplication>();
		m_gameplayController = FindObjectOfType<GameplayController>();
		m_poseController = FindObjectOfType<TangoARPoseController>();

		if (m_tangoApplication != null)
		{
			m_tangoApplication.Register(this);
			m_tangoApplication.RequestPermissions();
		}

//		On3DRToggle (false);
//		OnMeshViewToggle (false);
	}
	
	// Update is called once per frame
	void Update()
	{
		if ( !state.isTangoConnected() ) {
			return;
		}


	}

	public void OnGUI() {

		//Toggle Relocatilisation Graphic depending on state
		if( state.CurrentState == ProcessState.Localised ) {
			m_relocalizationOverlay.SetActive (false);
		} else if ( state.CurrentState == ProcessState.Active ) {
			m_relocalizationOverlay.SetActive (true);
		}
	}




	public void OnTangoPermissions(bool permissionsGranted)
	{
		if (permissionsGranted)
		{
			state.setPermitted();
			AreaDescription[] list = AreaDescription.GetList();

			Util.JLog(" There are " + (list == null ? 0 : list.Length ) + " Area Descriptions Available: ");

			if (list != null && list.Length != 0) {

				// Find our area Description.  They are by default enabled on tango.
				foreach (AreaDescription areaDescription in list) {
					AreaDescription.Metadata metadata = areaDescription.GetMetadata ();

					if (metadata.m_name == m_areaDescriptionName) {
						m_areaDescription = areaDescription;

						m_tangoApplication.Startup (m_areaDescription);
						return;
					}
				}

				if (m_areaDescription == null) {
					Util.JLogErr("No AreaDescription Found matching " + m_areaDescriptionName + "   Starting without one");
					m_tangoApplication.Startup (null);
					state.MoveNext (Command.Localise);
				}

			} else {
				// No Area Descriptions available.
				Util.JLogErr("No area descriptions available.  Starting without one in Drift corrected Motion tracking mode.");
				m_tangoApplication.EnableAreaDescriptions = false;
				m_tangoApplication.EnableMotionTracking = true;
				m_tangoApplication.EnableDriftCorrection = true;

				m_tangoApplication.Startup(null);
				state.MoveNext (Command.Localise);
			}
		}
		else
		{
			AndroidHelper.ShowAndroidToastMessage("Motion Tracking and Area Learning Permissions Needed");
			state.MoveNext (Command.Exit);
			Application.Quit();
		}
	}

	public void OnTangoServiceConnected()
	{
		Util.JLog(" Tango Service Connected");
		state.MoveNext (Command.Connect);

	}

	public void OnTangoServiceDisconnected()
	{
		Util.JLog(" Tango Service Disconnected");
		state.MoveNext (Command.Disconnect);
		//TODO Lifecycle reconnect
	}

	public void OnApplicationPause(bool pauseStatus)
	{

		// If we have just resumed
		if ( !pauseStatus ) {
			//If we have had Permissions granted already, we can reconnect.  Otherwise, the Start() code is still in control so we leave alone.
			if( state.isPermitted() )
				m_tangoApplication.Startup (m_areaDescription);
		}
	}


	public void debugSetOccludersVis(bool newVal) {
		foreach (OcclusionGameplayObject occluder in m_gameplayController.getOcclusionGameplayObjects()) {
			occluder.setOcclusion (!newVal);
		}
	}
}
