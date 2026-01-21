/********************************************************************
生成日期:		11:03:2020
类    名: 	TargetPreview
作    者:	HappLI
描    述:	渲染物件查看、编辑窗口
*********************************************************************/
#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace Framework.ED
{
    public class PreviewRenderUtilityEx : PreviewRenderUtility
    {
        public PreviewRenderUtilityEx() : base()
        {

        }
        public PreviewRenderUtilityEx(bool renderFullScene) : base(renderFullScene)
        {

        }

#if (UNITY_5_6 || UNITY_5_1)
    Vector4 m_ambientColor;
    public Camera camera
    {
        get { return m_Camera; }
    }
    public float cameraFieldOfView
    {
        get { return m_CameraFieldOfView; }
        set
        {
            m_CameraFieldOfView = value;
        }
    }
    public Vector4 ambientColor
    {
        get { return m_ambientColor;  }
        set { m_ambientColor = value; }
    }
    public Light[] lights
    {
        get
        {
            return m_Light;
        }
    }
    public void AddSingleGO(GameObject go)
    {

    }
    public void Render(bool bAllowScriptableRenderPipline, bool updateFov)
    {
        camera.Render(); 
    }
#endif
    }
    class InnerHandlerControllHashID
    {
        static HashSet<int> ms_InnerControllHash = null;
        #region inner_controll_hash
        static void CheckIDs()
        {
            if (ms_InnerControllHash == null)
            {
                ms_InnerControllHash = new HashSet<int>();
                ms_InnerControllHash.Add(GUIUtility.GetControlID("SliderHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("Slider2DHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("FreeRotateHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("RadiusHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("xAxisFreeMoveHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("yAxisFreeMoveHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("zAxisFreeMoveHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("FreeMoveHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("xzAxisFreeMoveHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("xyAxisFreeMoveHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("yzAxisFreeMoveHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("xAxisScaleHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("yAxisScaleHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("zAxisScaleHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("ScaleSliderHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("ScaleValueHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("DiscHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("ButtonHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("xRotateHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("yRotateHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("zRotateHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("cameraAxisRotateHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("xyzRotateHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("xScaleHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("yScaleHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("zScaleHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("xyzScaleHandleHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("TransformTranslationXHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("TransformTranslationYHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("TransformTranslationZHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("TransformTranslationXYHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("TransformTranslationXZHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("TransformTranslationYZHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("TransformTranslationXYZHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("TransformRotationXHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("TransformRotationYHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("TransformRotationZHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("TransformRotationCameraAxisHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("TransformRotationXYZHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("TransformScaleXHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("TransformScaleYHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("TransformScaleZHash".GetHashCode(), FocusType.Passive));
                ms_InnerControllHash.Add(GUIUtility.GetControlID("TransformScaleXYZHash".GetHashCode(), FocusType.Passive));
            }
        }
#endregion
        public static bool HasID(int id)
        {
            CheckIDs();
            return ms_InnerControllHash.Contains(id);
        }
        public static int TypeForControll(Event evt)
        {
            CheckIDs();
            foreach(var db in ms_InnerControllHash)
            {
                var type = evt.GetTypeForControl(db);
                if (type == EventType.MouseDown || type == EventType.MouseUp || type == EventType.MouseDrag || type == EventType.MouseMove)
                {
                    return db;
                }
            }
            return 0;
        }
    }

    // Disable it
    //[CustomPreview(typeof(GameObject))]
    public class TargetPreview : ObjectPreview
    {
        private class Styles
        {
            public GUIContent speedScale = IconContent("SpeedScale", "Changes particle preview speed");
            public GUIContent pivot = IconContent("AvatarPivot", "Displays avatar's pivot and mass center");
            public GUIContent[] play = new GUIContent[2]
            {
            IconContent("preAudioPlayOff", "Play"),
            IconContent("preAudioPlayOn", "Stop")
            };
            public GUIContent lockParticleSystem = IconContent("IN LockButton", "Lock the current particle");
            public GUIContent reload = new GUIContent("Reload", "Reload particle preview");
            public GUIStyle preButton = "preButton";
            public GUIStyle preSlider = "preSlider";
            public GUIStyle preSliderThumb = "preSliderThumb";
            public GUIStyle preLabel = "preLabel";
        }
        //-----------------------------------------------------
        protected enum ViewTool
        {
            None,
            Move,
            Scale,
            Rotate,
        }

        Rect m_Rect;
        public Rect viewRect
        {
            get
            {
                return m_Rect;
            }
        }

        public Action<int, Camera, Event> OnDrawBeforeCB = null;
        public Action<int, Camera, Event> OnDrawAfterCB = null;
        public Action<Ray, Vector3, Event> OnMoseMoveCB = null;
        public Action<Ray, Vector3, Event> OnMoseHitCB = null;
        public Action<Ray, Vector3, Event> OnMouseDownCB = null;
        public Action<Ray, Vector3, Event> OnMosueUpCB = null;
        public Action<GameObject> OnChangeTarget = null;
        public Action<MonoBehaviour> OnSetCameraShake = null;
        public Action<MonoBehaviour> OnSetCameraCloseUp = null;
        public Action OnPreviewTargetDestroy = null;
        public Action<System.Object, bool> OnPreviewStatePlay = null;
        public Action OnPreviewStateStop = null;
        public Predicate<System.Object> CheckCanMouseDrag = null;

        public delegate Vector3 OnGetCameraTargetPos();
        public OnGetCameraTargetPos onGetTargetPosition = null;

        public bool CanKeyMoveCamera = true;

#if USE_CAMERA_MONO
        private CameraCloseUp m_pCameraCloseUp;
        private CameraShake m_pCameraShake;
#endif

        private PreviewRenderUtilityEx m_PreviewUtility;

        private GameObject m_PreviewInstance;
        private Dictionary<Transform, bool> m_vSkeleton = new Dictionary<Transform, bool>();

        private GameObject m_ReferenceInstance;
        private GameObject m_DirectionInstance;
        private GameObject m_PivotInstance;

        private Vector3 m_vMouseHitPos = Vector3.zero;

        private Vector3 m_DirectionPos = Vector3.zero;
        public Vector3 diectionPos
        {
            set { m_DirectionPos = value; }
        }

        public bool bUpdateCameraTransform = true;
        public bool bLeftMouseForbidMove = false;
        public bool bLeftMouseForbidRotate = false;
        public bool bZoomAvatarScale = true;
        public bool bCanControllCamera = true;
        public bool bBreakEvent = true;
        public EditorWindow pOwnerWindow;

        private Vector2 m_MouseFirstPress = Vector2.zero;

        private GameObject m_RootInstance;
        private bool m_bShowSkeleton = false;
        private Vector2 m_PreviewDir = new Vector2(120f, -20f);
        private Mesh m_FloorPlane;
        private Texture2D m_FloorTexture;
        private Material m_FloorMaterial;
        private float m_AvatarScale = 1f;
        private float m_ZoomFactor = 1f;
        private float m_fCameraNear = 0.5f;
        private float m_fCameraFar = 100f;
        private float m_fFov = 30f;
        private Vector3 m_PivotPositionOffset = Vector3.zero;
        private float m_BoundingVolumeScale;
        public MouseCursor eMouseCursor = MouseCursor.Arrow;
        protected ViewTool m_ViewTool = ViewTool.None;
        private bool m_ShowReference;
        private bool m_Loaded;
        private int m_PreviewHint = "Preview".GetHashCode();
        private int m_PreviewSceneHint = "PreviewSene".GetHashCode();

        private bool m_Playing;
        private float m_PlaybackSpeed = 1f;
        public float fDeltaTime;

        private bool m_HasPreview;
        private Editor m_CacheEditor;
        public static int PreviewCullingLayer = 0;//31;
        private static Styles s_Styles;

        GUIStyle m_defaultStyle = null;

        private float m_bShowFloor = 0.5f;
        public float showFloor
        {
            get { return m_bShowFloor; }
            set { m_bShowFloor = value; }
        }
        //-----------------------------------------------------
        public float avatarScale
        {
            get { return m_AvatarScale; }
        }
        //-----------------------------------------------------
        public bool showSkeleton
        {
            get { return m_bShowSkeleton; }
            set { m_bShowSkeleton = value; }
        }
        //-----------------------------------------------------
        public Vector3 bodyPosition
        {
            get
            {
                if (onGetTargetPosition != null) return onGetTargetPosition();
                return (m_PreviewInstance != null) ? m_PreviewInstance.transform.position : Vector3.zero;
            }
        }
        //-----------------------------------------------------
        public GameObject roleObject
        {
            get { return m_PreviewInstance; }
        }
        //-----------------------------------------------------
        public TargetPreview()
        {
            pOwnerWindow = null;
        }
        //-----------------------------------------------------
        public TargetPreview(EditorWindow window)
        {
            pOwnerWindow = window;
        }
        //-----------------------------------------------------
        ~TargetPreview()
        {
            Destroy();
        }
        //-----------------------------------------------------
        public void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material mat, int subMeshIndex)
        {
            if (m_PreviewUtility != null)
                m_PreviewUtility.DrawMesh(mesh, matrix, mat, subMeshIndex);
        }
        //-----------------------------------------------------
        protected ViewTool viewTool
        {
            get
            {
                Event current = Event.current;
                if (m_ViewTool == ViewTool.None)
                {
                    bool flag = current.control && Application.platform == RuntimePlatform.OSXEditor;
                    bool actionKey = EditorGUI.actionKey;
                    bool flag2 = !actionKey && !flag && !current.alt;

                    if ((((current.button <= 0 && flag2) || (current.button <= 0 && actionKey))) || current.button == 2)
                    {
                        m_ViewTool = ViewTool.Move;
                    }
                    else
                    {
                        if ((current.button <= 0 && flag) || (current.button == 1 && current.alt))
                        {
                            m_ViewTool = ViewTool.Scale;
                        }
                        else
                        {
                            if ((current.button <= 0 && current.alt) || current.button == 1)
                            {
                                m_ViewTool = ViewTool.Rotate;
                            }
                        }
                    }
                }
                return m_ViewTool;
            }
        }
        //-----------------------------------------------------
        protected MouseCursor currentCursor
        {
            get
            {
                if (eMouseCursor != MouseCursor.Arrow) return eMouseCursor;
                switch (m_ViewTool)
                {
                    case ViewTool.Move:
                        return MouseCursor.Pan;
                    case ViewTool.Scale:
                        return MouseCursor.Zoom;
                    case ViewTool.Rotate:
                        return MouseCursor.Orbit;
                    default:
                        return MouseCursor.Arrow;
                }
            }
        }
        //-----------------------------------------------------
        public void SetCamera(float fNear, float fFar, float fov)
        {
            m_fCameraNear = fNear;
            m_fCameraFar = fFar;
            m_fFov = fov;
        }
        //-----------------------------------------------------
        public Camera GetCamera()
        {
            if (m_PreviewUtility == null) return null;
            return m_PreviewUtility.camera;
        }
        //-----------------------------------------------------
        public float GetCameraFov()
        {
            return m_fFov;
        }
        //-----------------------------------------------------
        public void SetLookatAndEulerAngle(Vector3 lookatPos, Vector3 eulerAngle, float fDistance, float zoomFactor = 0)
        {
            Vector3 dir = Quaternion.Euler(eulerAngle) * Vector3.forward;
            Vector3 pos = lookatPos - dir * fDistance;
            m_PreviewDir.y = -eulerAngle.x;
            m_PreviewDir.x = -eulerAngle.y;
            Quaternion rotation = Quaternion.Euler(-m_PreviewDir.y, -m_PreviewDir.x, 0f);
            if (zoomFactor != 0) m_ZoomFactor = zoomFactor;
            m_PivotPositionOffset = pos - (rotation * (Vector3.forward * -5.5f * m_ZoomFactor) + bodyPosition);
            Vector3 position2 = rotation * (Vector3.forward * -5.5f * m_ZoomFactor) + bodyPosition + m_PivotPositionOffset;
            m_PreviewUtility.camera.transform.position = position2;
            m_PreviewUtility.camera.transform.rotation = rotation;
        }
        //-----------------------------------------------------
        public void SetCameraPositionAndEulerAngle(Vector3 pos, Vector3 eulerAngle, float zoomFactor = 0)
        {
            m_PreviewDir.y = -eulerAngle.x;
            m_PreviewDir.x = -eulerAngle.y;
            Quaternion rotation = Quaternion.Euler(-m_PreviewDir.y, -m_PreviewDir.x, 0f);
            if (zoomFactor != 0) m_ZoomFactor = zoomFactor;
            m_PivotPositionOffset = pos - (rotation * (Vector3.forward * -5.5f * m_ZoomFactor) + bodyPosition);
            Vector3 position2 = rotation * (Vector3.forward * -5.5f * m_ZoomFactor) + bodyPosition + m_PivotPositionOffset;
            m_PreviewUtility.camera.transform.position = position2;
            m_PreviewUtility.camera.transform.rotation = rotation;
        }
        //-----------------------------------------------------
        public void SetCameraFov(float fov)
        {
            m_fFov = fov;
            if (m_PreviewUtility != null)
                m_PreviewUtility.cameraFieldOfView = m_fFov;
        }
        //-----------------------------------------------------
        public void InitCameraPos(Vector3 pos, Vector3 dir)
        {
            if (m_PreviewInstance != null) m_PreviewInstance.transform.position = pos;
            if (m_PreviewUtility != null)
            {
                m_PreviewUtility.camera.transform.forward = dir;
                m_PivotPositionOffset = -m_PreviewUtility.camera.transform.forward * 10;
            }
        }
        //-----------------------------------------------------
        public void SetEditor(Editor editor)
        {
            m_CacheEditor = editor;
        }
        //-----------------------------------------------------
        public override void Initialize(UnityEngine.Object[] targets)
        {
            base.Initialize(targets);

            if (m_CacheEditor == null)
            {
                var editors = ActiveEditorTracker.sharedTracker.activeEditors;
                foreach (var editor in editors)
                {
                    if (editor == null) continue;
                    if (editor.target == target)
                    {
                        m_CacheEditor = editor;
                        break;
                    }
                }
                if (m_CacheEditor == null && editors.Length > 0)
                {
                    m_CacheEditor = editors[0];
                }
            }

            m_HasPreview = EditorUtility.IsPersistent(target) && HasStaticPreview();
            if (m_Targets.Length != 1)
            {
                m_HasPreview = false;
            }

            InitPreview();
        }
        //-----------------------------------------------------
        public override bool HasPreviewGUI()
        {
            return m_HasPreview;
        }
        //-----------------------------------------------------
        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            m_Rect = r;
            InitPreview();
            if (m_PreviewUtility == null)
            {
                return;
            }

            if (m_defaultStyle == null)
                m_defaultStyle = new GUIStyle(EditorStyles.textField);

            if (background == null)
                background = m_defaultStyle;

            int preKeyControll = GUIUtility.keyboardControl;
            int preHotControll = GUIUtility.hotControl;
            //             GUIUtility.hotControl = preHotControll;
            //             GUIUtility.keyboardControl = preKeyControll;

            Rect rect2 = r;
            int controlID = GUIUtility.GetControlID(m_PreviewHint, FocusType.Keyboard, rect2);
            Event current = Event.current;
            EventType typeForControl = current.GetTypeForControl(controlID);

            if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)
            {
                m_ViewTool = ViewTool.None;
            }

            m_PreviewUtility.BeginPreview(rect2, background);
               
            DoRenderPreview(controlID, current);

            int controlID2 = GUIUtility.GetControlID(m_PreviewSceneHint, FocusType.Passive);
            Handles.SetCamera(GetCamera());
            typeForControl = current.GetTypeForControl(controlID2);
            HandleViewTool(current, typeForControl, controlID2, rect2);
            HandleMouseMove(current, controlID2, rect2);
            DoAvatarPreviewFrame(current, typeForControl, rect2);
            m_PreviewUtility.EndAndDrawPreview(rect2);

            if (current.type == EventType.Repaint)
            {
                EditorGUIUtility.AddCursorRect(rect2, currentCursor);
            }
        }
        //-----------------------------------------------------
        public override GUIContent GetPreviewTitle()
        {
            GUIContent content = base.GetPreviewTitle();
            content.text = "Particle Preview";
            return content;
        }
        //-----------------------------------------------------
        public override void OnPreviewSettings()
        {
            if (s_Styles == null)
            {
                s_Styles = new Styles();
            }

            InitPreview();
            if (m_PreviewUtility == null)
            {
                if (GUILayout.Button(s_Styles.reload, s_Styles.preButton))
                {
                    m_Loaded = false;
                }
                return;
            }

            EditorGUI.BeginChangeCheck();
            m_ShowReference = GUILayout.Toggle(m_ShowReference, s_Styles.pivot, s_Styles.preButton);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool("AvatarpreviewShowReference", m_ShowReference);
            }

            bool flag = CycleButton(!m_Playing ? 0 : 1, s_Styles.play, s_Styles.preButton) != 0;

            GUILayout.Box(s_Styles.speedScale, s_Styles.preLabel);
            EditorGUI.BeginChangeCheck();
            m_PlaybackSpeed = PreviewSlider(m_PlaybackSpeed, 0.03f);
            if (EditorGUI.EndChangeCheck() && m_PreviewInstance)
            {
                ParticleSystem[] particleSystems = m_PreviewInstance.GetComponentsInChildren<ParticleSystem>(true);
                foreach (var particleSystem in particleSystems)
                {
#if UNITY_5_5_OR_NEWER
                    ParticleSystem.MainModule main = particleSystem.main;
                    main.simulationSpeed = m_PlaybackSpeed;
#else
                particleSystem.playbackSpeed = m_PlaybackSpeed;
#endif
                }
            }
            GUILayout.Label(m_PlaybackSpeed.ToString("f2"), s_Styles.preLabel);
        }
        //-----------------------------------------------------
        public override void ReloadPreviewInstances()
        {
            Debug.Log("reload");
            if (m_PreviewUtility == null)
            {
                return;
            }
            CreatePreviewInstances();
        }
        //-----------------------------------------------------
        public override string GetInfoString()
        {
            return " ";
        }
        //-----------------------------------------------------
        private void InitPreview()
        {
            if (m_Loaded)
            {
                return;
            }

            m_Loaded = true;

            if (m_PreviewUtility == null)
            {
                m_PreviewUtility = new PreviewRenderUtilityEx(true);

#if USE_CAMERA_MONO
                m_pCameraCloseUp = m_PreviewUtility.camera.gameObject.GetComponent<CameraCloseUp>();
                if (m_pCameraCloseUp == null)
                {
                    m_pCameraCloseUp = m_PreviewUtility.camera.gameObject.AddComponent<CameraCloseUp>();
                }
                m_pCameraShake = m_PreviewUtility.camera.gameObject.GetComponent<CameraShake>();
                if (m_pCameraShake == null)
                {
                    m_pCameraShake = m_PreviewUtility.camera.gameObject.AddComponent<CameraShake>();
                }

                if (OnSetCameraCloseUp != null) OnSetCameraCloseUp(m_pCameraCloseUp);
                if (OnSetCameraShake != null) OnSetCameraShake(m_pCameraShake);
#endif

                m_PreviewUtility.cameraFieldOfView = m_fFov;
                int cull = 0;
                for (int i = 0; i < 32; ++i) cull |= (1 << i);
                m_PreviewUtility.camera.cullingMask = cull;// 1 << PreviewCullingLayer;
                m_PreviewUtility.camera.useOcclusionCulling = false;

                this.m_PreviewUtility.camera.allowHDR = false;
                this.m_PreviewUtility.camera.allowMSAA = false;
                m_PreviewUtility.ambientColor = new Color(1, 1, 1, 1);

                CreatePreviewInstances();
            }
            if (m_FloorPlane == null)
            {
                m_FloorPlane = (Resources.GetBuiltinResource(typeof(Mesh), "New-Plane.fbx") as Mesh);
            }
            if (m_FloorTexture == null)
            {
                m_FloorTexture = (Texture2D)EditorGUIUtility.Load("Avatar/Textures/AvatarFloor.png");
            }
            if (m_FloorMaterial == null)
            {
                Shader shader;
                if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline!=null)
                {
                    shader = Shader.Find("Hidden/Editor/PreviewPlaneWithShadowURP");
                    if (shader == null)
                        shader = EditorGUIUtility.LoadRequired("PreviewPlaneWithShadowURP.shader") as Shader;
                }
                else
                {
                    shader = Shader.Find("Hidden/Preview Plane With Shadow");
                    if (shader == null)
                        shader = EditorGUIUtility.LoadRequired("Previews/PreviewPlaneWithShadow.shader") as Shader;
                }
                m_FloorMaterial = new Material(shader);
                m_FloorMaterial.mainTexture = m_FloorTexture;
                m_FloorMaterial.mainTextureScale = Vector2.one * 5f * 4f;
                m_FloorMaterial.SetVector("_Alphas", new Vector4(0.5f, 0.3f, 0f, 0f));
                m_FloorMaterial.hideFlags = HideFlags.HideAndDontSave;
                m_FloorMaterial.renderQueue = 1000;
            }
            

            if (m_ReferenceInstance == null)
            {
                GameObject original = (GameObject)EditorGUIUtility.Load("Avatar/dial_flat.prefab");
                m_ReferenceInstance = (GameObject)UnityEngine.Object.Instantiate(original, Vector3.zero, Quaternion.identity);
                AddPreview(m_ReferenceInstance);
            }
            if (m_DirectionInstance == null)
            {
                GameObject original2 = (GameObject)EditorGUIUtility.Load("Avatar/arrow.fbx");
                m_DirectionInstance = (GameObject)UnityEngine.Object.Instantiate(original2, Vector3.zero, Quaternion.identity);
                AddPreview(m_DirectionInstance);
            }
            if (m_PivotInstance == null)
            {
                GameObject original3 = (GameObject)EditorGUIUtility.Load("Avatar/root.fbx");
                m_PivotInstance = (GameObject)UnityEngine.Object.Instantiate(original3, Vector3.zero, Quaternion.identity);
                AddPreview(m_PivotInstance);
            }
            if (m_RootInstance == null)
            {
                GameObject original4 = (GameObject)EditorGUIUtility.Load("Avatar/root.fbx");
                m_RootInstance = (GameObject)UnityEngine.Object.Instantiate(original4, Vector3.zero, Quaternion.identity);
                AddPreview(m_RootInstance);
            }
            m_ShowReference = EditorPrefs.GetBool("AvatarpreviewShowReference", true);
            SetPreviewCharacterEnabled(false, false);
        }
        //-----------------------------------------------------
        private bool HasStaticPreview()
        {
            if (target == null)
            {
                return false;
            }
            GameObject gameObject = target as GameObject;
#if UNITY_5_1
         return gameObject.GetComponentsInChildren(typeof(ParticleSystem)).Length>0;
#else
            return gameObject.GetComponentInChildren<ParticleSystem>(true);
#endif
        }
        //-----------------------------------------------------
        private void DoRenderPreview(int contollerId, Event evt)
        {

            //   Color backgroundColor = m_PreviewUtility.camera.backgroundColor;
            //  m_PreviewUtility.camera.backgroundColor = new Color(0f, 0f, 0f, 0f);

            bool oldFog = SetupPreviewLightingAndFx();
            if (bUpdateCameraTransform)
            {
                Vector3 bodyPosition = this.bodyPosition;
                Quaternion quaternion = Quaternion.identity;
                Vector3 vector = Vector3.zero;
                Quaternion quaternion2 = Quaternion.identity;
                Vector3 pivotPos = Vector3.zero;

                Vector3 forward = quaternion2 * Vector3.forward;
                forward[1] = 0f;
                Quaternion directionRot = Quaternion.LookRotation(forward);
                Vector3 directionPos = m_DirectionPos;
                Quaternion pivotRot = quaternion;
                PositionPreviewObjects(pivotRot, pivotPos, quaternion2, bodyPosition, directionRot, quaternion, vector, directionPos, m_AvatarScale);

                Vector3 vShakeOffset = Vector3.zero;
#if USE_CAMERA_MONO
            if (m_pCameraShake != null)
            {
                vShakeOffset = m_pCameraShake.GetShakeOffset();
            }
            if (m_pCameraCloseUp != null && m_pCameraCloseUp.isCloseUp())
            {

                m_pCameraCloseUp.GetCloseUpParameter(m_pCameraCloseUp.transform, fDeltaTime);

                // Quaternion rotation = Quaternion.Euler(-m_PreviewDir.y, -m_PreviewDir.x, 0f);
                // Vector3 position2 = vCloseUpTrans + vShakeOffset;
                // m_PreviewUtility.camera.transform.position = position2;
                // m_PreviewUtility.camera.transform.eulerAngles = vCloseUpDir;
            }
            else
            {
                Quaternion rotation = Quaternion.Euler(-m_PreviewDir.y, -m_PreviewDir.x, 0f);
                Vector3 position2 = rotation * (Vector3.forward * -5.5f * m_ZoomFactor) + bodyPosition + m_PivotPositionOffset + vShakeOffset;
                m_PreviewUtility.camera.transform.position = position2;
                m_PreviewUtility.camera.transform.rotation = rotation;
            }
#else
                Quaternion rotation = Quaternion.Euler(-m_PreviewDir.y, -m_PreviewDir.x, 0f);
                Vector3 position2 = rotation * (Vector3.forward * -5.5f * m_ZoomFactor) + bodyPosition + m_PivotPositionOffset + vShakeOffset;
                m_PreviewUtility.camera.transform.position = position2;
                m_PreviewUtility.camera.transform.rotation = rotation;
#endif

            }

            m_PreviewUtility.camera.nearClipPlane = m_fCameraNear * m_ZoomFactor;
            m_PreviewUtility.camera.farClipPlane = m_fCameraFar * m_AvatarScale;
            if(m_PreviewUtility.camera.orthographic)
            {
                m_PreviewUtility.camera.orthographicSize = m_ZoomFactor;
            }


            Quaternion identity = Quaternion.identity;
            Vector3 position = new Vector3(0f, 0f, 0f);
            if (m_ReferenceInstance != null) position = m_ReferenceInstance.transform.position;
            if(m_FloorPlane)
            {
                Material floorMaterial = m_FloorMaterial;
                if (floorMaterial != null)
                {
                    floorMaterial.renderQueue = 1000;
                    Matrix4x4 matrix2 = Matrix4x4.TRS(position, identity, Vector3.one * 5f * m_AvatarScale);
                    floorMaterial.mainTextureOffset = -new Vector2(position.x, position.z) * 5f * 0.08f * (1f / m_AvatarScale);
                    floorMaterial.SetVector("_Alphas", new Vector4(0.5f * 1f, 0.3f * 1f, 0f, 0f) * m_bShowFloor);
                    Graphics.DrawMesh(m_FloorPlane, matrix2, floorMaterial, PreviewCullingLayer, m_PreviewUtility.camera, 0);
                }
            }


            SetPreviewCharacterEnabled(true, m_ShowReference);

            Matrix4x4 oldMatrix = Handles.matrix;
            Handles.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
            Vector2 mousePos = evt.mousePosition;

            Rect screenRect = EditorGUIUtility.GUIToScreenRect(m_Rect);
            if (pOwnerWindow != null)
            {
                Vector2 mouseOffset = new Vector2(screenRect.xMin, 0f) - new Vector2(pOwnerWindow.position.xMin, 0f);
                evt.mousePosition -= mouseOffset;
            }

            UnityEngine.Rendering.CompareFunction zTest = Handles.zTest;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            OnDrawBefore(contollerId, evt);
            Handles.zTest = zTest;

            this.m_PreviewUtility.Render(true, true);

            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            OnDrawAfter(contollerId, evt);
            Handles.zTest = zTest;
            SetPreviewCharacterEnabled(false, false);
            TeardownPreviewLightingAndFx(oldFog);

            evt.mousePosition = mousePos;
            //  m_PreviewUtility.camera.backgroundColor = backgroundColor;
        }
        //-----------------------------------------------------
        private void OnDrawBefore(int controllerId, Event evt)
        {
            Handles.SetCamera(m_PreviewUtility.camera);

            if (OnDrawBeforeCB != null)
                OnDrawBeforeCB(controllerId, m_PreviewUtility.camera, evt);
        }
        //-----------------------------------------------------
        private void OnDrawAfter(int controllerId, Event evt)
        {
            Handles.SetCamera(m_PreviewUtility.camera);

            if (OnDrawAfterCB != null)
                OnDrawAfterCB(controllerId, m_PreviewUtility.camera, evt);
        }
        //-----------------------------------------------------
        public void SetPreviewInstance(GameObject gameObject, bool bDestroy = true)
        {
            if (bDestroy)
                DestroyPreviewInstances();
            AddPreview(gameObject);
            Animator component = gameObject.GetComponent<Animator>();
            if (component)
            {
                m_vSkeleton.Clear();
                GetTransformBone(ref m_vSkeleton, gameObject.transform);
            }
            m_PreviewInstance = gameObject;
            gameObject.transform.position = Vector3.zero;
            gameObject.transform.eulerAngles = Vector3.zero;

            //Debug.Log("OnCreate");

            if (OnChangeTarget != null)
            {
                OnChangeTarget(m_PreviewInstance);
            }

            Bounds bounds = new Bounds(m_PreviewInstance.transform.position, Vector3.zero);
            GetRenderableBoundsRecurse(ref bounds, m_PreviewInstance);
            if (bounds.size == Vector3.zero)
            {
                bounds.size = Vector3.one;
            }

            m_BoundingVolumeScale = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            if (!bZoomAvatarScale)
                m_AvatarScale = 1f;
            else
                m_AvatarScale = (m_ZoomFactor = m_BoundingVolumeScale / 2f);
        }
        //-----------------------------------------------------
        private void CreatePreviewInstances()
        {
            return;
            if (m_Targets == null || m_Targets.Length <= 0) return;
            DestroyPreviewInstances();

            GameObject gameObject = UnityEngine.Object.Instantiate(target) as GameObject;
            SetPreviewInstance(gameObject);
        }
        //-----------------------------------------------------
        public void ClearInstanceTarget()
        {
            if (m_PreviewInstance == null) return;
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(m_PreviewInstance);
            else
                UnityEngine.Object.DestroyImmediate(m_PreviewInstance);
            m_PreviewInstance = null;
        }
        //-----------------------------------------------------
        private void DestroyPreviewInstances()
        {
            if (m_PreviewInstance == null)
            {
                return;
            }
            m_vSkeleton.Clear();
            if(Application.isPlaying)
            {
                UnityEngine.Object.Destroy(m_PreviewInstance);
                UnityEngine.Object.Destroy(m_FloorMaterial);
                UnityEngine.Object.Destroy(m_ReferenceInstance);
                UnityEngine.Object.Destroy(m_RootInstance);
                UnityEngine.Object.Destroy(m_PivotInstance);
                UnityEngine.Object.Destroy(m_DirectionInstance);

            }
            else
            {
                UnityEngine.Object.DestroyImmediate(m_PreviewInstance);
                UnityEngine.Object.DestroyImmediate(m_FloorMaterial);
                UnityEngine.Object.DestroyImmediate(m_ReferenceInstance);
                UnityEngine.Object.DestroyImmediate(m_RootInstance);
                UnityEngine.Object.DestroyImmediate(m_PivotInstance);
                UnityEngine.Object.DestroyImmediate(m_DirectionInstance);
            }


            if (OnPreviewTargetDestroy != null)
                OnPreviewTargetDestroy();
            m_PreviewInstance = null;
        }
        //-----------------------------------------------------
        private bool SetupPreviewLightingAndFx()
        {
            m_PreviewUtility.lights[0].intensity = 1.4f;
            m_PreviewUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0f);
            m_PreviewUtility.lights[1].intensity = 1.4f;

            Color ambient = new Color(0.6f, 0.6f, 0.6f, 1f);
            InternalEditorUtility.SetCustomLighting(m_PreviewUtility.lights, ambient);
            //  InternalEditorUtility.SetCustomLightingInternal(m_PreviewUtility.lights, ambient); 
#if (UNITY_5_6 || UNITY_5_1)
       
       
        m_PreviewUtility.ambientColor = ambient;
#else
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Custom;
            RenderSettings.ambientLight = new Color(0.81f, 0.81f, 0.81f, 1f);
            RenderSettings.ambientIntensity = 2f;
            RenderSettings.ambientGroundColor = new Color(0.81f, 0.81f, 0.81f, 1f);
            RenderSettings.ambientProbe = RenderSettings.ambientProbe;
#endif

            bool fog = RenderSettings.fog;
            Unsupported.SetRenderSettingsUseFogNoDirty(false);
            return fog;
        }
        //-----------------------------------------------------
        public void SetLightIntensity(float intensity = 1.4f)
        {
            if (m_PreviewUtility != null && m_PreviewUtility.lights != null && m_PreviewUtility.lights.Length >= 2)
            {
                m_PreviewUtility.lights[0].intensity = intensity;
                m_PreviewUtility.lights[1].intensity = intensity;
            }
        }
        //-----------------------------------------------------
        public void SetLightEulerAngle(Vector3 eulerAngle,int index= -1)
        {
            if (m_PreviewUtility != null && m_PreviewUtility.lights != null)
            {
                if (index < 0)
                {
                    for (int i = 0; i < m_PreviewUtility.lights.Length; ++i)
                    {
                        m_PreviewUtility.lights[i].transform.eulerAngles = eulerAngle;
                    }
                }
                else if (index < m_PreviewUtility.lights.Length)
                {
                    m_PreviewUtility.lights[index].transform.eulerAngles = eulerAngle;
                }
            }
        }
        //-----------------------------------------------------
        private static void TeardownPreviewLightingAndFx(bool oldFog)
        {
            Unsupported.SetRenderSettingsUseFogNoDirty(oldFog);
            InternalEditorUtility.RemoveCustomLighting();
        }
        //-----------------------------------------------------
        private void SetPreviewCharacterEnabled(bool enabled, bool showReference)
        {
            if (m_PreviewInstance != null)
            {
                SetEnabledRecursive(m_PreviewInstance, enabled);
            }
            SetEnabledRecursive(m_ReferenceInstance, showReference && enabled);
            SetEnabledRecursive(m_DirectionInstance, showReference && enabled);
            SetEnabledRecursive(m_PivotInstance, showReference && enabled);
            SetEnabledRecursive(m_RootInstance, showReference && enabled);
        }
        //-----------------------------------------------------
        private void PositionPreviewObjects(Quaternion pivotRot, Vector3 pivotPos, Quaternion bodyRot, Vector3 bodyPos,
            Quaternion directionRot, Quaternion rootRot, Vector3 rootPos, Vector3 directionPos, float scale)
        {
            if (m_ReferenceInstance != null)
            {
                m_ReferenceInstance.transform.position = rootPos;
                m_ReferenceInstance.transform.rotation = rootRot;
                m_ReferenceInstance.transform.localScale = Vector3.one * scale * 1.25f;
            }
            if (m_DirectionInstance != null)
            {
                m_DirectionInstance.transform.position = directionPos;
                m_DirectionInstance.transform.rotation = directionRot;
                m_DirectionInstance.transform.localScale = Vector3.one * scale * 2f;
            }
            if (m_PivotInstance != null)
            {
                m_PivotInstance.transform.position = pivotPos;
                m_PivotInstance.transform.rotation = pivotRot;
                m_PivotInstance.transform.localScale = Vector3.one * scale * 0.1f;
            }

            if (m_RootInstance != null)
            {
                m_RootInstance.transform.position = bodyPos;
                m_RootInstance.transform.rotation = bodyRot;
                m_RootInstance.transform.localScale = Vector3.one * scale * 0.25f;
            }
        }
        //-----------------------------------------------------
        public void Destroy()
        {
            DestroyPreviewInstances();
            if (m_PreviewUtility != null)
            {
#if USE_CAMERA_MONO
                if (m_pCameraShake) UnityEngine.Object.DestroyImmediate(m_pCameraShake);
                if (m_pCameraCloseUp) UnityEngine.Object.DestroyImmediate(m_pCameraCloseUp);
#endif
                m_PreviewUtility.Cleanup();
                m_PreviewUtility = null;

                base.Cleanup();
            }
        }
        //-----------------------------------------------------
        public void Update(float fFrameTime)
        {
#if USE_CAMERA_MONO
            if (m_pCameraShake != null)
            {
                m_pCameraShake.ForceUpdate(fFrameTime);
            }
#endif
        }
        //-----------------------------------------------------
        public void Play(System.Object state, bool bPlaying)
        {
            if (OnPreviewStatePlay != null)
            {
                OnPreviewStatePlay(state, bPlaying);
                Repaint();
            }
        }
        //-----------------------------------------------------
        public void Stop()
        {
            if (OnPreviewStateStop != null)
                OnPreviewStateStop();
            Repaint();
        }
        //----------------------------------------------------
        private void Repaint()
        {
            if (m_CacheEditor)
            {
                m_CacheEditor.Repaint();
            }
        }
        //-----------------------------------------------------
        public Ray GetRay(Event evt, Rect previewRect)
        {
            Vector3 vPos = m_PreviewUtility.camera.ScreenToWorldPoint(evt.mousePosition);
            vPos = GetCurrentMouseWorldPosition(evt, m_Rect);

            Ray ray;
            float scaleFactor = m_PreviewUtility.GetScaleFactor(previewRect.width, previewRect.height);

            Vector3 mouspos = evt.mousePosition;
            mouspos.x = (evt.mousePosition.x - previewRect.x) * scaleFactor;
            mouspos.y = (previewRect.height - (evt.mousePosition.y - previewRect.y)) * scaleFactor;
            ray = m_PreviewUtility.camera.ScreenPointToRay(mouspos);

            return ray;
        }
        //-----------------------------------------------------
        protected void HandleMouseDown(Event evt, int id, Rect previewRect)
        {
            int hot = GUIUtility.hotControl;
            m_MouseFirstPress = evt.mousePosition;
            if (previewRect.Contains(evt.mousePosition))
            {
                ViewTool tool = viewTool;
                EditorGUIUtility.SetWantsMouseJumping(1);
                if (bBreakEvent) evt.Use();
                GUIUtility.hotControl = id;

                if (Event.current.type != EventType.MouseDrag && Event.current.button == 0)
                    OnRayHitTarget(evt, id, previewRect);

                if (OnMouseDownCB != null)
                {
                    OnMouseDownCB(GetRay(evt, previewRect), Change2DTo3DPos(ConvertMousePosition(evt.mousePosition, previewRect)), evt);
                }
            }
        }
        //-----------------------------------------------------
        public void HandleMouseMove(Event evt, int id, Rect previewRect)
        {
            if (!previewRect.Contains(evt.mousePosition)) return;
            if (!previewRect.Contains(m_MouseFirstPress)) return;

            if (OnMoseMoveCB != null)
            {
                OnMoseMoveCB(GetRay(evt, previewRect), Change2DTo3DPos(ConvertMousePosition(evt.mousePosition, previewRect)), evt );
            }
        }
        //-----------------------------------------------------
        protected void HandleMouseUp(Event evt, int id, Rect previewRect)
        {
            m_ViewTool = ViewTool.None;
            if (GUIUtility.hotControl == id)
            {
                GUIUtility.hotControl = 0;
                EditorGUIUtility.SetWantsMouseJumping(0);
                if (bBreakEvent) evt.Use();

                if (OnMosueUpCB != null)
                {
                    Vector3 vPos = m_PreviewUtility.camera.ScreenToWorldPoint(evt.mousePosition);
                    vPos = GetCurrentMouseWorldPosition(evt, m_Rect);
                    OnMosueUpCB(GetRay(evt, previewRect), vPos, evt);
                }
            }
        }
        //-----------------------------------------------------
        public void OnRayHitTarget(Event evt, int id, Rect previewRect)
        {
            if (m_PreviewUtility != null)
            {
                float scaleFactor = m_PreviewUtility.GetScaleFactor(previewRect.width, previewRect.height);

                if (OnMoseHitCB != null)
                {
                    Vector3 vPos = m_PreviewUtility.camera.ScreenToWorldPoint(evt.mousePosition);
                    vPos = GetCurrentMouseWorldPosition(evt, m_Rect);
                    OnMoseHitCB(GetRay(evt, previewRect), vPos, evt);
                }
            }
        }
        //-----------------------------------------------------
        protected void HandleMouseDrag(Event evt, int id, Rect previewRect)
        {
            if (m_PreviewInstance == null)
            {
                return;
            }

            if (!previewRect.Contains(m_MouseFirstPress)) return;

            if (GUIUtility.hotControl == id)
            {
                switch (m_ViewTool)
                {
                    case ViewTool.Move:
                        if( (bLeftMouseForbidMove && evt.button == 0) )
                        {

                        }
                        else DoAvatarPreviewPan(evt);
                        break;
                    case ViewTool.Scale:
                        DoAvatarPreviewZoom(evt, -HandleUtility.niceMouseDeltaZoom * ((!evt.shift) ? 0.5f : 2f), previewRect);
                        break;
                    case ViewTool.Rotate:
                        if ((bLeftMouseForbidRotate && evt.button == 0))
                        {

                        }
                        else
                            DoAvatarPreviewOrbit(evt, previewRect);
                        break;
                    default:
                        //  Debug.Log("Enum value not handled");
                        break;
                }
            }
        }
        //-----------------------------------------------------
        protected void HandleViewTool(Event evt, EventType eventType, int id, Rect previewRect)
        {
            switch (eventType)
            {
                case EventType.MouseDown:
                    HandleMouseDown(evt, id, previewRect);
                    break;
                case EventType.MouseUp:
                    {
                        HandleMouseUp(evt, id, previewRect);
                    }
                    break;
                case EventType.MouseDrag:
                    {
                        bool bcan = true;
                        if (CheckCanMouseDrag != null && CheckCanMouseDrag(null)
                            && evt.button <= 0)
                            bcan = false;
                        if (bcan)
                        {
                            HandleMouseDrag(evt, id, previewRect);
                        }
                    }
                    break;

                case EventType.ScrollWheel:
                    DoAvatarPreviewZoom(evt, HandleUtility.niceMouseDeltaZoom * ((!evt.shift) ? 0.5f : 2f), previewRect);
                    break;
            }
        }
        //-----------------------------------------------------
        public void DoAvatarPreviewOrbit(Event evt, Rect previewRect)
        {
            if (!bCanControllCamera) return;
            m_PreviewDir -= evt.delta * (float)((!evt.shift) ? 1 : 3) / Mathf.Min(previewRect.width, previewRect.height) * 140f;
            m_PreviewDir.y = Mathf.Clamp(m_PreviewDir.y, -90f, 90f);
            if (bBreakEvent) evt.Use();
        }
        //-----------------------------------------------------
        public void DoAvatarPreviewPan(Event evt)
        {
            if (!bCanControllCamera) return;
            Camera camera = m_PreviewUtility.camera;
            Vector3 vector = camera.WorldToScreenPoint(bodyPosition + m_PivotPositionOffset);
            Vector3 a = new Vector3(-evt.delta.x, evt.delta.y, 0f);
            vector += a * Mathf.Lerp(0.25f, 2f, m_ZoomFactor * 0.5f);
            Vector3 b = camera.ScreenToWorldPoint(vector) - (bodyPosition + m_PivotPositionOffset);
            m_PivotPositionOffset += b;
            if (bBreakEvent) evt.Use();
        }
        //-----------------------------------------------------
        public void ResetCamaraOffset(Vector3 offset)
        {
            m_PreviewDir = Vector2.zero;
            m_vMouseHitPos = Vector3.zero;
            m_PivotPositionOffset = offset;
            m_ZoomFactor = m_AvatarScale;
        }
        //-----------------------------------------------------
        public void ResetNoDir()
        {
            m_PivotPositionOffset = Vector3.zero;
            m_ZoomFactor = m_AvatarScale;
            m_vMouseHitPos = Vector3.zero;
        }
        //-----------------------------------------------------
        bool[] m_bKeyMovePress = new bool[] { false, false, false, false, false };
        public void DoAvatarPreviewFrame(Event evt, EventType type, Rect previewRect)
        {
            if (type == EventType.KeyDown && evt.keyCode == KeyCode.F && !Event.current.control)
            {
                m_PivotPositionOffset = Vector3.zero;
                m_ZoomFactor = m_AvatarScale;
                m_vMouseHitPos = Vector3.zero;
                if (bBreakEvent) evt.Use();
            }
            if (type == EventType.KeyDown && evt.keyCode == KeyCode.G && !Event.current.control)
            {
                m_vMouseHitPos = Vector3.zero;
                m_PivotPositionOffset = GetCurrentMouseWorldPosition(evt, previewRect) - bodyPosition;
                if (bBreakEvent) evt.Use();
            }


            if (CanKeyMoveCamera)
            {
                if (type == EventType.KeyDown)
                {
                    if (evt.keyCode == KeyCode.W) m_bKeyMovePress[0] = true;
                    if (evt.keyCode == KeyCode.S) m_bKeyMovePress[1] = true;
                    if (evt.keyCode == KeyCode.A) m_bKeyMovePress[2] = true;
                    if (evt.keyCode == KeyCode.D) m_bKeyMovePress[3] = true;
                    if (evt.keyCode == KeyCode.LeftShift) m_bKeyMovePress[4] = true;
                }
                else if (type == EventType.KeyUp)
                {
                    if (evt.keyCode == KeyCode.W) m_bKeyMovePress[0] = false;
                    if (evt.keyCode == KeyCode.S) m_bKeyMovePress[1] = false;
                    if (evt.keyCode == KeyCode.A) m_bKeyMovePress[2] = false;
                    if (evt.keyCode == KeyCode.D) m_bKeyMovePress[3] = false;
                    if (evt.keyCode == KeyCode.LeftShift) m_bKeyMovePress[4] = false;
                }

                if (m_bKeyMovePress[0])
                {
                    Camera camera = m_PreviewUtility.camera;
                    if (m_bKeyMovePress[4])
                        m_PivotPositionOffset += camera.transform.forward * 0.05f;
                    else
                        m_PivotPositionOffset += camera.transform.forward * 0.01f;
               //     evt.Use();
                }
                if (m_bKeyMovePress[1])
                {
                    Camera camera = m_PreviewUtility.camera;
                    if (m_bKeyMovePress[4])
                        m_PivotPositionOffset -= camera.transform.forward * 0.05f;
                    else
                        m_PivotPositionOffset -= camera.transform.forward * 0.01f;
                //    evt.Use();
                }
                if (m_bKeyMovePress[2])
                {
                    Camera camera = m_PreviewUtility.camera;
                    Vector3 vright = camera.transform.right;
                    vright.Normalize();
                    if (m_bKeyMovePress[4])
                        m_PivotPositionOffset -= vright * 0.02f;
                    else
                        m_PivotPositionOffset -= vright * 0.005f;
                //    evt.Use();
                }
                if (m_bKeyMovePress[3])
                {
                    Camera camera = m_PreviewUtility.camera;
                    Vector3 vright = camera.transform.right;
                    vright.Normalize();
                    if (m_bKeyMovePress[4])
                        m_PivotPositionOffset += vright * 0.02f;
                    else
                        m_PivotPositionOffset += vright * 0.005f;
                //    evt.Use();
                }
            }
            else
            {
                m_bKeyMovePress[0] = false;
                m_bKeyMovePress[1] = false;
                m_bKeyMovePress[2] = false;
                m_bKeyMovePress[3] = false;
                m_bKeyMovePress[4] = false;
            }
        }
        //-----------------------------------------------------
        public Vector3 GetCurrentMouseWorldPosition(Event evt)
        {
            return Change2DTo3DPos(ConvertMousePosition(evt.mousePosition, m_Rect));
        }
        //-----------------------------------------------------
        protected Vector3 GetCurrentMouseWorldPosition(Event evt, Rect previewRect)
        {
            return Change2DTo3DPos(ConvertMousePosition(evt.mousePosition, previewRect));
        }
        //-----------------------------------------------------
        protected Vector3 ConvertMousePosition(Vector3 mousePosition, Rect previewRect)
        {
            float scaleFactor = m_PreviewUtility.GetScaleFactor(previewRect.width, previewRect.height);

            Vector3 mouspos = mousePosition;
            mouspos.x = (mousePosition.x - previewRect.x) * scaleFactor;
            mouspos.y = (previewRect.height - (mousePosition.y - previewRect.y)) * scaleFactor;
            return mouspos;
        }
        //-----------------------------------------------------
        protected Vector3 Change2DTo3DPos(Vector3 mousePosition)
        {
            Ray ray = m_PreviewUtility.camera.ScreenPointToRay(mousePosition);

            Camera camera = m_PreviewUtility.camera;

            Vector3 pos = ray.origin;
            Vector3 dir = ray.direction;

            Vector3 vPlanePos = bodyPosition;
            Vector3 vPlaneNor = m_PreviewInstance ? m_PreviewInstance.transform.up : Vector3.up;

            float fdot = Vector3.Dot(dir, vPlaneNor);
            if (fdot == 0.0f)
                return Vector3.zero;

            float fRage = ((vPlanePos.x - pos.x) * vPlaneNor.x + (vPlanePos.y - pos.y) * vPlaneNor.y + (vPlanePos.z - pos.z) * vPlaneNor.z) / fdot;

            Vector3 vPos = pos + dir * fRage;
            vPos.y = vPlanePos.y;

            return vPos;
        }
        //-----------------------------------------------------
        public void DoAvatarPreviewZoom(Event evt, float delta, Rect previewRect)
        {
            if (!bCanControllCamera) return;
            if (!previewRect.Contains(evt.mousePosition))
                return;

            float num = -delta * 0.05f;
            m_ZoomFactor += m_ZoomFactor * num;
            m_ZoomFactor = Mathf.Max(m_ZoomFactor, m_AvatarScale / 10f);
            if (bBreakEvent) evt.Use();
        }
        //-----------------------------------------------------
        private float PreviewSlider(float val, float snapThreshold)
        {
            val = GUILayout.HorizontalSlider(val, 0.1f, 3f, s_Styles.preSlider, s_Styles.preSliderThumb, GUILayout.MaxWidth(64f));
            if (val > 0.25f - snapThreshold && val < 0.25f + snapThreshold)
            {
                val = 0.25f;
            }
            else
            {
                if (val > 0.5f - snapThreshold && val < 0.5f + snapThreshold)
                {
                    val = 0.5f;
                }
                else
                {
                    if (val > 0.75f - snapThreshold && val < 0.75f + snapThreshold)
                    {
                        val = 0.75f;
                    }
                    else
                    {
                        if (val > 1f - snapThreshold && val < 1f + snapThreshold)
                        {
                            val = 1f;
                        }
                        else
                        {
                            if (val > 1.25f - snapThreshold && val < 1.25f + snapThreshold)
                            {
                                val = 1.25f;
                            }
                            else
                            {
                                if (val > 1.5f - snapThreshold && val < 1.5f + snapThreshold)
                                {
                                    val = 1.5f;
                                }
                                else
                                {
                                    if (val > 1.75f - snapThreshold && val < 1.75f + snapThreshold)
                                    {
                                        val = 1.75f;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return val;
        }
        //-----------------------------------------------------
        // GameObject mesh_show = null;
        // public static void DrawMesh(Mesh mesh, Matrix4x4 transform, Material mat, int cull, Camera camera, TargetPreview preview = null)
        // {
        ////     UnityEngine.Rendering.CompareFunction zTest = Handles.zTest;
        ////     Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        //     Graphics.DrawMesh(mesh, transform, mat, cull, camera);
        //   //  Handles.zTest = zTest;

        //     //mat.renderQueue = 1000;
        //     //if (preview != null)
        //     //{
        //     //    MeshRenderer render;
        //     //    MeshFilter meshfile;
        //     //    if (preview.mesh_show == null)
        //     //    {
        //     //        preview.mesh_show = new GameObject("meshDraw");
        //     //        render = preview.mesh_show.AddComponent<MeshRenderer>();
        //     //        meshfile = preview.mesh_show.AddComponent<MeshFilter>();
        //     //        render.sortingOrder = 100;
        //     //        preview.m_PreviewUtility.AddSingleGO(preview.mesh_show);
        //     //    }
        //     //    else
        //     //    {
        //     //        render = preview.mesh_show.GetComponent<MeshRenderer>();
        //     //        meshfile = preview.mesh_show.GetComponent<MeshFilter>();
        //     //    }

        //     //    meshfile.sharedMesh = mesh;
        //     //    render.sharedMaterial = mat;

        //     //    //preview.m_PreviewUtility.DrawMesh(mesh, transform, mat, 0);
        //     //    return;
        //     //}
        //     //if(camera != null)
        //     //{
        //     //    Graphics.DrawMesh(mesh, transform, mat, cull, camera);
        //     // }
        // }
        //-----------------------------------------------------
        public static void SetEnabledRecursive(GameObject go, bool enabled)
        {
            if (go == null) return;
            Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                Renderer renderer = componentsInChildren[i];
                renderer.enabled = enabled;
            }
        }
        //-----------------------------------------------------
        public static void BackupInstantiatedFlags(GameObject go, ref Dictionary<GameObject, HideFlags> flagMaps)
        {
            if (go == null) return;
            if (!flagMaps.ContainsKey(go))
                flagMaps.Add(go, go.hideFlags);
            foreach (Transform transform in go.transform)
            {
                BackupInstantiatedFlags(transform.gameObject, ref flagMaps);
            }
        }
        //-----------------------------------------------------
        public void AddPreview(GameObject go, HideFlags falgs = HideFlags.HideAndDontSave)
        {
            if (go == null) return;

            if (m_PreviewUtility != null && go.transform.parent == null)
                m_PreviewUtility.AddSingleGO(go);

            go.layer = PreviewCullingLayer;
            go.hideFlags = falgs;
            foreach (Transform transform in go.transform)
            {
                AddPreview(transform.gameObject, falgs);
            }
        }
        //-----------------------------------------------------
        public static void GetRenderableBoundsRecurse(ref Bounds bounds, GameObject go)
        {
            if (go == null) return;
            MeshRenderer meshRenderer = go.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
            MeshFilter meshFilter = go.GetComponent(typeof(MeshFilter)) as MeshFilter;
            if (meshRenderer && meshFilter && meshFilter.sharedMesh)
            {
                if (bounds.extents == Vector3.zero)
                {
                    bounds = meshRenderer.bounds;
                }
                else
                {
                    bounds.Encapsulate(meshRenderer.bounds);
                }
            }
            SkinnedMeshRenderer skinnedMeshRenderer = go.GetComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
            if (skinnedMeshRenderer && skinnedMeshRenderer.sharedMesh)
            {
                if (bounds.extents == Vector3.zero)
                {
                    bounds = skinnedMeshRenderer.bounds;
                }
                else
                {
                    bounds.Encapsulate(skinnedMeshRenderer.bounds);
                }
            }
            SpriteRenderer spriteRenderer = go.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
            if (spriteRenderer && spriteRenderer.sprite)
            {
                if (bounds.extents == Vector3.zero)
                {
                    bounds = spriteRenderer.bounds;
                }
                else
                {
                    bounds.Encapsulate(spriteRenderer.bounds);
                }
            }
            foreach (Transform transform in go.transform)
            {
                GetRenderableBoundsRecurse(ref bounds, transform.gameObject);
            }
        }
        //-----------------------------------------------------
        private static int CycleButton(int selected, GUIContent[] contents, GUIStyle style)
        {
            bool flag = GUILayout.Button(contents[selected], style);
            if (flag)
            {
                int num = selected;
                selected = num + 1;
                bool flag2 = selected >= contents.Length;
                if (flag2)
                {
                    selected = 0;
                }
            }
            return selected;
        }
        //-----------------------------------------------------
        private static GUIContent IconContent(string name, string tooltip)
        {
            GUIContent content = EditorGUIUtility.IconContent(name, tooltip);
            content.tooltip = tooltip;
            return content;
        }
        //-----------------------------------------------------
        public static void GetTransformBone(ref Dictionary<Transform, bool> vList, Transform root)
        {
            if (root)
            {
                if (!vList.ContainsKey(root.transform))
                    vList.Add(root.transform, true);
                for (int i = 0; i < root.childCount; ++i)
                {
                    if (!vList.ContainsKey(root.GetChild(i)))
                        vList.Add(root.GetChild(i), true);
                    GetTransformBone(ref vList, root.GetChild(i).transform);
                }
            }
        }
        //-----------------------------------------------------
        static public void GetGameObjectBone(ref List<string> vList, GameObject root)
        {
            if (root)
            {
                string bonename = root.name.ToLower();
                if (bonename.Contains("bone") || bonename.Contains("bip") || bonename.Contains("POINT"))
                    vList.Add(root.name);
                for (int i = 0; i < root.transform.childCount; ++i)
                {
                    GetGameObjectBone(ref vList, root.transform.GetChild(i).gameObject);
                }
            }
        }
    }

}
#endif