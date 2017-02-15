using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using System.Xml;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//using ADFMeshUtil;

using Tango;

public class ApplicationController : MonoBehaviour, ITangoLifecycle, ITangoEvent, ITangoPose, ITangoDepth
{
	public string m_areaDescriptionName = "JethroTestAreaDescription";
	public GameObject m_areaMeshPrefab;
	public GameObject m_relocalizationOverlay;
	public GameObject m_buttonsPanel;
	public GameObject m_exportButton;
	public GameObject m_prefabMarker;
	public TangoPointCloud m_pointCloud;
	public RectTransform m_prefabTouchEffect;	//The touch effect to place on taps.
	public Canvas m_canvas; // The canvas to place 2D game objects under.

	[Header("Materials")]
	public Material m_depthMaskMat;// The reference to the depth mask material to be applied to occlusion meshes.
	public Material m_visibleMat;// The reference to the visible material applied to the mesh.

	private ApplicationStateMachine state;
	private GameplayController m_gameplayController;
//	private ADFMeshUtil adfMeshUtil;
	private GameObject m_areaMesh;
	private GameObject m_meshFromFile;	// The loaded mesh reconstructed from the serialized AreaDescriptionMesh file.
	private TangoApplication m_tangoApplication;
	private TangoARPoseController m_poseController;
	private TangoDynamicMesh m_dynamicMesh;
	private bool m_modeMarkPoint = false;
	private AreaDescription m_areaDescription = null;
	private bool m_findPlaneWaitingForDepth;// If set, then the depth camera is on and we are waiting for the next depth update.

//	private OcclusionGameplayObject tmp;

	public void Start()
	{
		JLog(" Application Starting Up");
		state = new ApplicationStateMachine ();



		m_poseController = FindObjectOfType<TangoARPoseController>();
		m_tangoApplication = FindObjectOfType<TangoApplication>();
		m_dynamicMesh = FindObjectOfType<TangoDynamicMesh>();
		m_gameplayController = FindObjectOfType<GameplayController>();

//		adfMeshUtil = new ADFMeshUtil ();


//		instantiateAreaMeshPrefab ();

		if (m_tangoApplication != null)
		{
			m_tangoApplication.Register(this);
			m_tangoApplication.RequestPermissions();
		}

		On3DRToggle (false);
		OnMeshViewToggle (false);
	}

	public void Awake() {
		//JLog("AWAKE");
	}

	public void OnTangoPermissions(bool permissionsGranted)
	{
		if (permissionsGranted)
		{
			state.setPermitted();
			AreaDescription[] list = AreaDescription.GetList();

			JLog(" There are " + list.Length + " Area Descriptions Available: ");

			if (list != null && list.Length != 0) {
				
				// Find our area Description
				foreach (AreaDescription areaDescription in list) {
					AreaDescription.Metadata metadata = areaDescription.GetMetadata ();

					if (metadata.m_name == m_areaDescriptionName) {
						m_areaDescription = areaDescription;

						m_tangoApplication.Startup (m_areaDescription);
						return;
					}
				}

				if (m_areaDescription == null) {
					JLogErr("J# No AreaDescription Found matching " + m_areaDescriptionName + "   Starting without one");
					m_tangoApplication.Startup (null);
				}

			}
			else
			{
				// No Area Descriptions available.
				JLogErr("J# No area descriptions available.  Starting without one");
				m_tangoApplication.Startup(null);
				state.MoveNext (Command.Connect);
			}
		}
		else
		{
			AndroidHelper.ShowAndroidToastMessage("Motion Tracking and Area Learning Permissions Needed");
			Application.Quit();
		}
	}

	private void onFirstLocalisation() {
		JLog ("There are no Initial Localisation Actions");
		//instantiateAreaMeshPrefab ();

	}

