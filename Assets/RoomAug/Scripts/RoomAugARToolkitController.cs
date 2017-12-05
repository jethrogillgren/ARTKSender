using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RoomAugARToolkitController : ARController
{

	public bool showVideoFeed = false;
	public Vector3 farfaraway = new Vector3(0,1000,0);


	public override bool StartAR()
	{

		// Catch attempts to inadvertently call StartAR() twice.
		if (_running)
		{
			Log ( LogTag + "WARNING: StartAR() called while already running. Ignoring.\n" );
			return false;
		}

		Log ( LogTag + "Starting AR." );

		_sceneConfiguredForVideo = _sceneConfiguredForVideoWaitingMessageLogged = false;


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




	protected override bool UpdateAR()
	{
		if (!_running) {
			return false;
		}


		if (showVideoFeed && !_sceneConfiguredForVideo)
		{

			// Wait for the wrapper to confirm video frames have arrived before configuring our video-dependent stuff.
			if (!PluginFunctions.arwIsRunning ())
			{
				if (!_sceneConfiguredForVideoWaitingMessageLogged)
				{
					Log ( LogTag + "UpdateAR: Waiting for ARToolKit video." );
					_sceneConfiguredForVideoWaitingMessageLogged = true;
				}
			}
			else
			{
				Log ( LogTag + "UpdateAR: ARToolKit video is running. Configuring Unity scene for video." );

				// Retrieve ARToolKit video source(s) frame size and format, and projection matrix, and store globally.
				// Then create the required object(s) to instantiate a mesh/meshes with the frame texture(s).
				// Each mesh lives in a separate "video background" layer.
				if (!VideoIsStereo)
				{

					// ARToolKit video size and format.

					bool ok1 = PluginFunctions.arwGetVideoParams ( out _videoWidth0, out _videoHeight0, out _videoPixelSize0, out _videoPixelFormatString0 );
					if (!ok1)
						return false;
					Log ( LogTag + "Video " + _videoWidth0 + "x" + _videoHeight0 + "@" + _videoPixelSize0 + "Bpp (" + _videoPixelFormatString0 + ")" );

					// ARToolKit projection matrix adjusted for Unity
					float [] projRaw = new float[16];
					PluginFunctions.arwGetProjectionMatrix ( projRaw );
					_videoProjectionMatrix0 = ARUtilityFunctions.MatrixFromFloatArray ( projRaw );
					Log ( LogTag + "Projection matrix: [" + Environment.NewLine + _videoProjectionMatrix0.ToString ().Trim () + "]" );
					if (ContentRotate90)
						_videoProjectionMatrix0 = Matrix4x4.TRS ( Vector3.zero, Quaternion.AngleAxis ( 90.0f, Vector3.back ), Vector3.one ) * _videoProjectionMatrix0;
					if (ContentFlipV)
						_videoProjectionMatrix0 = Matrix4x4.TRS ( Vector3.zero, Quaternion.identity, new Vector3 ( 1.0f, -1.0f, 1.0f ) ) * _videoProjectionMatrix0;
					if (ContentFlipH)
						_videoProjectionMatrix0 = Matrix4x4.TRS ( Vector3.zero, Quaternion.identity, new Vector3 ( -1.0f, 1.0f, 1.0f ) ) * _videoProjectionMatrix0;

					_videoBackgroundMeshGO0 = CreateVideoBackgroundMesh ( 0, _videoWidth0, _videoHeight0, BackgroundLayer0, out _videoColorArray0, out _videoColor32Array0, out _videoTexture0, out _videoMaterial0 );
					if (_videoBackgroundMeshGO0 == null || _videoTexture0 == null || _videoMaterial0 == null)
					{
						Log ( LogTag + "Error: unable to create video mesh." );
					}

				} else  {
					Debug.LogError ("TODO - RoomAug Stereo Camera display not written yet...");
				}



				// Create background camera(s) to actually view the "video background" layer(s).
				bool haveStereoARCameras = false;
				ARCamera[] arCameras = FindObjectsOfType(typeof(ARCamera)) as ARCamera[];
				foreach (ARCamera arc in arCameras) {
					if (arc.Stereo) haveStereoARCameras = true;
				}
				if (!haveStereoARCameras) {
					// Mono display.
					// Use only first video source, regardless of whether VideoIsStereo.
					// (The case where stereo video source is used with a mono display is not likely to be common.)
					_videoBackgroundCameraGO0 = CreateVideoBackgroundCamera("Video background", BackgroundLayer0, out _videoBackgroundCamera0);
					if (_videoBackgroundCameraGO0 == null || _videoBackgroundCamera0 == null) {
						Log (LogTag + "Error: unable to create video background camera.");
					}
				} else {
					// Stereo display.
					// If not VideoIsStereo, right eye will display copy of video frame.
					_videoBackgroundCameraGO0 = CreateVideoBackgroundCamera("Video background (L)", BackgroundLayer0, out _videoBackgroundCamera0);
					_videoBackgroundCameraGO1 = CreateVideoBackgroundCamera("Video background (R)", (VideoIsStereo ? BackgroundLayer1 : BackgroundLayer0), out _videoBackgroundCamera1);
					if (_videoBackgroundCameraGO0 == null || _videoBackgroundCamera0 == null || _videoBackgroundCameraGO1 == null || _videoBackgroundCamera1 == null) {
						Log (LogTag + "Error: unable to create video background camera.");
					}
				}

				// Setup foreground cameras for the video configuration.
				ConfigureForegroundCameras();

				// Adjust viewports of both background and foreground cameras.
				ConfigureViewports();

				// On platforms with multithreaded OpenGL rendering, we need to
				// tell the native plugin the texture ID in advance, so do that now.
				if (_useNativeGLTexturing) {
					if (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.Android) {
						if (!VideoIsStereo) PluginFunctions.arwSetUnityRenderEventUpdateTextureGLTextureID(_videoTexture0.GetNativeTextureID());
						else PluginFunctions.arwSetUnityRenderEventUpdateTextureGLStereoTextureIDs(_videoTexture0.GetNativeTextureID(), _videoTexture1.GetNativeTextureID());
					}
				}

				Log (LogTag + "Scene configured for video.");
				_sceneConfiguredForVideo = true;
			}
		}



//		PluginFunctions.arwCapture();
//		bool ok = PluginFunctions.arwUpdateAR();
//		if (!ok) return false;

		bool gotFrame = PluginFunctions.arwCapture();
		bool ok = PluginFunctions.arwUpdateAR();
		if (!ok) return false;
		if (gotFrame) {
			if (showVideoFeed && _sceneConfiguredForVideo && UseVideoBackground) {
				UpdateTexture();
			}
		}

		return true;
	}

	protected override GameObject CreateVideoBackgroundMesh(int index, int w, int h, int layer, out Color[] vbca, out Color32[] vbc32a, out Texture2D vbt, out Material vbm)
	{
		GameObject vbmgo = base.CreateVideoBackgroundMesh ( index, w, h, layer, out  vbca, out  vbc32a, out  vbt, out  vbm );
		vbmgo.transform.position = farfaraway;
		return vbmgo;
	}

	protected override GameObject CreateVideoBackgroundCamera(String name, int layer, out Camera vbc)
	{
		GameObject vbcgo = base.CreateVideoBackgroundCamera( name,  layer, out vbc );
		vbc.transform.position = farfaraway;
		return vbcgo;
	}




//	// Iterate through all ARCamera objects, asking each to set its viewing frustum and any viewing pose.
//	protected override bool ConfigureForegroundCameras()
//	{
//		bool ret = base.ConfigureForegroundCameras ();
//
//		ARCamera[] arCameras = FindObjectsOfType(typeof(ARCamera)) as ARCamera[];
//		foreach (ARCamera arc in arCameras) {
//			arc.transform.position = farfaraway;
//		}
//
//		return ret;
//	}


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
		UseVideoBackground = true;
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