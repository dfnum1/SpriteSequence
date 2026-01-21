#if UNITY_EDITOR
/********************************************************************
生成日期:		11:06:2020
类    名: 	EditorWindowBase
作    者:	HappLI
描    述:	基础编辑器窗口,所有将编辑都继承于他
*********************************************************************/
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Framework.ED
{
    public abstract class EditorWindowBase : EditorWindow
    {
        private bool                                    m_bRuntimingOpened = false;
        protected EditorTimer                           m_pTimer = new EditorTimer();

        protected System.Object                         m_pCurrentObj;
        private List<AEditorLogic>                      m_vLogics = new List<AEditorLogic>();
        private Dictionary<System.Type, AEditorLogic>   m_vLogicKV = new Dictionary<System.Type, AEditorLogic>();
        //--------------------------------------------------------
        public List<AEditorLogic> GetLogics()
        {
            return m_vLogics;
        }
        //--------------------------------------------------------
        public System.Object GetCurrentObj()
        {
            return m_pCurrentObj;
        }
        //--------------------------------------------------------
        void OnEnable()
        {
            EditorWindowMgr.RegisterWindow(this);
            ScanerRegisterLogics();

            SceneView.duringSceneGui += OnSceneView;

            OnInnerEnable();

            for (int i = 0; i < m_vLogics.Count; ++i)
                m_vLogics[i].Enable();
        }
        //--------------------------------------------------------
        void OnDisable()
        {
            EditorWindowMgr.UnRegisterWindow(this);
            SceneView.duringSceneGui -= OnSceneView;

            OnInnerDisable();
            for (int i =0; i < m_vLogics.Count; ++i)
                m_vLogics[i].Disable();
            m_bRuntimingOpened = false;
            m_pCurrentObj = null;
        }
        //--------------------------------------------------------
        protected virtual void OnInnerEnable() { }
        protected virtual void OnInnerDisable() { }
        //--------------------------------------------------------
        void Update()
        {
            m_pTimer.Update();
            OnInnerUpdate();
            for (int i = 0; i < m_vLogics.Count; ++i)
                m_vLogics[i].Update(m_pTimer.deltaTime);
            this.Repaint();
        }
        //--------------------------------------------------------
        protected virtual void OnInnerUpdate() { }
        //--------------------------------------------------------
        void OnGUI()
        {
            OnEvent(Event.current);

            OnInnerGUI();
            for (int i = 0; i < m_vLogics.Count; ++i)
                m_vLogics[i].RenderGUI();
            OnAfterInnerGUI();
        }
        //--------------------------------------------------------
        protected virtual void OnInnerGUI() { }
        //--------------------------------------------------------
        protected virtual void OnAfterInnerGUI() { }
        //--------------------------------------------------------
        protected void OnEvent(UnityEngine.Event evt)
        {
            OnInnerEvent(evt);
            for (int i = 0; i < m_vLogics.Count; ++i)
                m_vLogics[i].GuiEvent(Event.current);
        }
        //--------------------------------------------------------
        protected virtual void OnInnerEvent(UnityEngine.Event evt) { }
        //--------------------------------------------------------
        protected virtual void OnSceneView(SceneView sceneView)
        {
            for (int i = 0; i < m_vLogics.Count; ++i)
                m_vLogics[i].OnSceneView(sceneView);
        }
        //--------------------------------------------------------
        protected void ScanerRegisterLogics()
        {
            var types = this.GetType().Assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.IsSubclassOf(typeof(AEditorLogic)) && type.IsDefined(typeof(EditorBinderAttribute), false))
                {
                    EditorBinderAttribute attr = type.GetCustomAttribute<EditorBinderAttribute>();
                    if (attr.bindType == this.GetType())
                        RegisterLogic(type);
                }
            }
        }
        //--------------------------------------------------------
        void RegisterLogic(AEditorLogic logic)
        {
            if (m_vLogicKV.ContainsKey(logic.GetType())) return;
            m_vLogics.Add(logic);
            m_vLogics.Sort((l1, l2) => {
                EditorBinderAttribute attr1 = l1.GetType().GetCustomAttribute<EditorBinderAttribute>();
                EditorBinderAttribute attr2 = l2.GetType().GetCustomAttribute<EditorBinderAttribute>();
                return attr2.order - attr1.order;
            });
            m_vLogicKV[logic.GetType()] = logic;
        }
        //--------------------------------------------------------
        public T RegisterLogic<T>() where T : AEditorLogic
        {
            AEditorLogic logic = (AEditorLogic)System.Activator.CreateInstance(typeof(T));
            logic.Init(this);
            RegisterLogic(logic);
            return logic as T;
        }
        //--------------------------------------------------------
        public AEditorLogic RegisterLogic(System.Type type)
        {
            AEditorLogic logic = (AEditorLogic)System.Activator.CreateInstance(type);
            logic.Init(this);
            RegisterLogic(logic);
            return logic;
        }
        //--------------------------------------------------------
        public T GetLogic<T>() where T : AEditorLogic
        {
            AEditorLogic logic;
            if (m_vLogicKV.TryGetValue(typeof(T), out logic))
                return logic as T;
            return null;
        }
        //--------------------------------------------------------
        public override void SaveChanges()
        {
            base.SaveChanges();
            for (int i = 0; i < m_vLogics.Count; ++i)
            {
                m_vLogics[i].OnSaveChanges();
            }
        }
        //--------------------------------------------------------
        public float GetTimeScale()
        {
            return m_pTimer.m_currentSnap;
        }
        //--------------------------------------------------------
        public void SetTimeScale(float scale)
        {
            if (scale <= 0f) scale = 0.01f;
            m_pTimer.m_currentSnap = scale;
        }
        //--------------------------------------------------------
        public float GetDeltaTime()
        {
            return m_pTimer.deltaTime;
        }
        //--------------------------------------------------------
        public virtual int GetPriority() { return 0; }
        public virtual bool IsManaged() { return true; }
        public virtual bool IsRuntimeOpen() { return m_bRuntimingOpened; }
        //--------------------------------------------------------
        public virtual void OnChangeSelect(System.Object pObject)
        {
            if (m_pCurrentObj == pObject)
                return;
            m_pCurrentObj = pObject;
            for (int i = 0; i < m_vLogics.Count; ++i)
            {
                m_vLogics[i].OnChangeSelect(pObject);
            }
        }
    }
}
#endif