	private void instantiateAreaMeshPrefab() {
		if (m_areaMeshPrefab != null) {
			m_areaMesh = Instantiate (m_areaMeshPrefab);
			m_areaMesh.name = m_areaMeshPrefab.name; //defaults to name + "(Clone)"
			MeshRenderer mr = m_areaMesh.AddComponent<MeshRenderer> ();
			m_areaMesh.AddComponent<MeshCollider>();

			foreach( MeshRenderer m in m_areaMesh.GetComponentsInChildren<MeshRenderer>() ){
				m.material = m_depthMaskMat;
				m.gameObject.layer = LayerMask.NameToLayer("Occlusion");
			}
			m_areaMesh.transform.Rotate (new Vector3 (0, 180, 0));
			mr.material = m_depthMaskMat;
			m_areaMesh.layer = LayerMask.NameToLayer("Occlusion");
			m_areaMesh.GetComponent<MeshRenderer>().material = m_depthMaskMat;
			JLog ("Created the Area Mesh Programatically OK:  " + m_areaMesh.name);

			OcclusionGameplayObject tmp = m_areaMesh.AddComponent ( typeof(OcclusionGameplayObject) ) as OcclusionGameplayObject ;
			tmp.m_visibleMat = m_visibleMat;
			tmp.m_depthMaskMat = m_depthMaskMat;
			tmp.m_GameplayState = OcclusionGameplayObject.GameplayState.Started;
			tmp.m_IsDecorationOnly = true;
			m_gameplayController.addGameplayObject (tmp);
			tmp.setOcclusion (true);	
			
			JLog ("Created the Area Mesh Programatically OK:  " + m_areaMesh.name + "  as an occluder: " + m_areaMesh.GetComponent<OcclusionGameplayObject>().m_GameplayState );
		}

	}


//	private void createAdfMeshGameobject(ADFMeshUtil.AreaDescriptionMesh mesh) {
//		// Create GameObject container with mesh components for the loaded mesh.
//		m_meshFromFile = new GameObject();
//
//		MeshFilter mf = m_meshFromFile.AddComponent<MeshFilter>();
//		mf.mesh = adfMeshUtil._AreaDescriptionMeshToUnityMesh(mesh);
//
//		MeshRenderer mr = m_meshFromFile.AddComponent<MeshRenderer>();
//		mr.material = m_depthMaskMat;
//
//		m_meshFromFile.AddComponent<MeshCollider>();
//		m_meshFromFile.layer = LayerMask.NameToLayer("Occlusion");
//		JLog ("Created ADF Mesh GameObject at " + m_meshFromFile.transform.position );
//	}

