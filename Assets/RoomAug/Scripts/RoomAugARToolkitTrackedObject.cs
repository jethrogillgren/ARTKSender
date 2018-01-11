using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class RoomAugARToolkitTrackedObject : ARTrackedObject {

	public RoomAugNetworkController networkController;

	// Use this for initialization
	protected override void Start () {
		LogTag = "RoomAugARToolkitTrackedObject: ";
		networkController = FindObjectOfType<RoomAugNetworkController> ();
	}

	//Overridden version strips all unnessecary (for us) draws
	protected override void LateUpdate()
	{
		// Update tracking if we are running in the Player.
		if (Application.isPlaying) {

			// Sanity check, make sure we have an ARMarker assigned.
			ARMarker marker = GetMarker();
			if (marker == null) {
				//visible = visibleOrRemain = false;
			} else {

				// Note the current time
				float timeNow = Time.realtimeSinceStartup;

				if (marker.Visible) {

					if (!visible) {
						// Marker was hidden but now is visible.
						visible = true;
//						if (eventReceiver != null) eventReceiver.BroadcastMessage("OnMarkerFound", marker, SendMessageOptions.DontRequireReceiver);

					}

					//Marker Update available - send it to the Server
//					if (eventReceiver != null) eventReceiver.BroadcastMessage("OnMarkerTracked", marker, SendMessageOptions.DontRequireReceiver);
					if( networkController.ARToolkit_UdpClient != null )
					{
						float[] floatArray = new float[] {
							marker.TransformationMatrix.m00,
							marker.TransformationMatrix.m01, 
							marker.TransformationMatrix.m02,
							marker.TransformationMatrix.m03,
							marker.TransformationMatrix.m10,
							marker.TransformationMatrix.m11,
							marker.TransformationMatrix.m12,
							marker.TransformationMatrix.m13,
							marker.TransformationMatrix.m20,
							marker.TransformationMatrix.m21,
							marker.TransformationMatrix.m22,
							marker.TransformationMatrix.m23,
							marker.TransformationMatrix.m30,
							marker.TransformationMatrix.m31,
							marker.TransformationMatrix.m32,
							marker.TransformationMatrix.m33,
						};
						byte[] encodedTag = Encoding.ASCII.GetBytes(marker.Tag);

						// create a byte array and copy the floats into it...
						var byteArray = new byte[(floatArray.Length *4) + encodedTag.Length ];
						System.Buffer.BlockCopy(floatArray, 0, byteArray, 0, floatArray.Length * 4);

						//Then append the encoded Tag to the end.
						encodedTag.CopyTo ( byteArray, (floatArray.Length *4) );

						Debug.LogError ("Sending an AR Update from CamID " + networkController.ARToolkit_CamID + " for tag: " + marker.Tag);
						networkController.ARToolkit_UdpClient.Send ( byteArray, byteArray.Length );
					}


				} else {
					
					if ( timeNow - timeTrackingLost >= secondsToRemainVisible) {
//						if (eventReceiver != null) eventReceiver.BroadcastMessage("OnMarkerLost", marker, SendMessageOptions.DontRequireReceiver);
					}
				}
			} // marker

		} // Application.isPlaying

	}
}
