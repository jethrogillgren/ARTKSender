//using System;
//using System.ComponentModel.Design.Serialization;
//using System.IO;
//using SKStudios.Common.Editor;
//using SKStudios.Common.Utils;
//using UnityEditor;
//using UnityEngine;
//using SKStudios.Rendering;
//
//namespace SKStudios.Portals.Editor
//{
//    [CustomEditor(typeof(PortalController))]
//    [InitializeOnLoad]
//    [CanEditMultipleObjects]
//    [ExecuteInEditMode]
//    public class PortalControllerEditor : MeshFilterPreview
//    {
//        private PreviewRenderUtility _previewRenderUtility;
//        private LineRenderer renderer;
//        private GameObject VisLineObj;
//
//        private static bool _imageFoldout = false;
//        private static bool _interactionFoldout = false;
//        private static bool _editorFoldout = false;
//
//        private static float bumperSize = 3;
//        private int tab = 0;
//
//        private static Camera _sceneCameraDupe;
//        UnityEditor.Editor _matEditor;
//
//        public override MeshFilter TargetMeshFilter {
//            get {
//                //if(SKSGlobalRenderSettings.Preview)
//               //     return PortalController.PreviewRenderer.GetComponent<MeshFilter>();
//               // else
//                    return EditorPreviewFilter;
//                
//            }
//            set {}
//        }
//        public override MeshRenderer TargetMeshRenderer {
//            get {
//                //if (SKSGlobalRenderSettings.Preview)
//                //    return PortalController.PreviewRenderer;
//                //else
//                    return EditorPreviewRenderer;
//            }
//            set { }
//        }
//
//        private MeshRenderer _editorPreviewRenderer;
//        private MeshRenderer EditorPreviewRenderer {
//            get {
//                if (!_editorPreviewRenderer) {
//                    GameObject editorPreviewObj = EditorUtility.CreateGameObjectWithHideFlags("Preview Object", HideFlags.HideAndDontSave);
//                    editorPreviewObj.tag = "SKSEditorTemp";
//                    editorPreviewObj.transform.SetParent(PortalController.transform);
//                    editorPreviewObj.transform.localPosition = Vector3.zero;
//                    
//                    _editorPreviewRenderer = editorPreviewObj.AddComponent<MeshRenderer>();
//                   
//                    _editorPreviewRenderer.sharedMaterials = new Material[4];
//                    Material m = new Material(Shader.Find("Standard"));
//                    Material[] MaterialArray = new Material[4];
//                    MaterialArray[0] = Resources.Load<Material>("UI/Materials/Floor");
//                    MaterialArray[1] = Resources.Load<Material>("UI/Materials/Background");
//                    MaterialArray[2] = PortalController.PortalMaterial;
//                    MaterialArray[3] = Resources.Load<Material>("UI/Materials/Background");
//                    _editorPreviewRenderer.sharedMaterials = MaterialArray;
//                }
//                return _editorPreviewRenderer;
//            }
//        }
//
//        private MeshFilter _editorPreviewFilter;
//
//        private MeshFilter EditorPreviewFilter{
//            get {
//                if (!_editorPreviewFilter) {
//                    _editorPreviewFilter = EditorPreviewRenderer.gameObject.AddComponent<MeshFilter>();
//                    _editorPreviewFilter.mesh = Resources.Load<Mesh>("UI/PortalPreview");
//                }
//                return _editorPreviewFilter;
//            }
//        }
//
//        private String sourceName;
//
//        public Material PortalMaterial {
//            set {
//                if (!PortalController)
//                    return;
//
//                //if (Application.isPlaying)
//                //   return;
//
//                PortalController.PortalMaterial = value;
//                try {
//                    DestroyImmediate(_matEditor, true);
//                }
//                catch { }
//            }
//        }
//
//
//
//        private Texture2D _fakeBackgroundTex;
//
//        private Texture2D FakeBackgroundTex {
//            get {
//                if (!_fakeBackgroundTex)
//                {
//                    _fakeBackgroundTex = Resources.Load<Texture2D>("UI/PortalFakeBackground");
//                }
//                return _fakeBackgroundTex;
//            }
//        }
//
//        private void OnEnable()
//        {
//
//            if (Application.isPlaying) return;
//
//            if (!PortalController.gameObject.activeInHierarchy)
//                return;
//
//            if (!PortalController.isActiveAndEnabled)
//                return;
//            if (!PortalController.TargetController)
//                return;
//
//          
//
//            if (SKSGlobalRenderSettings.Preview)
//            {
//
//                //PortalController.GetComponent<Renderer>().sharedMaterial.color = Color.clear;
//                Camera pokecam = PortalController.PreviewCamera;
//                GameObject pokeObj = PortalController.PreviewRoot;
//                Camera pokecam2 = PortalController.TargetController.PreviewCamera;
//                GameObject pokeObj2 = PortalController.TargetController.PreviewRoot;
//                pokecam2.enabled = false;
//                pokeObj2.SetActive(false);
//            }
//
//
//
//            //EditorApplication.update -= UpdatePreview;
//            //EditorApplication.update += UpdatePreview;
//
//#if SKS_VR
//            //GlobalPortalSettings.SinglePassStereo = settings.SinglePassStereoCached;
//#endif
//
//        }
//
//        
//        private void OnDisable()
//        {
//            //EditorApplication.update -= UpdatePreview;
//
//            CleanupTemp();
//            if (PortalController && PortalController.TargetController)
//                PortalController.TargetController.CleanupTemp();
//            //DestroyImmediate(_matEditor, true);
//
//            if (Application.isPlaying)
//                return;
//
//            if (PortalController)
//                PortalController.GetComponent<Renderer>().enabled = true;
//
//            if (PortalController)
//                PortalController.GetComponent<Renderer>().sharedMaterial.color = PortalController.color;
//        }
//
//        //Preview texture for portals
//        private RenderTexture _previewTex;
//
//        private RenderTexture PreviewTex {
//            get {
//                if (_previewTex)
//                    RenderTexture.ReleaseTemporary(_previewTex);
//
//                _previewTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 24,
//                    RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Default);
//
//                return _previewTex;
//            }
//        }
//
//        private PortalController _portalController;
//
//        private PortalController PortalController {
//            get {
//                if (!_portalController)
//                    _portalController = (PortalController)target;
//                return _portalController;
//            }
//        }
//
//        public override bool HasPreviewGUI() {
//            return base.HasPreviewGUI_s();
//        }
//
//        public override void OnPreviewGUI(Rect r, GUIStyle background)
//        {
//            TargetMeshRenderer.gameObject.SetActive(true);
//            TargetMeshRenderer.gameObject.transform.localPosition = Vector3.zero;
//            
//            try
//            {
//
//                Texture fakePortalTex = FakeBackgroundTex;
//                PortalController.PortalMaterial.SetTexture("_LeftEyeTexture", fakePortalTex);
//                PortalController.PortalMaterial.SetVector("_LeftDrawPos", new Vector4(0, 0, 1, 1));
//                PortalController.PortalMaterial.SetTexture("_RightEyeTexture", fakePortalTex);
//                PortalController.PortalMaterial.SetVector("_RightDrawPos", new Vector4(0, 0, 1, 1));
//            }
//            catch
//            {
//                //Unity silliness again
//            }
//            base.OnPreviewGUI_s(r, background);
//            TargetMeshRenderer.gameObject.SetActive(false);
//        }
//
//        public override void OnInspectorGUI() {
//            TargetMeshRenderer.gameObject.SetActive(false);
//            tab = GUILayout.Toolbar(tab, new string[] { "Instance settings", "Global Portal Settings" });
//            if (tab == 0) {
//                try {
//                    GUILayout.Label("Instance settings:", EditorStyles.boldLabel);
//                    foreach (PortalController p in targets)
//                        Undo.RecordObject(p, "Portal Controller Editor Changes");
//
//                    EditorGUI.BeginChangeCheck();
//                    PortalController.TargetController = (PortalController) EditorGUILayout.ObjectField(
//                        new GUIContent("Target Controller", "The targetTransform of this Portal."),
//                        PortalController.TargetController, typeof(PortalController), true, null);
//                    if (EditorGUI.EndChangeCheck())
//                        foreach (PortalController p in targets)
//                            p.TargetController = PortalController.TargetController;
//
//                    //if (!PortalController.PortalScript.PortalCamera ||
//                    //    !PortalController.TargetController.PortalScript.PortalCamera) return;
//
//                    EditorGUI.BeginChangeCheck();
//                    PortalController.Portal =
//                        (GameObject) EditorGUILayout.ObjectField(
//                            new GUIContent("Portal Prefab", "The Prefab to use for when the Portal is spawned"),
//                            PortalController.Portal, typeof(GameObject), false, null);
//                    if (EditorGUI.EndChangeCheck())
//                        foreach (PortalController p in targets)
//                            p.Portal = PortalController.Portal;
//
//
//                    if (SKSGlobalRenderSettings.ShouldOverrideMask)
//                        EditorGUILayout.HelpBox("Your Global Portal Settings are currently overriding the mask",
//                            MessageType.Warning);
//
//                    EditorGUI.BeginChangeCheck();
//                    PortalController.Mask = (Texture2D) EditorGUILayout.ObjectField(
//                        new GUIContent("Portal Mask", "The transparency mask to use on the Portal"),
//                        PortalController.Mask, typeof(Texture2D), false,
//                        GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
//
//                    if (EditorGUI.EndChangeCheck())
//                        foreach (PortalController p in targets)
//                            p.Mask = PortalController.Mask;
//
//                    EditorGUI.BeginChangeCheck();
//                    Material material =
//                        (Material) EditorGUILayout.ObjectField(
//                            new GUIContent("Portal Material", "The material to use for the Portal"),
//                            PortalController.PortalMaterial, typeof(Material), false, null);
//                    if (EditorGUI.EndChangeCheck()) {
//                        PortalMaterial = material;
//                        foreach (PortalController p in targets)
//                            p.PortalMaterial = PortalController.PortalMaterial;
//                    }
//
//
//                    EditorGUI.BeginChangeCheck();
//                    PortalController.Enterable =
//                        EditorGUILayout.Toggle(
//                            new GUIContent("Enterable", "Is the Portal Enterable by Teleportable Objects?"),
//                            PortalController.Enterable);
//                    if (EditorGUI.EndChangeCheck())
//                        foreach (PortalController p in targets)
//                            p.Enterable = PortalController.Enterable;
//
//                    EditorGUI.BeginChangeCheck();
//                    PortalController.Is3D =
//                        EditorGUILayout.Toggle(
//                            new GUIContent("Portal is 3D Object", "Is the Portal a 3d object, such as a Crystal ball?"),
//                            PortalController.Is3D);
//                    if (EditorGUI.EndChangeCheck())
//                        foreach (PortalController p in targets)
//                            p.Is3D = PortalController.Is3D;
//
//                    EditorGUI.BeginChangeCheck();
//                    PortalController.DetectionScale = EditorGUILayout.Slider(
//                        new GUIContent("Detection zone Scale", "The scale of the portal detection zone."),
//                        PortalController.DetectionScale, 1f, 10f);
//                    if (EditorGUI.EndChangeCheck())
//                        foreach (PortalController p in targets)
//                            p.DetectionScale = PortalController.DetectionScale;
//                    //Show the Portal Material Inspector
//                    if (Application.isPlaying)
//                        return;
//
//                }
//                catch {
//                    //Just for cleanliness
//                }
//                finally {
//                    if (!SKSGlobalRenderSettings.Preview) {
//                        //CleanupTemp();
//                    }
//                }
//            }else if (tab == 1) {
//                GUILayout.Label("Global settings:", EditorStyles.boldLabel);
//
//#if !SKS_VR
//                if (!GlobalPortalSettings.PlayerTeleportable)
//                {
//                    EditorGUILayout.HelpBox(
//                        "No PlayerTeleportable set. Seamless passthrough will not function. Add a PlayerTeleportable script to your teleportable player object.",
//                        MessageType.Warning);
//                }
//                else
//                {
//                    GUILayout.BeginHorizontal();
//                    EditorGUILayout.LabelField("Player Teleportable", new GUIStyle() { normal = new GUIStyleState() { textColor = Color.white } });
//                    EditorGUILayout.ObjectField(GlobalPortalSettings.PlayerTeleportable.gameObject, typeof(object), true);
//                    GUILayout.EndHorizontal();
//                }
//#endif
//                
//
//                GUILayout.Space(bumperSize);
//                if (_imageFoldout = EditorGUILayout.Foldout(_imageFoldout, "Image Settings", EditorStyles.foldout)) {
//                    EditorGUI.indentLevel = 2;
//
//                    GUILayout.BeginHorizontal();
//                    GUILayout.Space(EditorGUI.indentLevel * 10);
//#if SKS_VR
//                GUILayout.Label("Single Pass Stereo Rendering: " + SKSGlobalRenderSettings.SinglePassStereo);
//#endif
//                    GUILayout.EndHorizontal();
//
//
//                    GUI.enabled = !Application.isPlaying;
//
//                    //GUILayout.BeginHorizontal();
//                    // GUILayout.Space(EditorGUI.indentLevel * 5);
//#if SKS_VR
//                GUILayout.BeginHorizontal();
//                GUILayout.Space(EditorGUI.indentLevel * 10);
//                GUILayout.Label("Recursion in VR is very expensive. 3 is the typically acceptable max (prefer 0 if possible)");
//                GUILayout.EndHorizontal();
//#endif
//
//                    SKSGlobalRenderSettings.RecursionNumber = EditorGUILayout.IntSlider(
//                        new GUIContent("Recursion Number",
//                            "The number of times that Portals will draw through each other."),
//                        SKSGlobalRenderSettings.RecursionNumber, 0, 10);
//
//
//                    if (SKSGlobalRenderSettings.RecursionNumber > 1)
//                        EditorGUILayout.HelpBox(
//                            "Please be aware that recursion can get very expensive very quickly." +
//                            " Consider making this scale with the Quality setting of your game.",
//                            MessageType.Warning);
//
//
//
//                    GUI.enabled = true;
//
//
//                    GUILayout.BeginHorizontal();
//                    GUILayout.Space(EditorGUI.indentLevel * 10);
//                    SKSGlobalRenderSettings.AggressiveRecursionOptimization = GUILayout.Toggle(
//                        SKSGlobalRenderSettings.AggressiveRecursionOptimization,
//                        new GUIContent("Enable Aggressive Optimization for Recursion",
//                            "Aggressive optimization will halt recursive rendering immediately if the " +
//                            "source portal cannot raycast to the portal it is trying to render. Without " +
//                            "Occlusion Culling (due to lack of Unity Support), this is a lifesaver for " +
//                            "large scenes."));
//                    GUILayout.EndHorizontal();
//
//                    if (SKSGlobalRenderSettings.AggressiveRecursionOptimization)
//                        EditorGUILayout.HelpBox(
//                            "Enabling this option can save some serious performance," +
//                            " but it is possible for visual bugs to arise due to portals being partially" +
//                            "inside walls. If you are seeing black portals while " +
//                            "recursing, try turning this option off and see if it helps." +
//                            " If it does, then please" +
//                            "make sure that your portals are not inside walls.",
//                            MessageType.Warning);
//
//                    GUILayout.BeginHorizontal();
//                    GUILayout.Space(EditorGUI.indentLevel * 10);
//                    SKSGlobalRenderSettings.AdaptiveQuality = GUILayout.Toggle(SKSGlobalRenderSettings.AdaptiveQuality,
//                        new GUIContent("Enable Adaptive Quality Optimization for Recursion",
//                            "Adaptive quality rapidly degrades the quality of recursively " +
//                            "rendered portals. This is usually desirable."));
//                    GUILayout.EndHorizontal();
//
//                    GUILayout.BeginHorizontal();
//                    GUILayout.Space(EditorGUI.indentLevel * 10);
//                    SKSGlobalRenderSettings.Clipping = GUILayout.Toggle(SKSGlobalRenderSettings.Clipping,
//                        new GUIContent("Enable perfect object clipping",
//                            "Enable objects clipping as they enter portals. This is usually desirable."));
//                    GUILayout.EndHorizontal();
//
//                    GUILayout.BeginHorizontal();
//                    GUILayout.Space(EditorGUI.indentLevel * 10);
//                    SKSGlobalRenderSettings.ShouldOverrideMask = GUILayout.Toggle(
//                        SKSGlobalRenderSettings.ShouldOverrideMask,
//                        "Override Masks on all PortalSpawners");
//                    GUILayout.EndHorizontal();
//
//                    if (SKSGlobalRenderSettings.ShouldOverrideMask) {
//                        GUILayout.BeginHorizontal();
//                        GUILayout.Space(EditorGUI.indentLevel * 10);
//                        SKSGlobalRenderSettings.Mask =
//                            (Texture2D) EditorGUILayout.ObjectField(SKSGlobalRenderSettings.Mask, typeof(Texture2D),
//                                false);
//                        GUILayout.EndHorizontal();
//                    }
//
//                    GUILayout.BeginHorizontal();
//                    GUILayout.Space(EditorGUI.indentLevel * 10);
//                    SKSGlobalRenderSettings.CustomSkybox = GUILayout.Toggle(SKSGlobalRenderSettings.CustomSkybox,
//                        new GUIContent("Enable Skybox Override",
//                            "Enable custom skybox rendering. This is needed for skyboxes to not look strange through" +
//                            "SKSEffectCameras."));
//                    GUILayout.EndHorizontal();
//                    EditorGUI.indentLevel = 0;
//                }
//
//
//
//                GUILayout.Space(bumperSize);
//                if (_interactionFoldout =
//                    EditorGUILayout.Foldout(_interactionFoldout, "Interaction Settings", EditorStyles.foldout)) {
//                    EditorGUI.indentLevel = 2;
//
//                    GUILayout.BeginHorizontal();
//                    GUILayout.Space(EditorGUI.indentLevel * 10);
//                    SKSGlobalRenderSettings.PhysicsPassthrough = GUILayout.Toggle(
//                        SKSGlobalRenderSettings.PhysicsPassthrough,
//                        new GUIContent("Enable Physics Passthrough",
//                            "Enable collision with objects on the other side of portals"));
//                    GUILayout.EndHorizontal();
//
//
//                    if (SKSGlobalRenderSettings.PhysicsPassthrough)
//                        EditorGUILayout.HelpBox(
//                            "This setting enables interaction with objects on the other side of portals. " +
//                            "Objects can pass through portals without it, and it is not needed for most games. " +
//                            "In extreme cases, it can cause a slight performance hit.",
//                            MessageType.Info);
//
//                    GUILayout.BeginHorizontal();
//                    GUILayout.Space(EditorGUI.indentLevel * 10);
//                    SKSGlobalRenderSettings.PhysStyleB = GUILayout.Toggle(SKSGlobalRenderSettings.PhysStyleB,
//                        new GUIContent("Enable Physics Model B (More Accurate)",
//                            "Physics Model B maintains relative momentum between portals." +
//                            " This may or may not be desirable when the portals move."));
//                    GUILayout.EndHorizontal();
//
//                    GUILayout.BeginHorizontal();
//                    GUILayout.Space(EditorGUI.indentLevel * 10);
//                    SKSGlobalRenderSettings.NonScaledRenderers = GUILayout.Toggle(
//                        SKSGlobalRenderSettings.NonScaledRenderers,
//                        new GUIContent("Disable Portal scaling",
//                            "Disable portal scaling. This should be enabled if " +
//                            "portals are never used to change object's size."));
//                    GUILayout.EndHorizontal();
//
//                    EditorGUI.indentLevel = 0;
//                }
//
//                GUILayout.Space(bumperSize);
//                if (_editorFoldout = EditorGUILayout.Foldout(_editorFoldout, "Editor Settings", EditorStyles.foldout)) {
//                    EditorGUI.indentLevel = 2;
//
//                    GUILayout.BeginHorizontal();
//                    GUILayout.Space(EditorGUI.indentLevel * 10);
//                    SKSGlobalRenderSettings.Visualization = GUILayout.Toggle(SKSGlobalRenderSettings.Visualization,
//                        new GUIContent("Visualize Portal Connections",
//                            "Visualize all portal connections in the scene"));
//                    GUILayout.EndHorizontal();
//
//                    GUILayout.BeginHorizontal();
//                    GUILayout.Space(EditorGUI.indentLevel * 10);
//                    SKSGlobalRenderSettings.Gizmos = GUILayout.Toggle(SKSGlobalRenderSettings.Gizmos,
//                        new GUIContent("Draw Portal Gizmos",
//                            "Draw Portal Gizmos when selected in the Editor, " +
//                            "and when all portals are Visualized."));
//                    GUILayout.EndHorizontal();
//
//
//                    GUILayout.BeginHorizontal();
//                    GUILayout.Space(EditorGUI.indentLevel * 10);
//                    SKSGlobalRenderSettings.Preview = GUILayout.Toggle(SKSGlobalRenderSettings.Preview,
//                        new GUIContent("Draw Portal Previews (experimental)",
//                            "Draw Portal Previews when selected in the Editor." +
//                            " Experimental, and may only work with shallow viewing angles."));
//                    GUILayout.EndHorizontal();
//                    EditorGUI.indentLevel = 0;
//                }
//
//                GUILayout.Label("Something doesn't look right!/I'm getting errors!");
//
//                SKSGlobalRenderSettings.UvFlip = GUILayout.Toggle(SKSGlobalRenderSettings.UvFlip,
//                    "My portals are upside down!");
//
//                GUILayout.Label("Troubleshooting:");
//
//                string assetName = "PortalKit Pro";
//                string path = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
//                if (path == null)
//                    return;
//                path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));
//                string root = path.Substring(0, path.LastIndexOf(assetName) + (assetName.Length + 1));
//                string PDFPath = root + "README.pdf";
//
//                GUILayout.BeginHorizontal();
//                if (GUILayout.Button("Click here to open the manual")) {
//                    Application.OpenURL(PDFPath);
//                }
//                if (GUILayout.Button("Setup")) {
//                    SettingsWindow.Show();
//                }
//                GUILayout.EndHorizontal();
//            }
//            //Cache state of random
//            UnityEngine.Random.State seed = UnityEngine.Random.state;
//            //Make color deterministic based on ID
//            UnityEngine.Random.InitState(PortalController.GetInstanceID());
//            PortalController.color = UnityEngine.Random.ColorHSV(0, 1, 0.48f, 0.48f, 0.81f, 0.81f);
//            //Reset the random
//            UnityEngine.Random.state = seed;
//            try
//            {
//                if (PortalController.PortalMaterial)
//                    if (_matEditor == null)
//                        _matEditor = UnityEditor.Editor.CreateEditor(PortalController.PortalMaterial);
//
//
//                _matEditor.DrawHeader();
//                _matEditor.OnInspectorGUI();
//            }
//            catch { }
//
//        }
//
//        private void CleanupTemp()
//        {
//
//            if (PortalController)
//            {
//                MeshRenderer renderer = PortalController.GetComponent<MeshRenderer>();
//                if (renderer)
//                    renderer.enabled = true;
//            }
//
//            PortalController.CleanupTemp();
//        }
//    }
//}