	/// <summary>
	/// Unity Update function.
	/// </summary>
	public void Update()
	{
		if ( !state.isTangoConnected() )
		{
			return;
		}

		//If one finger is touching screen
		if (m_modeMarkPoint && Input.touchCount == 1)
		{
			Touch t = Input.GetTouch(0);
			Vector2 guiPositionPosYDown = new Vector2(t.position.x, Screen.height - t.position.y);
			Vector2 guiPositionPosYUp = new Vector2(t.position.x, t.position.y);

			Camera cam = Camera.main;
//			RaycastHit hitInfo;

			if (t.phase != TouchPhase.Began)
			{
				return;
			}

			if ( RectTransformUtility.RectangleContainsScreenPoint(m_buttonsPanel.GetComponent<RectTransform>(), guiPositionPosYUp) )
			{
				// do nothing, the button will handle it
				JLog ("Update step Ignoring Click over controlPanel");
				return;
			}
//			else if (Physics.Raycast(cam.ScreenPointToRay(t.position), out hitInfo))
//			{
//				// TODO Check if we have clicked an existing marker, using a RayCast onto a collision group
//			}
			else
			{
				// Place a new point at that location, clear selection
				JLog("  Click!   At " + guiPositionPosYDown );
				StartCoroutine(_WaitForDepthAndFindPlane(t.position));

				// Because we may wait a small amount of time, this is a good place to play a small
				// animation so the user knows that their input was received.
				RectTransform touchEffectRectTransform = Instantiate(m_prefabTouchEffect) as RectTransform;
				touchEffectRectTransform.transform.SetParent(m_canvas.transform, false);
				Vector2 normalizedPosition = t.position;
				normalizedPosition.x /= Screen.width;
				normalizedPosition.y /= Screen.height;
				touchEffectRectTransform.anchorMin = touchEffectRectTransform.anchorMax = normalizedPosition;
			}
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



	/// <summary>
	/// This is called each time a Tango event happens.
	/// </summary>
	/// <param name="tangoEvent">Tango event.</param>
	public void OnTangoEventAvailableEventHandler(Tango.TangoEvent tangoEvent)
	{
		if( 		tangoEvent.type  ==  TangoEnums.TangoEventType.TANGO_EVENT_COLOR_CAMERA ) {
			//Ignore
		} else if ( tangoEvent.type  ==  TangoEnums.TangoEventType.TANGO_EVENT_FISHEYE_CAMERA ) {
			//Ignore
		} else if ( tangoEvent.type == TangoEnums.TangoEventType.TANGO_EVENT_FEATURE_TRACKING
			&& tangoEvent.event_key == "TooFewFeaturesTracked") {
			//Ignore
		} else {
			JLog(" Recieved Tango Event: " + tangoEvent.type + " " + tangoEvent.event_key );
		}
	}
		

	/// <summary>
	/// OnTangoPoseAvailable is called from Tango when a new Pose is available.
	/// </summary>
	/// <param name="pose">The new Tango pose.</param>
	public void OnTangoPoseAvailable(TangoPoseData pose)
	{

		if (pose.framePair.baseFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION
			&& pose.framePair.targetFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE)
		{
			if (pose.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID  &&  state.CurrentState == ProcessState.Active)
			{
				JLog("Relocalised");
				if (!state.hasFirstLocalisedHappened ())
					onFirstLocalisation ();
				state.setFirstLocalised();

				state.MoveNext (Command.Localise);
			}
			else if ( pose.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_INVALID  &&  state.CurrentState == ProcessState.Localised )
			{
				JLogErr("Lost Localisation");
				state.MoveNext (Command.Unlocalise);
//			} else {
//				JLog( "J#  Unhandled Tango Pose Event: " + pose.status_code.ToString() );
			}
		}
	}

	/// <summary>
	/// Wait for the next depth update, then find the plane at the touch position.
	/// </summary>
	/// <returns>Coroutine IEnumerator.</returns>
	/// <param name="touchPosition">Touch position to find a plane at.</param>
	private IEnumerator _WaitForDepthAndFindPlane(Vector2 touchPosition)
	{
		m_findPlaneWaitingForDepth = true;

		JLog ("Waiting for Depth...");
		// Turn on the camera and wait for a single depth update.
		m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.MAXIMUM);
		while (m_findPlaneWaitingForDepth)
		{
			yield return null;
		}
		JLog ("Got Depth");
		m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.DISABLED);

		// Find the plane.
		Camera cam = Camera.main;
		Vector3 planeCenter;
		Plane plane;
		if (!m_pointCloud.FindPlane(cam, touchPosition, out planeCenter, out plane))
		{
			yield break;
		}

		// Ensure the location is always facing the camera.  This is like a LookRotation, but for the Y axis.
		Vector3 up = plane.normal;
		Vector3 forward;
		if (Vector3.Angle(plane.normal, cam.transform.forward) < 175)
		{
			Vector3 right = Vector3.Cross(up, cam.transform.forward).normalized;
			forward = Vector3.Cross(right, up).normalized;
		}
		else
		{
			// Normal is nearly parallel to camera look direction, the cross product would have too much
			// floating point error in it.
			forward = Vector3.Cross(up, cam.transform.right);
		}

		JLog ("Created Marker at " + planeCenter);
		
		GameObject newMarkObject = Instantiate(m_prefabMarker, planeCenter, Quaternion.LookRotation(forward, up));

		TangoARMarker markerScript = newMarkObject.GetComponent<TangoARMarker>();

//		markerScript.m_type = 0;
		markerScript.m_timestamp = (float)m_poseController.m_poseTimestamp;

		Matrix4x4 uwTDevice = Matrix4x4.TRS(m_poseController.m_tangoPosition,
			m_poseController.m_tangoRotation,
			Vector3.one);
		Matrix4x4 uwTMarker = Matrix4x4.TRS(newMarkObject.transform.position,
			newMarkObject.transform.rotation,
			Vector3.one);
		markerScript.m_deviceTMarker = Matrix4x4.Inverse(uwTDevice) * uwTMarker;


		JLog ( JsonUtility.ToJson(newMarkObject.transform.position) );
	}

	/// <summary>
	/// This is called each time new depth data is available.
	/// 
	/// On the Tango tablet, the depth callback occurs at 5 Hz.
	/// </summary>
	/// <param name="tangoDepth">Tango depth.</param>
	public void OnTangoDepthAvailable(TangoUnityDepth tangoDepth)
	{
		// Don't handle depth here because the PointCloud may not have been updated yet.  Just
		// tell the coroutine it can continue.
		m_findPlaneWaitingForDepth = false;
	}

	public void OnTangoServiceConnected()
	{
		JLog(" Tango Service Connected");
		state.MoveNext (Command.Connect);

	}

