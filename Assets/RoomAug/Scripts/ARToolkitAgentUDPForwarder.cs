using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ARToolkitAgentUDPForwarder : MonoBehaviour {

	public RoomAugNetworkController networkController;

	// Use this for initialization
	protected void Start () {
		networkController = FindObjectOfType<RoomAugNetworkController> ();
	}

	public void OnMarkerTracked(ARMarker marker)
	{

        ARUtilityFunctions.PositionFromMatrix(marker.TransformationMatrix);


        //THe UDP CLient is made once we have a connection to the main server
        if ( marker && networkController.ARToolkit_UdpClient != null )
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

			Debug.Log("Sending an AR Update from CamID " + networkController.ARToolkit_CamID + " for tag: " + marker.Tag);
            Debug.Log("Transform     pos: x " + marker.TransformationMatrix.GetColumn(3).x + "   y " + marker.TransformationMatrix.GetColumn(3).y + "  z" + marker.TransformationMatrix.GetColumn(3).z + "   rot: " + Util.QuaternionFromMatrix(marker.TransformationMatrix).eulerAngles);

            networkController.ARToolkit_UdpClient.Send ( byteArray, byteArray.Length );
        }
	}
}
