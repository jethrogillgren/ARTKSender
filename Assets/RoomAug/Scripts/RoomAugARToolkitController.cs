using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RoomAugARToolkitController : ARController
{
	bool enableVisual = false;

//	public void Start()
//	{
//		UseVideoBackground = false;
//
//	}



	public override bool StartAR()
	{
		if (enableVisual)
		{
			return base.StartAR ();
		}
		else
		{
			
			// Catch attempts to inadvertently call StartAR() twice.
			if (_running)
			{
				Log ( LogTag + "WARNING: StartAR() called while already running. Ignoring.\n" );
				return false;
			}

			Log ( LogTag + "Starting AR." );

			// Retrieve video configuration, and append any required per-platform overrides.
			// For native GL texturing we need monoplanar video; iOS and Android default to biplanar format. 
			string videoConfiguration0;
			string videoConfiguration1;
			switch ( Application.platform )
			{
				case RuntimePlatform.OSXEditor:
				case RuntimePlatform.OSXPlayer:
					videoConfiguration0 = videoConfigurationMacOSX0;
					videoConfiguration1 = videoConfigurationMacOSX1;
					if (_useNativeGLTexturing || !AllowNonRGBVideo)
					{
						if (videoConfiguration0.IndexOf ( "-device=QuickTime7" ) != -1 || videoConfiguration0.IndexOf ( "-device=QUICKTIME" ) != -1)
							videoConfiguration0 += " -pixelformat=BGRA";
						if (videoConfiguration1.IndexOf ( "-device=QuickTime7" ) != -1 || videoConfiguration1.IndexOf ( "-device=QUICKTIME" ) != -1)
							videoConfiguration1 += " -pixelformat=BGRA";
					}
					break;
				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.WindowsPlayer:
					videoConfiguration0 = videoConfigurationWindows0;
					videoConfiguration1 = videoConfigurationWindows1;
					if (_useNativeGLTexturing || !AllowNonRGBVideo)
					{
						if (videoConfiguration0.IndexOf ( "-device=WinMF" ) != -1)
							videoConfiguration0 += " -format=BGRA";
						if (videoConfiguration1.IndexOf ( "-device=WinMF" ) != -1)
							videoConfiguration1 += " -format=BGRA";
					}
					break;
				case RuntimePlatform.Android:
					videoConfiguration0 = videoConfigurationAndroid0 + " -cachedir=\"" + Application.temporaryCachePath + "\"" + ( _useNativeGLTexturing || !AllowNonRGBVideo ? " -format=RGBA" : "" );
					videoConfiguration1 = videoConfigurationAndroid1 + " -cachedir=\"" + Application.temporaryCachePath + "\"" + ( _useNativeGLTexturing || !AllowNonRGBVideo ? " -format=RGBA" : "" );
					break;
				case RuntimePlatform.IPhonePlayer:
					videoConfiguration0 = videoConfigurationiOS0 + ( _useNativeGLTexturing || !AllowNonRGBVideo ? " -format=BGRA" : "" );
					videoConfiguration1 = videoConfigurationiOS1 + ( _useNativeGLTexturing || !AllowNonRGBVideo ? " -format=BGRA" : "" );
					break;
				case RuntimePlatform.WSAPlayerX86:
				case RuntimePlatform.WSAPlayerX64:
				case RuntimePlatform.WSAPlayerARM:
					videoConfiguration0 = videoConfigurationWindowsStore0;
					videoConfiguration1 = videoConfigurationWindowsStore1;
					break;
			//case RuntimePlatform.LinuxEditor:
				case RuntimePlatform.LinuxPlayer:
					videoConfiguration0 = videoConfigurationLinux0;
					videoConfiguration1 = videoConfigurationLinux1;
					break;
				default:
					videoConfiguration0 = "";			
					videoConfiguration1 = "";			
					break;
			}	

			// Load the default camera parameters.
			TextAsset ta;
			byte [] cparam0 = null;
			byte [] cparam1 = null;
			byte [] transL2R = null;
			ta = Resources.Load ( "ardata/" + videoCParamName0, typeof ( TextAsset ) ) as TextAsset;
			if (ta == null)
			{		
				// Error - the camera_para.dat file isn't in the right place			
				Log ( LogTag + "StartAR(): Error: Camera parameters file not found at Resources/ardata/" + videoCParamName0 + ".bytes" );
				return ( false );
			}
			cparam0 = ta.bytes;
			if (VideoIsStereo)
			{
				ta = Resources.Load ( "ardata/" + videoCParamName1, typeof ( TextAsset ) ) as TextAsset;
				if (ta == null)
				{		
					// Error - the camera_para.dat file isn't in the right place			
					Log ( LogTag + "StartAR(): Error: Camera parameters file not found at Resources/ardata/" + videoCParamName1 + ".bytes" );
					return ( false );
				}
				cparam1 = ta.bytes;
				ta = Resources.Load ( "ardata/" + transL2RName, typeof ( TextAsset ) ) as TextAsset;
				if (ta == null)
				{		
					// Error - the transL2R.dat file isn't in the right place			
					Log ( LogTag + "StartAR(): Error: The stereo calibration file not found at Resources/ardata/" + transL2RName + ".bytes" );
					return ( false );
				}
				transL2R = ta.bytes;
			}

			// Begin video capture and marker detection.
			if (!VideoIsStereo)
			{
				Log ( LogTag + "Starting ARToolKit video with vconf '" + videoConfiguration0 + "'." );
				//_running = PluginFunctions.arwStartRunning(videoConfiguration, cparaName, nearPlane, farPlane);
				_running = PluginFunctions.arwStartRunningB ( videoConfiguration0, cparam0, cparam0.Length, NearPlane, FarPlane );
			}
			else
			{
				Log ( LogTag + "Starting ARToolKit video with vconfL '" + videoConfiguration0 + "', vconfR '" + videoConfiguration1 + "'." );
				//_running = PluginFunctions.arwStartRunningStereo(vconfL, cparaNameL, vconfR, cparaNameR, transL2RName, nearPlane, farPlane);
				_running = PluginFunctions.arwStartRunningStereoB ( videoConfiguration0, cparam0, cparam0.Length, videoConfiguration1, cparam1, cparam1.Length, transL2R, transL2R.Length, NearPlane, FarPlane );

			}

			if (!_running)
			{
				Log ( LogTag + "Error starting running" );
				ARW_ERROR error = ( ARW_ERROR )PluginFunctions.arwGetError ();
				if (error == ARW_ERROR.ARW_ERROR_DEVICE_UNAVAILABLE)
				{
					showGUIErrorDialogContent = "Unable to start AR tracking. The camera may be in use by another application.";
				}
				else
				{
					showGUIErrorDialogContent = "Unable to start AR tracking. Please check that you have a camera connected.";
				}
				showGUIErrorDialog = true;
				return false;
			}

			// After calling arwStartRunningB/arwStartRunningStereoB, set ARToolKit configuration.
			Log ( LogTag + "Setting ARToolKit tracking settings." );
			VideoThreshold = currentThreshold;
			VideoThresholdMode = currentThresholdMode;
			LabelingMode = currentLabelingMode;
			BorderSize = currentBorderSize;
			PatternDetectionMode = currentPatternDetectionMode;
			MatrixCodeType = currentMatrixCodeType;
			ImageProcMode = currentImageProcMode;
			NFTMultiMode = currentNFTMultiMode;

			// Remaining Unity setup happens in UpdateAR().
			return true;
		}
	}



	protected override bool UpdateAR()
	{
		if (enableVisual)
		{
			return base.UpdateAR ();
		}
		else
		{
			PluginFunctions.arwCapture();
			bool ok = PluginFunctions.arwUpdateAR();
			if (!ok) return false;

			return true;
		}
	}


	protected override void Update()
	{
		//Log(LogTag + "ARController.Update()");

		if (Application.isPlaying) {

			/*/// jgillgr the only change to this overload is commenting these key handlers out.
			if (Input.GetKeyDown(KeyCode.Menu) || Input.GetKeyDown(KeyCode.Return)) showGUIDebug = !showGUIDebug;
			if (QuitOnEscOrBack && Input.GetKeyDown(KeyCode.Escape)) Application.Quit(); // On Android, maps to "back" button.*/
			CalculateFPS();

			UpdateAR();

		} else {

			// Editor update.

		}
	}


	public override void OnEnable()
	{
		UseVideoBackground = enableVisual;
		showGUIDebug = false;
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