	public void OnTangoServiceDisconnected()
	{
		JLog(" Tango Service Disconnected");
		state.MoveNext (Command.Disconnect);
		//reloadApp ();
		//		if (m_initialized) {
		//			// When application is backgrounded, we reload the level because the Tango Service is disconected. All
		//			// learned area and placed marker should be discarded as they are not saved.
		//			reloadApp ();
		//		}

		//m_initialized = false;

	}

	/// <summary>
	/// Application onPause / onResume callback.
	/// </summary>
	/// <param name="pauseStatus"><c>true</c> if the application about to pause, otherwise <c>false</c>.</param>
	public void OnApplicationPause(bool pauseStatus)
	{

		if(m_dynamicMesh != null)
			m_dynamicMesh.Clear();

		// If we have just resumed
		if ( !pauseStatus ) {
			//If we have had Permissions granted already, we can reconnect.  Otherwise, the Start() code is still in control so we leave alone.
			if( state.isPermitted() )
				m_tangoApplication.Startup (m_areaDescription);
		}
		/*if (pauseStatus)
		{
			// When application is backgrounded, we reload the level because the Tango Service is disconected. All
			// learned area and placed marker should be discarded as they are not saved.
			state.MoveNext (Command.Exit);
			reloadApp ();
		} else {
			JLog("Not acting on a OnApplicationPause callback");
		}*/
	}



	private void reloadApp() {
		#pragma warning disable 618
		JLog("Application is being Backgrounded, Reloading the scene.");
		Application.LoadLevel(Application.loadedLevel);
		#pragma warning restore 618
	}

	//Debug something to the screen
	public void OnButtonShoutClick()
	{
//		string ret = " Current Pose Position.  World: " + Camera.main.transform.position + "   local:" + Camera.main.transform.localPosition;
		StringBuilder builder = new StringBuilder ();

		if (m_gameplayController != null) {
			builder.AppendLine (m_gameplayController.m_gameplayObjects.Count + " Gameplay Objects:");
			foreach (BaseGameplayObject g in m_gameplayController.getGameplayObjectsByState (BaseGameplayObject.GameplayState.Started)) { // Loop through all strings
				builder.AppendLine (g.name);
			}
		} else {
			JLogErr ("BADTING");
		}

//		string ret = string.Join(",", m_gameplayController.getGameplayObjectsByState (BaseGameplayObject.GameplayState.Started) );
		string ret = builder.ToString ();

		JLog( ret );
		AndroidHelper.ShowAndroidToastMessage ( ret );
	}

	//Save current Mesh to file
	public void OnButtonExportMeshClick()
	{
		string filepath = Application.persistentDataPath + "/meshes/3DRMesh_" + m_areaDescriptionName + ".obj";
		if (m_dynamicMesh != null) {
			m_dynamicMesh.ExportMeshToObj (filepath);
			JLog ("Exported mesh to " + filepath);
		} else {
			JLogErr ("No Dynamic Mesh loaded to Export!");
		}
	}

	//Save current Mesh to file
	public void OnButtonExportMarkedPointsClick()
	{
//		string filepath = Application.persistentDataPath + "/meshes/Marks_" + m_areaDescriptionName + ".obj";

	}

	//Called by Canvas Checkbox
	public void OnMarkPointToggle(bool newVal)
	{
		JLog(" Setting MarkPoint mode to " + newVal);
		m_modeMarkPoint = newVal;
	}

	//Called by Canvas Checkbox
	public void On3DRToggle(bool newVal)
	{
		JLog(" Setting 3D Reconstruction mode to " + newVal);
		m_dynamicMesh.enabled = newVal;
		m_tangoApplication.Set3DReconstructionEnabled(newVal);
		m_exportButton.GetComponent<Button>().interactable = newVal;
	}

	//Called by Canvas Checkbox
	public void OnMeshViewToggle(bool newVal)
	{
		foreach (OcclusionGameplayObject occluder in m_gameplayController.getOcclusionGameplayObjects() ){
			occluder.setOcclusion (!newVal);
			JLog ("Set GameplayObject " + occluder.gameObject.name + " to occlusion=" + !newVal);
		}

	}


	private void JLog(string val) {
		Debug.Log ("J# " + val);
	}
	private void JLogErr(string val) {
		Debug.LogError ("J# " + val);
	}
}