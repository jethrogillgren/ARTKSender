using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RoomAugARToolkitController : ARController
{


//	public void Start()
//	{
//		UseVideoBackground = false;
//
//	}


	public override void OnEnable()
	{
//		UseVideoBackground = false;
		LogTag = "RoomAugARController";

		base.OnEnable ();
	}

	public override Rect getViewport(int contentWidth, int contentHeight, bool stereo, ARCamera.ViewEye viewEye)
	{
		base.getViewport (contentWidth, contentHeight, stereo, viewEye);

//		Debug.LogError(LogTag + "Overrode viewport " + Util.ARToolkitViewportRectW + "x" + Util.ARToolkitViewportRectH + " at (" + Util.ARToolkitViewportRectX + ", " + Util.ARToolkitViewportRectY + ").");
		return new Rect(Util.ARToolkitViewportRectX, Util.ARToolkitViewportRectY,
				Util.ARToolkitViewportRectW, Util.ARToolkitViewportRectH);
	}

	public override bool CreateClearCamera()
	{
		bool bb = base.CreateClearCamera ();

		if (bb)
		{
			clearCamera = this.gameObject.GetComponent<Camera> ();
			clearCamera.rect = new Rect ( Util.ARToolkitViewportRectX, Util.ARToolkitViewportRectY,
				Util.ARToolkitViewportRectW, Util.ARToolkitViewportRectH );
		}

		return bb;

//		return false;

	}


//	protected virtual GameObject CreateVideoBackgroundCamera(String name, int layer, out Camera vbc)
//	{
//		GameObject vbcgo = base.CreateVideoBackgroundCamera (name, layer, out vbc);
//		vbc = vbcgo.GetComponent<Camera>();
//		vbc.rect = new Rect(viewportRectX, viewportRectY, viewportRectW, viewportRectH);
//		return vbcgo;
//	}
}