/*
 *  ARController.cs
 *  ARToolKit for Unity
 *
 *  This file is part of ARToolKit for Unity.
 *
 *  ARToolKit for Unity is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  ARToolKit for Unity is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with ARToolKit for Unity.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  As a special exception, the copyright holders of this library give you
 *  permission to link this library with independent modules to produce an
 *  executable, regardless of the license terms of these independent modules, and to
 *  copy and distribute the resulting executable under terms of your choice,
 *  provided that you also meet, for each linked independent module, the terms and
 *  conditions of the license of that module. An independent module is a module
 *  which is neither derived from nor based on this library. If you modify this
 *  library, you may extend this exception to your version of the library, but you
 *  are not obligated to do so. If you do not wish to do so, delete this exception
 *  statement from your version.
 *
 *  Copyright 2015 Daqri, LLC.
 *  Copyright 2010-2015 ARToolworks, Inc.
 *
 *  Author(s): Philip Lamb, Julian Looser
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

public enum ContentMode
{
	Stretch,
	Fit,
	Fill,
	OneToOne
}

public enum ContentAlign
{
	TopLeft,
	Top,
	TopRight,
	Left,
	Center,
	Right,
	BottomLeft,
	Bottom,
	BottomRight
}

/// <summary>
/// Manages core ARToolKit behaviour.
/// </summary>
/// 
[ExecuteInEditMode]
public class ARController : MonoBehaviour
{
	//
    // Logging.
	//
    public static Action<String> logCallback { get; set; }
    private static List<String> logMessages = new List<String>();
    private const int MaximumLogMessages = 1000;
    private const string LogTag = "J# ARController: ";

	// Application preferences.
	public bool UseNativeGLTexturingIfAvailable = true;
	public bool AllowNonRGBVideo = false;
	public bool QuitOnEscOrBack = true;
	public bool AutoStartAR = true;

	//
	// State.
	//

	private string _version = "";
	private bool _running = false;
	private bool _runOnUnpause = false;
	private bool _sceneConfiguredForVideo = false;
	private bool _sceneConfiguredForVideoWaitingMessageLogged = false;
	private bool _useNativeGLTexturing = false;
	private bool _useColor32 = true;

	//
	// Video source 0.
	//

	// Config. in.
	public string videoCParamName0 = "camera_para";
	public string videoConfigurationWindows0 = "-showDialog -flipV";
	public string videoConfigurationMacOSX0 = "-width=640 -height=480";
	public string videoConfigurationiOS0 = "";
	public string videoConfigurationAndroid0 = "";
	public string videoConfigurationWindowsStore0 = "-device=WinMC -format=BGRA -position=rear";
	public string videoConfigurationLinux0="";
	public int BackgroundLayer0 = 8;

	// Config. out.
	private int _videoWidth0 = 0;
	private int _videoHeight0 = 0;
	private int _videoPixelSize0 = 0;
	private string _videoPixelFormatString0 = "";
	private Matrix4x4 _videoProjectionMatrix0;

	// Stereo config.
	public bool VideoIsStereo = false;
	public string transL2RName = "transL2R";

	//
	// Video source 1.
	//
	
	// Config. in.
	public string videoCParamName1 = "camera_paraR";
	public string videoConfigurationWindows1 = "-devNum=2 -showDialog -flipV";
	public string videoConfigurationMacOSX1 = "-source=1 -width=640 -height=480";
	public string videoConfigurationiOS1 = "";
	public string videoConfigurationAndroid1 = "";
	public string videoConfigurationWindowsStore1 = "-device=WinMC -format=BGRA";
	public string videoConfigurationLinux1="";
	public int BackgroundLayer1 = 9;

	// Config. out.
	private int _videoWidth1 = 0;
	private int _videoHeight1 = 0;
	private int _videoPixelSize1 = 0;
	private string _videoPixelFormatString1 = "";
	private Matrix4x4 _videoProjectionMatrix1;


	//
	// Other
	//
	
    public float NearPlane = 0.01f;
    public float FarPlane = 5.0f;

	public bool ContentRotate90 = false; // Used in CreateVideoBackgroundCamera().
	public bool ContentFlipH = false;
	public bool ContentFlipV = false;
	public ContentAlign ContentAlign = ContentAlign.Center;

	//private int _frameStatsCount = 0;
	//private float _frameStatsTimeUpdateTexture = 0.0f;
	//private float _frameStatsTimeSetPixels = 0.0f;
	//private float _frameStatsTimeApply = 0.0f;

	public readonly static Dictionary<ContentMode, string> ContentModeNames = new Dictionary<ContentMode, string>
	{
		{ContentMode.Stretch, "Stretch"},
		{ContentMode.Fit, "Fit"},
		{ContentMode.Fill, "Fill"},
		{ContentMode.OneToOne, "1:1"},
	};

    // Frames per second calculations
    private int frameCounter = 0;
    private float timeCounter = 0.0f;
    private float lastFramerate = 0.0f;
    private float refreshTime = 0.5f;


    public enum ARToolKitThresholdMode
    {
        Manual = 0,
        Median = 1,
        Otsu = 2,
        Adaptive = 3,
		Bracketing = 4
	}

    public enum ARToolKitLabelingMode
    {
        WhiteRegion = 0,
        BlackRegion = 1,
    }

    public readonly static Dictionary<ARToolKitThresholdMode, string> ThresholdModeDescriptions = new Dictionary<ARToolKitThresholdMode, string>
    {
        {ARToolKitThresholdMode.Manual, "Uses a fixed threshold value"},
        {ARToolKitThresholdMode.Median, "Automatically adjusts threshold to whole-image median"},
        {ARToolKitThresholdMode.Otsu, "Automatically adjusts threshold using Otsu's method for foreground/background determination"},
        {ARToolKitThresholdMode.Adaptive, "Uses adaptive dynamic thresholding (warning: computationally expensive)"},
		{ARToolKitThresholdMode.Bracketing, "Automatically adjusts threshold using bracketed threshold values"}
	};
	
	public enum ARToolKitPatternDetectionMode {
		AR_TEMPLATE_MATCHING_COLOR = 0,
		AR_TEMPLATE_MATCHING_MONO = 1,
		AR_MATRIX_CODE_DETECTION = 2,
		AR_TEMPLATE_MATCHING_COLOR_AND_MATRIX = 3,
		AR_TEMPLATE_MATCHING_MONO_AND_MATRIX = 4
	};

	public enum ARToolKitMatrixCodeType {
	    AR_MATRIX_CODE_3x3 = 3,
    	AR_MATRIX_CODE_3x3_PARITY65 = 257,
    	AR_MATRIX_CODE_3x3_HAMMING63 = 515,
    	AR_MATRIX_CODE_4x4 = 4,
    	AR_MATRIX_CODE_4x4_BCH_13_9_3 = 772,
    	AR_MATRIX_CODE_4x4_BCH_13_5_5 = 1028//,
//    	AR_MATRIX_CODE_5x5 = 5,
//    	AR_MATRIX_CODE_6x6 = 6,
//    	AR_MATRIX_CODE_GLOBAL_ID = 2830
	};
	
	public enum ARToolKitImageProcMode {
		AR_IMAGE_PROC_FRAME_IMAGE = 0,
		AR_IMAGE_PROC_FIELD_IMAGE = 1
	};

	public enum ARW_UNITY_RENDER_EVENTID {
        NOP = 0, // No operation (does nothing).
        UPDATE_TEXTURE_GL = 1,
		UPDATE_TEXTURE_GL_STEREO = 2,
	};

	public enum ARW_ERROR {
		ARW_ERROR_NONE                  =    0,
		ARW_ERROR_GENERIC               =   -1,
		ARW_ERROR_OUT_OF_MEMORY         =   -2,
		ARW_ERROR_OVERFLOW              =   -3,
		ARW_ERROR_NODATA				=   -4,
		ARW_ERROR_IOERROR               =   -5,
		ARW_ERROR_EOF                   =	-6,
		ARW_ERROR_TIMEOUT               =   -7,
		ARW_ERROR_INVALID_COMMAND       =   -8,
		ARW_ERROR_INVALID_ENUM          =   -9,
		ARW_ERROR_THREADS               =   -10,
		ARW_ERROR_FILE_NOT_FOUND		=   -11,
		ARW_ERROR_LENGTH_UNAVAILABLE	=	-12,
		ARW_ERROR_DEVICE_UNAVAILABLE    =   -13
	};

	public enum AR_LOG_LEVEL {
		AR_LOG_LEVEL_DEBUG = 0,
		AR_LOG_LEVEL_INFO,
		AR_LOG_LEVEL_WARN,
		AR_LOG_LEVEL_ERROR,
		AR_LOG_LEVEL_REL_INFO
	}

	// Private fields with accessors.
	[SerializeField]
	private ContentMode currentContentMode = ContentMode.Fit;
	[SerializeField]
    private ARToolKitThresholdMode currentThresholdMode = ARToolKitThresholdMode.Manual;
	[SerializeField]
    private int currentThreshold = 100;
	[SerializeField]
    private ARToolKitLabelingMode currentLabelingMode = ARToolKitLabelingMode.BlackRegion;
	[SerializeField]
	private int currentTemplateSize = 16;
	[SerializeField]
	private int currentTemplateCountMax = 25;
	[SerializeField]
	private float currentBorderSize = 0.25f;
	[SerializeField]
	private ARToolKitPatternDetectionMode currentPatternDetectionMode = ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_COLOR;
	[SerializeField]
	private ARToolKitMatrixCodeType currentMatrixCodeType = ARToolKitMatrixCodeType.AR_MATRIX_CODE_3x3;
	[SerializeField]
	private ARToolKitImageProcMode currentImageProcMode = ARToolKitImageProcMode.AR_IMAGE_PROC_FRAME_IMAGE;
	
	[SerializeField]
	private bool currentNFTMultiMode = false;
	[SerializeField]
	private AR_LOG_LEVEL currentLogLevel = AR_LOG_LEVEL.AR_LOG_LEVEL_INFO;

	//
	// MonoBehavior methods.
	//
	
    void Awake()
    {
		//Log(LogTag + "ARController.Awake())");
		#if UNITY_IOS && !UNITY_EDITOR
		ARNativePluginStatic.aruRequestCamera();
		Thread.Sleep(2000);
		#endif
        if (PluginFunctions.arwInitialiseAR(TemplateSize, TemplateCountMax)) {
			// ARToolKit version number
			_version = PluginFunctions.arwGetARToolKitVersion();
			Log(LogTag + "ARToolKit version " + _version + " initialised.");
		} else {
            Log(LogTag + "Error initialising ARToolKit");
        }
	}

	void OnEnable()
	{
		//Log(LogTag + "ARController.OnEnable()");

	}
	
	void Start()
	{
		//Log(LogTag + "ARController.Start()");
        
		// Ensure ARMarker objects that were instantiated/deserialized before the native interface came up are all loaded.
		ARMarker[] markers = FindObjectsOfType(typeof(ARMarker)) as ARMarker[];
		foreach (ARMarker m in markers) {
			m.Load();
		}
		
		if (Application.isPlaying) {
			
			// Player start.
			if (AutoStartAR) {
				if (!StartAR()) Application.Quit();
			}

		} else {
		
            // Editor Start.
        
        }
	}
	
	void OnApplicationPause(bool paused)
	{
		//Log(LogTag + "ARController.OnApplicationPause(" + paused + ")");
		if (paused) {
			if (_running) {
				StopAR();
				_runOnUnpause = true;
			}
		} else {
			if (_runOnUnpause) {
				StartAR();
				_runOnUnpause = false;
			}
		}
	}
	
	void Update()

    {
		//Log(LogTag + "ARController.Update()");
        
		if (Application.isPlaying) {

            // Player update.
            if (Input.GetKeyDown(KeyCode.Menu) || Input.GetKeyDown(KeyCode.Return)) showGUIDebug = !showGUIDebug;
			if (QuitOnEscOrBack && Input.GetKeyDown(KeyCode.Escape)) Application.Quit(); // On Android, maps to "back" button.
	
	        CalculateFPS();
	        
	        UpdateAR();
	
		} else {
		
            // Editor update.
        
        }
    }

    // Called when the user quits the application, or presses stop in the editor.
    void OnApplicationQuit()
    {
		//Log(LogTag + "ARController.OnApplicationQuit()");
        
        StopAR();
    }

	void OnDisable()
	{
		//Log(LogTag + "ARController.OnDisable()");


    }
	
	// As OnDestroy() is called from the ARController object's destructor, don't do anything
	// here that assumes that the ARController object is still valid. Do that sort of shutdown
	// in OnDisable() instead.
    void OnDestroy()
	{
		//Log(LogTag + "ARController.OnDestroy()");

		Log(LogTag + "Shutting down ARToolKit");
		// arwShutdownAR() causes everything ARToolKit holds to be unloaded.
		if (!PluginFunctions.arwShutdownAR ()) {
			Log(LogTag + "Error shutting down ARToolKit.");
		}

		// Classes inheriting from MonoBehavior should set all static member variables to null on unload.
	}
	
	//
	// User-callable AR methods.
	//
	
	public bool StartAR()
	{
		// Catch attempts to inadvertently call StartAR() twice.
        if (_running) {
            Log(LogTag + "WARNING: StartAR() called while already running. Ignoring.\n");
            return false;
        }
        
        Log(LogTag + "Starting AR.");

		_sceneConfiguredForVideo = _sceneConfiguredForVideoWaitingMessageLogged = false;
        
        // Check rendering device.
        string renderDevice = SystemInfo.graphicsDeviceVersion;
        _useNativeGLTexturing = !renderDevice.StartsWith("Direct") && UseNativeGLTexturingIfAvailable;
        if (_useNativeGLTexturing) {
            Log(LogTag + "Render device: " + renderDevice + ", using native GL texturing.");
        } else {
            Log(LogTag + "Render device: " + renderDevice + ", using Unity texturing.");
        }

		// Retrieve video configuration, and append any required per-platform overrides.
		// For native GL texturing we need monoplanar video; iOS and Android default to biplanar format. 
        string videoConfiguration0;
		string videoConfiguration1;
		switch (Application.platform) {
			case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
				videoConfiguration0 = videoConfigurationMacOSX0;
				videoConfiguration1 = videoConfigurationMacOSX1;
				if (_useNativeGLTexturing || !AllowNonRGBVideo) {
					if (videoConfiguration0.IndexOf("-device=QuickTime7") != -1 || videoConfiguration0.IndexOf("-device=QUICKTIME") != -1) videoConfiguration0 += " -pixelformat=BGRA";
					if (videoConfiguration1.IndexOf("-device=QuickTime7") != -1 || videoConfiguration1.IndexOf("-device=QUICKTIME") != -1) videoConfiguration1 += " -pixelformat=BGRA";
				}
				break;
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                videoConfiguration0 = videoConfigurationWindows0;
				videoConfiguration1 = videoConfigurationWindows1;
				if (_useNativeGLTexturing || !AllowNonRGBVideo) {
					if (videoConfiguration0.IndexOf("-device=WinMF") != -1) videoConfiguration0 += " -format=BGRA";
					if (videoConfiguration1.IndexOf("-device=WinMF") != -1) videoConfiguration1 += " -format=BGRA";
				}
				break;
            case RuntimePlatform.Android:
				videoConfiguration0 = videoConfigurationAndroid0 + " -cachedir=\"" + Application.temporaryCachePath + "\""  + (_useNativeGLTexturing || !AllowNonRGBVideo ? " -format=RGBA" : "");
				videoConfiguration1 = videoConfigurationAndroid1 + " -cachedir=\"" + Application.temporaryCachePath + "\""  + (_useNativeGLTexturing || !AllowNonRGBVideo ? " -format=RGBA" : "");
				break;
            case RuntimePlatform.IPhonePlayer:
				videoConfiguration0 = videoConfigurationiOS0 + (_useNativeGLTexturing || !AllowNonRGBVideo ? " -format=BGRA" : "");
				videoConfiguration1 = videoConfigurationiOS1 + (_useNativeGLTexturing || !AllowNonRGBVideo ? " -format=BGRA" : "");
				break;
			case RuntimePlatform.MetroPlayerX86:
			case RuntimePlatform.MetroPlayerX64:
			case RuntimePlatform.MetroPlayerARM:
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
		byte[] cparam0 = null;
		byte[] cparam1 = null;
		byte[] transL2R = null;
        ta = Resources.Load("ardata/" + videoCParamName0, typeof(TextAsset)) as TextAsset;
        if (ta == null) {		
            // Error - the camera_para.dat file isn't in the right place			
			Log(LogTag + "StartAR(): Error: Camera parameters file not found at Resources/ardata/" + videoCParamName0 + ".bytes");
            return (false);
        }
        cparam0 = ta.bytes;
		if (VideoIsStereo) {
			ta = Resources.Load("ardata/" + videoCParamName1, typeof(TextAsset)) as TextAsset;
			if (ta == null) {		
				// Error - the camera_para.dat file isn't in the right place			
				Log(LogTag + "StartAR(): Error: Camera parameters file not found at Resources/ardata/" + videoCParamName1 + ".bytes");
				return (false);
			}
			cparam1 = ta.bytes;
			ta = Resources.Load("ardata/" + transL2RName, typeof(TextAsset)) as TextAsset;
			if (ta == null) {		
				// Error - the transL2R.dat file isn't in the right place			
				Log(LogTag + "StartAR(): Error: The stereo calibration file not found at Resources/ardata/" + transL2RName + ".bytes");
				return (false);
			}
			transL2R = ta.bytes;
		}
        
        // Begin video capture and marker detection.
		if (!VideoIsStereo) {
			Log(LogTag + "Starting ARToolKit video with vconf '" + videoConfiguration0 + "'.");
			//_running = PluginFunctions.arwStartRunning(videoConfiguration, cparaName, nearPlane, farPlane);
			_running = PluginFunctions.arwStartRunningB(videoConfiguration0, cparam0, cparam0.Length, NearPlane, FarPlane);
		} else {
			Log(LogTag + "Starting ARToolKit video with vconfL '" + videoConfiguration0 + "', vconfR '" + videoConfiguration1 + "'.");
			//_running = PluginFunctions.arwStartRunningStereo(vconfL, cparaNameL, vconfR, cparaNameR, transL2RName, nearPlane, farPlane);
			_running = PluginFunctions.arwStartRunningStereoB(videoConfiguration0, cparam0, cparam0.Length, videoConfiguration1, cparam1, cparam1.Length, transL2R, transL2R.Length, NearPlane, FarPlane);

		}
        
        if (!_running) {
            Log(LogTag + "Error starting running");
			ARW_ERROR error = (ARW_ERROR)PluginFunctions.arwGetError();
			if (error == ARW_ERROR.ARW_ERROR_DEVICE_UNAVAILABLE) {
				showGUIErrorDialogContent = "Unable to start AR tracking. The camera may be in use by another application.";
			} else {
				showGUIErrorDialogContent = "Unable to start AR tracking. Please check that you have a camera connected.";
			}
			showGUIErrorDialog = true;
            return false;
        }
        
		// After calling arwStartRunningB/arwStartRunningStereoB, set ARToolKit configuration.
        Log(LogTag + "Setting ARToolKit tracking settings.");
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
	
	bool UpdateAR()
	{
        if (!_running) {
            return false;
        }
        
        if (!_sceneConfiguredForVideo) {
            
            // Wait for the wrapper to confirm video frames have arrived before configuring our video-dependent stuff.
            if (!PluginFunctions.arwIsRunning()) {
				if (!_sceneConfiguredForVideoWaitingMessageLogged) {
					Log(LogTag + "UpdateAR: Waiting for ARToolKit video.");
					_sceneConfiguredForVideoWaitingMessageLogged = true;
				}
            } else {
				Log(LogTag + "UpdateAR: ARToolKit video is running. Configuring Unity scene for video.");
		
				// Retrieve ARToolKit video source(s) frame size and format, and projection matrix, and store globally.
				// Then create the required object(s) to instantiate a mesh/meshes with the frame texture(s).
				// Each mesh lives in a separate "video background" layer.
				if (!VideoIsStereo) {

					// ARToolKit video size and format.
				 
					bool ok1 = PluginFunctions.arwGetVideoParams(out _videoWidth0, out _videoHeight0, out _videoPixelSize0, out _videoPixelFormatString0);
					if (!ok1) return false;
					Log(LogTag + "Video " + _videoWidth0 + "x" + _videoHeight0 + "@" + _videoPixelSize0 + "Bpp (" + _videoPixelFormatString0 + ")");
					
					// ARToolKit projection matrix adjusted for Unity
					float[] projRaw = new float[16];
					PluginFunctions.arwGetProjectionMatrix(projRaw);
					_videoProjectionMatrix0 = ARUtilityFunctions.MatrixFromFloatArray(projRaw);
					Log(LogTag + "Projection matrix: [" + Environment.NewLine + _videoProjectionMatrix0.ToString().Trim() + "]");
					if (ContentRotate90) _videoProjectionMatrix0 = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(90.0f, Vector3.back), Vector3.one) * _videoProjectionMatrix0;
					if (ContentFlipV) _videoProjectionMatrix0 = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1.0f, -1.0f, 1.0f)) * _videoProjectionMatrix0;
					if (ContentFlipH) _videoProjectionMatrix0 = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1.0f, 1.0f, 1.0f)) * _videoProjectionMatrix0;

				}

				// Setup foreground cameras for the video configuration.
				ConfigureForegroundCameras();

				// Adjust viewports of both background and foreground cameras.
				ConfigureViewports();

				Log (LogTag + "Scene configured for video.");
	            _sceneConfiguredForVideo = true;
	        } // !running
		} // !sceneConfiguredForVideo
        
		bool gotFrame = PluginFunctions.arwCapture();
		bool ok = PluginFunctions.arwUpdateAR();
		if (!ok) return false;

		return true;
	}
	
	public bool StopAR()
	{
        if (!_running) {
            return false;
        }
        
		Log(LogTag + "Stopping AR.");

        // Stop video capture and marker detection.
    	if (!PluginFunctions.arwStopRunning()) {
            Log(LogTag + "Error stopping AR.");
        }

		_running = false;
		return true;
	}

	//
	// User-callable configuration methods.
	//

	// At present, you must call this before calling StartAR(), or after calling StopAR().
	public void SetContentForScreenOrientation(bool cameraIsFrontFacing)
	{
		ScreenOrientation orientation = Screen.orientation;
		if (orientation == ScreenOrientation.Portrait) { // Portait
			ContentRotate90 = true;
			ContentFlipV = false;
			ContentFlipH = cameraIsFrontFacing;
		} else if (orientation == ScreenOrientation.LandscapeLeft) { // Landscape with top of device at left.
			ContentRotate90 = false;
			ContentFlipV = false;
			ContentFlipH = cameraIsFrontFacing;
		} else if (orientation == ScreenOrientation.PortraitUpsideDown) { // Portrait upside-down.
			ContentRotate90 = true;
			ContentFlipV = true;
			ContentFlipH = (!cameraIsFrontFacing);
		} else if (orientation == ScreenOrientation.LandscapeRight) { // Landscape with top of device at right.
			ContentRotate90 = false;
			ContentFlipV = true;
			ContentFlipH = (!cameraIsFrontFacing);
		}
	}





	// Iterate through all ARCamera objects, asking each to set its viewing frustum and any viewing pose.
	private bool ConfigureForegroundCameras()
	{
		// Note if  any of the ARCamera objects are in optical mode so we can adjust UseVideoBackground.
		bool optical = false;

		ARCamera[] arCameras = FindObjectsOfType(typeof(ARCamera)) as ARCamera[];
		foreach (ARCamera arc in arCameras) {

			bool ok;
			if (!arc.Stereo) {
				// A mono display.
				ok = arc.SetupCamera(NearPlane, FarPlane, _videoProjectionMatrix0, ref optical);
			} else {
				// One eye of a stereo display.
				if (arc.StereoEye == ARCamera.ViewEye.Left) {
					ok = arc.SetupCamera(NearPlane, FarPlane, _videoProjectionMatrix0, ref optical);
				} else {
					ok = arc.SetupCamera(NearPlane, FarPlane, (VideoIsStereo ? _videoProjectionMatrix1 : _videoProjectionMatrix0), ref optical);
				}
			}
			if (!ok) {
				Log(LogTag + "Error setting up ARCamera.");
			}
		}

		return true;
	}

	private bool ConfigureViewports()
	{
		bool haveStereoARCamera = false;

		// Set viewports on foreground camera(s).
		ARCamera[] arCameras = FindObjectsOfType(typeof(ARCamera)) as ARCamera[];
		foreach (ARCamera arc in arCameras) {
			if (!arc.Stereo) {
				// A mono display.
				arc.gameObject.GetComponent<Camera>().pixelRect = getViewport(_videoWidth0, _videoHeight0, false, ARCamera.ViewEye.Left);
			} else {
				// One eye of a stereo display.
				haveStereoARCamera = true;
				if (arc.StereoEye == ARCamera.ViewEye.Left) {
					arc.gameObject.GetComponent<Camera>().pixelRect = getViewport(_videoWidth0, _videoHeight0, true, ARCamera.ViewEye.Left);
				} else {
					arc.gameObject.GetComponent<Camera>().pixelRect = getViewport((VideoIsStereo ? _videoWidth1 : _videoWidth0), (VideoIsStereo ? _videoHeight1 : _videoHeight0), true, ARCamera.ViewEye.Right);
				}
			}
		}

		return true;
	}

	// References globals ContentMode, ContentAlign, ContentRotate90, Screen.width, Screen.height.
	private Rect getViewport(int contentWidth, int contentHeight, bool stereo, ARCamera.ViewEye viewEye)
	{
		int backingWidth = Screen.width;
		int backingHeight = Screen.height;
		int left, bottom, w, h;

		if (stereo) {
			// Assume side-by-side or half side-by-side mode.
			w = backingWidth / 2;
			h = backingHeight;
			if (viewEye == ARCamera.ViewEye.Left) left = 0;
			else left = backingWidth / 2;
			bottom = 0;
		} else {
			if (ContentMode == ContentMode.Stretch) {
				w = backingWidth;
				h = backingHeight;
			} else {
				int contentWidthFinalOrientation = (ContentRotate90 ? contentHeight : contentWidth);
				int contentHeightFinalOrientation = (ContentRotate90 ? contentWidth : contentHeight);
				if (ContentMode == ContentMode.Fit || ContentMode == ContentMode.Fill) {
					float scaleRatioWidth, scaleRatioHeight, scaleRatio;
					scaleRatioWidth = (float)backingWidth / (float)contentWidthFinalOrientation;
					scaleRatioHeight = (float)backingHeight / (float)contentHeightFinalOrientation;
					if (ContentMode == ContentMode.Fill) scaleRatio = Math.Max(scaleRatioHeight, scaleRatioWidth);
					else scaleRatio = Math.Min(scaleRatioHeight, scaleRatioWidth);
					w = (int)((float)contentWidthFinalOrientation * scaleRatio);
					h = (int)((float)contentHeightFinalOrientation * scaleRatio);
				} else { // 1:1
					w = contentWidthFinalOrientation;
					h = contentHeightFinalOrientation;
				}
			}

			if (ContentAlign == ContentAlign.TopLeft
				|| ContentAlign == ContentAlign.Left
				|| ContentAlign == ContentAlign.BottomLeft) left = 0;
			else if (ContentAlign == ContentAlign.TopRight
				|| ContentAlign == ContentAlign.Right
				|| ContentAlign == ContentAlign.BottomRight) left = backingWidth - w;
			else left = (backingWidth - w) / 2;

			if (ContentAlign == ContentAlign.BottomLeft
				|| ContentAlign == ContentAlign.Bottom
				|| ContentAlign == ContentAlign.BottomRight) bottom = 0;
			else if (ContentAlign == ContentAlign.TopLeft
				|| ContentAlign == ContentAlign.Top
				|| ContentAlign == ContentAlign.TopRight) bottom = backingHeight - h;
			else bottom = (backingHeight - h) / 2;
		}

		//Log(LogTag + "For " + backingWidth + "x" + backingHeight + " screen, calculated viewport " + w + "x" + h + " at (" + left + ", " + bottom + ").");
		return new Rect(left, bottom, w, h);
	}

    public bool DebugVideo
    {
        get
        {
			return (PluginFunctions.arwGetVideoDebugMode());
        }

        set
        {
            PluginFunctions.arwSetVideoDebugMode(value);
        }
    }

	public string Version
	{
		get
		{
			return _version;
		}
	}

    public ARController.ARToolKitThresholdMode VideoThresholdMode
    {
        get
        {
			int ret;
            if (_running) {
				ret = PluginFunctions.arwGetVideoThresholdMode();
                if (ret >= 0) currentThresholdMode = (ARController.ARToolKitThresholdMode)ret;
				else currentThresholdMode = ARController.ARToolKitThresholdMode.Manual;
            }
            return currentThresholdMode;
        }

        set
        {
            currentThresholdMode = value;
            if (_running) {
				PluginFunctions.arwSetVideoThresholdMode((int)currentThresholdMode);
            }
        }
    }

    public int VideoThreshold
    {
        get
        {
            if (_running) {
				currentThreshold = PluginFunctions.arwGetVideoThreshold();
            	if (currentThreshold < 0 || currentThreshold > 255) currentThreshold = 100;
			}
            return currentThreshold;
        }

        set
        {
            currentThreshold = value;
            if (_running) {
                PluginFunctions.arwSetVideoThreshold(value);
            }
        }
    }

    public ARController.ARToolKitLabelingMode LabelingMode
    {
        get
        {
			int ret;
            if (_running) {
				ret = PluginFunctions.arwGetLabelingMode();
                if (ret >= 0) currentLabelingMode = (ARController.ARToolKitLabelingMode)ret;
				else currentLabelingMode = ARController.ARToolKitLabelingMode.BlackRegion;
            }
            return currentLabelingMode;
        }

        set
        {
            currentLabelingMode = value;
            if (_running) {
				PluginFunctions.arwSetLabelingMode((int)currentLabelingMode);
            }
        }
    }

    public float BorderSize
    {
        get
        {
			float ret;
            if (_running) {
				ret = PluginFunctions.arwGetBorderSize();
                if (ret > 0.0f && ret < 0.5f) currentBorderSize = ret;
				else currentBorderSize = 0.25f;
            }
            return currentBorderSize;
        }

        set
        {
            currentBorderSize = value;
            if (_running) {
				PluginFunctions.arwSetBorderSize(currentBorderSize);
            }
        }
    }

	public ContentMode ContentMode
	{
		get
		{
			return currentContentMode;
		}

		set
		{
			if (currentContentMode != value) {
				currentContentMode = value;
				if (_running) {
					ConfigureViewports();
				}
			}
		}
	}

	public int TemplateSize
	{
		get
		{
			return currentTemplateSize;
		}
		
		set
		{
			currentTemplateSize = value;
			Log (LogTag + "Warning: template size changed. Please reload scene.");
		}
	}
	
	public int TemplateCountMax
	{
		get
		{
			return currentTemplateCountMax;
		}
		
		set
		{
			currentTemplateCountMax = value;
			Log (LogTag + "Warning: template maximum count changed. Please reload scene.");
		}
	}
	
	public ARController.ARToolKitPatternDetectionMode PatternDetectionMode
	{
        get
        {
			int ret;
            if (_running) {
				ret = PluginFunctions.arwGetPatternDetectionMode();
                if (ret >= 0) currentPatternDetectionMode = (ARController.ARToolKitPatternDetectionMode)ret;
				else currentPatternDetectionMode = ARController.ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_COLOR;
            }
            return currentPatternDetectionMode;
        }

        set
        {
            currentPatternDetectionMode = value;
            if (_running) {
				PluginFunctions.arwSetPatternDetectionMode((int)currentPatternDetectionMode);
            }
        }
    }

    public ARController.ARToolKitMatrixCodeType MatrixCodeType
    {
        get
        {
			int ret;
            if (_running) {
				ret = PluginFunctions.arwGetMatrixCodeType();
                if (ret >= 0) currentMatrixCodeType = (ARController.ARToolKitMatrixCodeType)ret;
				else currentMatrixCodeType = ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_3x3;
            }
            return currentMatrixCodeType;
        }

        set
        {
            currentMatrixCodeType = value;
            if (_running) {
				PluginFunctions.arwSetMatrixCodeType((int)currentMatrixCodeType);
            }
        }
    }

    public ARController.ARToolKitImageProcMode ImageProcMode
    {
        get
        {
			int ret;
            if (_running) {
				ret = PluginFunctions.arwGetImageProcMode();
                if (ret >= 0) currentImageProcMode = (ARController.ARToolKitImageProcMode)ret;
				else currentImageProcMode = ARController.ARToolKitImageProcMode.AR_IMAGE_PROC_FRAME_IMAGE;
            }
            return currentImageProcMode;
        }

        set
        {
            currentImageProcMode = value;
            if (_running) {
				PluginFunctions.arwSetImageProcMode((int)currentImageProcMode);
            }
        }
    }

	public bool NFTMultiMode
	{
		get
		{
			if (_running) {
				currentNFTMultiMode = PluginFunctions.arwGetNFTMultiMode();
			}
			return currentNFTMultiMode;
		}
		
		set
		{
			currentNFTMultiMode = value;
			if (_running) {
				PluginFunctions.arwSetNFTMultiMode(currentNFTMultiMode);
			}
		}
	}

	public AR_LOG_LEVEL LogLevel
	{
		get
		{
			return currentLogLevel;
		}
		
		set
		{
			currentLogLevel = value;
			PluginFunctions.arwSetLogLevel((int)currentLogLevel);
		}
	}
	

	
	//
	// Internal methods.
	//





    public static void Log(String msg)
    {
        // Add the new log message to the collection. If the collection has grown too large
        // then remove the oldest messages.
        logMessages.Add(msg);
        while (logMessages.Count > MaximumLogMessages) logMessages.RemoveAt(0);

        // If there is a logCallback then use that to handle the log message. Otherwise simply
        // print out on the debug console.
        if (logCallback != null) logCallback(msg);
        else Debug.Log(msg);
    }

    private void CalculateFPS()
    {
        if (timeCounter < refreshTime) {
            timeCounter += Time.deltaTime;
            frameCounter++;
        } else {
            lastFramerate = (float)frameCounter / timeCounter;
            frameCounter = 0;
            timeCounter = 0.0f;
        }
    }
    

    // ------------------------------------------------------------------------------------
    // GUI Methods
    // ------------------------------------------------------------------------------------

    private GUIStyle[] style = new GUIStyle[3];
    private bool guiSetup = false;

    private void SetupGUI()
    {

        style[0] = new GUIStyle(GUI.skin.label);
        style[0].normal.textColor = new Color(0, 0, 0, 1);

        style[1] = new GUIStyle(GUI.skin.label);
        style[1].normal.textColor = new Color(0.0f, 0.5f, 0.0f, 1);

        style[2] = new GUIStyle(GUI.skin.label);
        style[2].normal.textColor = new Color(0.5f, 0.0f, 0.0f, 1);

        guiSetup = true;
    }
	
	private bool showGUIErrorDialog = false;
	private string showGUIErrorDialogContent = "";
	private Rect showGUIErrorDialogWinRect = new Rect(0.0f, 0.0f, 320.0f, 240.0f);

	private bool showGUIDebug = true;
    private bool showGUIDebugInfo = true;
	private bool showGUIDebugLogConsole = false;
	
    void OnGUI()
    {
        if (!guiSetup) SetupGUI();

		if (showGUIErrorDialog) {
			showGUIErrorDialogWinRect = GUILayout.Window(0, showGUIErrorDialogWinRect, DrawErrorDialog, "Error");
			showGUIErrorDialogWinRect.x = ((float)Screen.width - showGUIErrorDialogWinRect.width) * 0.5f;
			showGUIErrorDialogWinRect.y = ((float)Screen.height - showGUIErrorDialogWinRect.height) * 0.5f;
			GUILayout.Window(0, showGUIErrorDialogWinRect, DrawErrorDialog, "Error");	
		}
		
        if (showGUIDebug) {
            if (GUI.Button(new Rect(570, 250, 150, 50), "Info")) showGUIDebugInfo = !showGUIDebugInfo;
            if (showGUIDebugInfo) DrawInfoGUI();

			if (GUI.Button(new Rect(570, 390, 150, 50), "Content mode: " + ContentModeNames[ContentMode])) CycleContentMode();


            if (GUI.Button(new Rect(570, 320, 150, 50), "Log")) showGUIDebugLogConsole = !showGUIDebugLogConsole;
            if (showGUIDebugLogConsole) DrawLogConsole();

#if UNITY_ANDROID
			if (Application.platform == RuntimePlatform.Android) {
				if (GUI.Button(new Rect(400, 250, 150, 50), "Camera preferences")) {
					using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
						AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
						jo.Call("launchPreferencesActivity");
					}
				}
			}
#endif
			if (GUI.Button(new Rect(400, 390, 150, 50), "Debug mode: " + DebugVideo)) DebugVideo = !DebugVideo;
		
			ARToolKitThresholdMode currentThresholdMode = VideoThresholdMode;
	        GUI.Label(new Rect(400, 460, 320, 25), "Threshold Mode: " + currentThresholdMode);
			if (currentThresholdMode == ARToolKitThresholdMode.Manual) {
		        float currentThreshold = VideoThreshold;
		        float newThreshold = GUI.HorizontalSlider(new Rect(400, 495, 270, 25), currentThreshold, 0, 255);
		        if (newThreshold != currentThreshold) {
		            VideoThreshold = (int)newThreshold;
		        }
				GUI.Label(new Rect(680, 495, 50, 25), VideoThreshold.ToString());
			}

            GUI.Label(new Rect(700, 20, 100, 25), "FPS: " + lastFramerate);
        }
    }
	
	
	private void DrawErrorDialog(int winID)
	{
		GUILayout.BeginVertical();
		GUILayout.Label(showGUIErrorDialogContent);
	   	GUILayout.FlexibleSpace();
		if (GUILayout.Button("OK")) showGUIErrorDialog = false;
		GUILayout.EndVertical();
	}
	
//	private bool toggle = false;

    private void DrawInfoGUI()
    {
        // Basic ARToolKit information
        GUI.Label(new Rect(10, 10, 500, 25), "ARToolKit " + Version);
        GUI.Label(new Rect(10, 30, 500, 25), "Video " + _videoWidth0 + "x" + _videoHeight0 + "@" + _videoPixelSize0 + "Bpp (" + _videoPixelFormatString0 + ")");

        // Some system information
        GUI.Label(new Rect(10, 90, 500, 25), "Graphics device: " + SystemInfo.graphicsDeviceName);
        GUI.Label(new Rect(10, 110, 500, 25), "Operating system: " + SystemInfo.operatingSystem);
        GUI.Label(new Rect(10, 130, 500, 25), "Processors: (" + SystemInfo.processorCount + "x) " + SystemInfo.processorType);
        GUI.Label(new Rect(10, 150, 500, 25), "Memory: " + SystemInfo.systemMemorySize + "MB");

        GUI.Label(new Rect(10, 170, 500, 25), "Resolution : " + Screen.currentResolution.width + "x" + Screen.currentResolution.height + "@" + Screen.currentResolution.refreshRate + "Hz");
        GUI.Label(new Rect(10, 190, 500, 25), "Screen : " + Screen.width + "x" + Screen.height);
        //GUI.Label(new Rect(10, 250, 800, 100), "Base Data Path : " + BaseDataPath);
		

        int y = 350;

		ARMarker[] markers = Component.FindObjectsOfType(typeof(ARMarker)) as ARMarker[];
		foreach (ARMarker m in markers) {
            GUI.Label(new Rect(10, y, 500, 25), "Marker: " + m.UID + ", " + m.Visible);
            y += 25;
        }
    }


    public Vector2 scrollPosition = Vector2.zero;

    private void DrawLogConsole()
    {
        Rect consoleRect = new Rect(0, 0, Screen.width, 200);

        GUIStyle s = new GUIStyle(GUI.skin.box);
        s.normal.background = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        s.normal.background.SetPixel(0, 0, new Color(1, 1, 1, 1));
        s.normal.background.Apply();

        GUI.Box(consoleRect, "", s);

        DrawLogEntries(consoleRect, false);
    }


    private void DrawLogEntries(Rect container, bool reverse)
    {
        //int numItems = logMessages.Count;

        Rect scrollViewRect = new Rect(5, 5, container.width - 10, container.height - 10);

        float height = 0;
        float width = scrollViewRect.width - 30;

        foreach (String s in logMessages) {
            float h = GUI.skin.label.CalcHeight(new GUIContent(s), width);
            height += h;
        }

        Rect contentRect = new Rect(0, 0, width, height);

        scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, contentRect);

        float y = 0;

        IEnumerable<string> lm = logMessages;
        if (reverse) lm = lm.Reverse<string>();

        int i = 0;

        foreach (String s in lm) {
            i = 0;
            if (s.StartsWith(LogTag)) i = 1;
            else if (s.StartsWith("ARController C++:")) i = 2;

            float h = GUI.skin.label.CalcHeight(new GUIContent(s), width);
            GUI.Label(new Rect(0, y, width, h), s, style[i]);

            y += h;
        }

        GUI.EndScrollView();
    }

	private void CycleContentMode()
	{
		switch (ContentMode) {
		case ContentMode.Fit:
			ContentMode = ContentMode.Stretch;
			//ContentMode = ContentMode.Fill; // Fill and OneToOne mode can potentially result in negative values for viewport x and y. Unity can't handle that.
			break;
			//case ContentMode.Fill:
			//	ContentMode = ContentMode.Stretch;
			//	break;
			//case ContentMode.Stretch:
			//	ContentMode = ContentMode.OneToOne;
			//	break;
		default:
			ContentMode = ContentMode.Fit;
			break;
		}
	}
}

