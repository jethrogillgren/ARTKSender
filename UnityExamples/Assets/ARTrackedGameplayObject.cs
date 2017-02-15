using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
[ExecuteInEditMode]
public class ARTrackedGameplayObject : BaseGameplayObject {

	private AROrigin _origin = null;
	private ARMarker _marker = null;

	private bool visible = false;					// Current visibility from tracking
	private float timeTrackingLost = 0;				// Time when tracking was last lost
	public float secondsToRemainVisible = 0.0f;	// How long to remain visible after tracking is lost (to reduce flicker)
	private bool visibleOrRemain = false;			// Whether to show the content (based on above variables)

	public GameObject eventReceiver;

	// Private fields with accessors.
	[SerializeField]
	private string _markerTag = "";					// Unique tag for the marker to get tracking from


	public string MarkerTag
	{
		get
		{
			return _markerTag;
		}

		set
		{
			_markerTag = value;
			_marker = null;
		}
	}

	// Return the marker associated with this component.
	// Uses cached value if available, otherwise performs a find operation.
	public virtual ARMarker GetMarker()
	{
		if (_marker == null) {
			// Locate the marker identified by the tag
			ARMarker[] ms = FindObjectsOfType<ARMarker>();
			foreach (ARMarker m in ms) {
				if (m.Tag == _markerTag) {
					_marker = m;
					break;
				}
			}
		}
		return _marker;
	}

	// Return the origin associated with this component.
	// Uses cached value if available, otherwise performs a find operation.
	public virtual AROrigin GetOrigin()
	{
		if (_origin == null) {
			// Locate the origin in parent.
			_origin = this.gameObject.GetComponentInParent<AROrigin>(); // Unity v4.5 and later.
		}
		return _origin;
	}


	void Start()
	{
		JLog( "Start()" );
		secondsToRemainVisible = 0.0f;

		if (Application.isPlaying) {
			// In Player, set initial visibility to not visible.
			for (int i = 0; i < this.transform.childCount; i++) this.transform.GetChild(i).gameObject.SetActive(false);
		} else {
			// In Editor, set initial visibility to visible.
			for (int i = 0; i < this.transform.childCount; i++) this.transform.GetChild(i).gameObject.SetActive(true);
		}
	}

	// Use LateUpdate to be sure the ARMarker has updated before we try and use the transformation.
	void LateUpdate()
	{
		// Local scale is always 1 for now
		transform.localScale = Vector3.one;

		// Update tracking if we are running in the Player.
		if (Application.isPlaying) {

			// Sanity check, make sure we have an AROrigin in parent hierachy.
			AROrigin origin = GetOrigin();
			if (origin == null) {
				JLog("No Origin");
				//visible = visibleOrRemain = false;

			} else {

				// Sanity check, make sure we have an ARMarker assigned.
				ARMarker marker = GetMarker();
				if (marker == null) {
					JLog( "No ARMarker");
					//visible = visibleOrRemain = false;
				} else {

					// Note the current time
					float timeNow = Time.realtimeSinceStartup;

					ARMarker baseMarker = origin.GetBaseMarker();
					if (baseMarker != null && marker.Visible) {

						if (!visible) {
							// Marker was hidden but now is visible.
							JLog( "Marker was hidden but now is visible.");
							visible = visibleOrRemain = true;
							if (eventReceiver != null) eventReceiver.BroadcastMessage("OnMarkerFound", marker, SendMessageOptions.DontRequireReceiver);

							for (int i = 0; i < this.transform.childCount; i++) this.transform.GetChild(i).gameObject.SetActive(true);
						} else {
							//							JLog( "Marker stayed visible");
						}

						Matrix4x4 pose;

//						if (marker == baseMarker) {
//							// If this marker is the base, no need to take base inverse etc.
//							pose = origin.transform.localToWorldMatrix;
//						} else {
							pose = (origin.transform.localToWorldMatrix * baseMarker.TransformationMatrix.inverse * marker.TransformationMatrix);
//						}

						transform.position = ( ARUtilityFunctions.PositionFromMatrix(pose) + Camera.main.transform.position );

//						Quaternion localisedRotation = Quaternion.Eu (ARUtilityFunctions.QuaternionFromMatrix (pose), Camera.main.transform.rotation);
						transform.rotation = ARUtilityFunctions.QuaternionFromMatrix (pose);

	//					Vector3 worldPos = ARUtilityFunctions.PositionFromMatrix (marker.TransformationMatrix) + Camera.main.transform.position;
	//					Quaternion worldRot = ARUtilityFunctions.QuaternionFromMatrix (marker.TransformationMatrix);
						JLog ("- marker.TranformationMatrix:  " + marker.TransformationMatrix );
						JLog ("- marker.TranformationMatrix as position transform " + ARUtilityFunctions.PositionFromMatrix(marker.TransformationMatrix) );

						JLog ("***  The Camera is at: " + Camera.main.transform.position + "   The marker is at " + transform.position + "   The camera is looking at " + Camera.main.transform.forward );

						if (eventReceiver != null) eventReceiver.BroadcastMessage("OnMarkerTracked", marker, SendMessageOptions.DontRequireReceiver);

					} else {
						if (visible) {
							// Marker was visible but now is hidden.
							JLog( "Marker was visible but now is hidden. (after " + secondsToRemainVisible + "s)");
							visible = false;
							timeTrackingLost = timeNow;
						} else {
							//							ARControllertroller.Log (LogTag + "Marker stayed hidden.");
						}

						if (visibleOrRemain && (timeNow - timeTrackingLost >= secondsToRemainVisible)) {
							visibleOrRemain = false;
							if (eventReceiver != null) eventReceiver.BroadcastMessage("OnMarkerLost", marker, SendMessageOptions.DontRequireReceiver);
							for (int i = 0; i < this.transform.childCount; i++) this.transform.GetChild(i).gameObject.SetActive(false);
						}
					}
				} // marker
			}//Origin

		}  // Application.isPlaying
		else {
			JLog( "Applicaiton Not Playing");
		}

	}
}
