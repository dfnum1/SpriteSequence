/********************************************************************
生成日期:		11:06:2020
类    名: 	AEditorLogic
作    者:	HappLI
描    述:	编辑器逻辑基础抽象类，所有编辑器的具体逻辑功能由多个Logic组成
*********************************************************************/
#if UNITY_EDITOR
using Framework.ED;
using System.Reflection;
using UnityEngine;

namespace Framework.ED
{
    public abstract class AEditorLogic
    {
        private bool m_bActive = true;
        private EditorWindowBase m_pEditorOwner;
        System.Reflection.FieldInfo m_pRectField = null;
        System.Reflection.PropertyInfo m_pRectProp = null;
        System.Reflection.MethodInfo m_pRectMethod = null;
        //--------------------------------------------------------
        internal void Init(EditorWindowBase editor)
        {
            m_pEditorOwner = editor; 
            var attri = GetType().GetCustomAttribute<EditorBinderAttribute>();
            if (attri != null && !string.IsNullOrEmpty(attri.rectMethod))
            {
                m_pRectField = editor.GetType().GetField(attri.rectMethod, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (m_pRectField == null)
                    m_pRectField = editor.GetType().GetField(attri.rectMethod, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (m_pRectField == null)
                {
                    m_pRectProp = editor.GetType().GetProperty(attri.rectMethod, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (m_pRectProp == null)
                        m_pRectProp = editor.GetType().GetProperty(attri.rectMethod, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    if (m_pRectProp == null)
                    {
                        m_pRectMethod = editor.GetType().GetMethod(attri.rectMethod, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (m_pRectMethod == null)
                            m_pRectMethod = editor.GetType().GetMethod(attri.rectMethod, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    }
                }
            }
            Awake();
        }
        //--------------------------------------------------------
        public void Active(bool bEable)
        {
            m_bActive = bEable;
        }
        //--------------------------------------------------------
        public bool IsActive()
        {
            return m_bActive;
        }
        //--------------------------------------------------------
        public EditorWindowBase GetOwner()
        {
            return m_pEditorOwner;
        }
        //--------------------------------------------------------
        public T GetOwner<T>() where T : EditorWindowBase
        {
            return m_pEditorOwner as T;
        }
        //--------------------------------------------------------
        public void Repaint()
        {
            m_pEditorOwner.Repaint();
        }
        //--------------------------------------------------------
        public virtual void OnSaveChanges()
        {

        }
        //--------------------------------------------------------
        public T GetLogic<T>() where T : AEditorLogic
        {
            return m_pEditorOwner.GetLogic<T>();
        }
        //--------------------------------------------------------
        public System.Collections.Generic.List<T> GetLogics<T>() where T : AEditorLogic
        {
            var logics = m_pEditorOwner.GetLogics();
            System.Collections.Generic.List<T> vLogics = new System.Collections.Generic.List<T>();
            foreach (var db in logics)
            {
                if (db is T) vLogics.Add(db as T);
            }
            return vLogics;
        }
        //--------------------------------------------------------
        public Rect GetRect()
        {
            if (m_pRectField != null) return (Rect)m_pRectField.GetValue(m_pEditorOwner);
            if (m_pRectProp != null) return (Rect)m_pRectProp.GetValue(m_pEditorOwner);
            if (m_pRectMethod != null) return (Rect)m_pRectMethod.Invoke(m_pEditorOwner, null);
            return Rect.zero;
        }
        //--------------------------------------------------------
        protected virtual void Awake() { }
        //--------------------------------------------------------
        internal void Enable() 
        {
            OnEnable();
        }
        protected virtual void OnEnable() { }
        //--------------------------------------------------------
        internal void Disable()
        {
            OnDisable();
        }
        protected virtual void OnDisable() { }
        //--------------------------------------------------------
        internal void RenderGUI()
        {
            if(m_bActive)
                OnGUI();
        }
        //--------------------------------------------------------
        protected virtual void OnGUI() { }
        //--------------------------------------------------------
        internal void GuiEvent(Event evt)
        {
            if (m_bActive)
                OnEvent(evt);
        }
        //--------------------------------------------------------
        protected virtual void OnEvent(Event evt) { }
        //--------------------------------------------------------
        internal void Update(float delta)
        {
            if (m_bActive)
                OnUpdate(delta);
        }
        //--------------------------------------------------------
        protected virtual void OnUpdate(float delta) { }
        //--------------------------------------------------------
        public virtual void OnChangeSelect(System.Object pData)
        {
        }
        //--------------------------------------------------------
        public virtual void OnRefreshData(System.Object pData)
        {

        }
        //--------------------------------------------------------
        public virtual void OnSceneView(UnityEditor.SceneView sceneView)
        {

        }
        //--------------------------------------------------------
        internal void Destroy()
        {
            OnDestroy();
        }
        //--------------------------------------------------------
        protected virtual void OnDestroy() { }

    }
}

#endif