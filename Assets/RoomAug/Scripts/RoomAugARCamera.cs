using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomAugARCamera : ARCamera {

	// Use this for initialization
	void Start () {
		LogTag = "RoomAugARCamera";
	}

	public override bool SetupCamera(float nearClipPlane, float farClipPlane, Matrix4x4 projectionMatrix, ref bool opticalOut)
	{
		bool bb = base.SetupCamera ( nearClipPlane, farClipPlane, projectionMatrix, ref opticalOut );

		if (bb)
		{
			Camera c = this.gameObject.GetComponent<Camera> ();
			c.rect = new Rect ( Util.ARToolkitViewportRectX, Util.ARToolkitViewportRectY, Util.ARToolkitViewportRectW, Util.ARToolkitViewportRectH );
		}

		return bb;
	}

}