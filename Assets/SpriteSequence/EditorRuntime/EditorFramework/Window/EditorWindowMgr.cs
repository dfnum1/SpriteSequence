#if UNITY_EDITOR
/********************************************************************
生成日期:		11:06:2020
类    名: 	EditorWindowMgr
作    者:	HappLI
描    述:	编辑器窗口管理
*********************************************************************/
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework.ED
{
    public class EditorWindowMgr
    {
        static List<EditorWindowBase> m_vList = new List<EditorWindowBase>();
        static List<EditorWindowBase> m_vRuntimeOpenList = new List<EditorWindowBase>();
        //-------------------------------------------
        static public void RegisterWindow(EditorWindowBase window)
        {
            if (!window.IsManaged()) return;
            if(window.IsRuntimeOpen())
            {
                if (m_vRuntimeOpenList.Contains(window)) return;
                m_vRuntimeOpenList.Add(window);
                return;
            }
            if (m_vList.Contains(window)) return;
            m_vList.Add(window);
        }
        //-------------------------------------------
        static public void UnRegisterWindow(EditorWindowBase window)
        {
            if (ms_bCloseLock) return;
            m_vList.Remove(window);
            m_vRuntimeOpenList.Remove(window);
        }
        //-------------------------------------------
        static bool ms_bCloseLock = false;
        static public bool CloseAll()
        {
            if(m_vList.Count>0)
            {
                if(!EditorUtility.DisplayDialog("提示", "当前有开着的编辑器，是否确认保存，再继续？", "继续", "保存后再继续吧"))
                {
                    return false;
                }

                ms_bCloseLock = true;
                for (int i = 0; i < m_vList.Count; ++i)
                {
                    if (m_vList[i]) m_vList[i].Close();
                }
                ms_bCloseLock = false;
                m_vList.Clear();
            }
            if (m_vRuntimeOpenList.Count > 0)
            {
                ms_bCloseLock = true;
                for (int i = 0; i < m_vRuntimeOpenList.Count; ++i)
                {
                    if (m_vRuntimeOpenList[i]) m_vRuntimeOpenList[i].Close();
                }
                ms_bCloseLock = false;
                m_vRuntimeOpenList.Clear();
            }
            return true;
        }
    }
}
#endif