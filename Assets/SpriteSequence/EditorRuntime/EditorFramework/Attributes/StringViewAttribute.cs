/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	StringViewAttribute
作    者:	HappLI
描    述:	字符串显示未某种类型
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class StringViewAttribute : Attribute
    {
#if UNITY_EDITOR
        private System.Type m_bindType;
        public string bindTypeName;
        public int order = 0;
#endif
        public StringViewAttribute(System.Type type, int order =0)
        {
#if UNITY_EDITOR
            m_bindType = type;
            bindTypeName = null;
            this.order = order;
#endif
        }
        public StringViewAttribute(string typeName, int order =0)
        {
#if UNITY_EDITOR
            m_bindType = null;
            bindTypeName = typeName;
            this.order = order;
#endif
        }
#if UNITY_EDITOR
        //------------------------------------------------------
        public System.Type GetBindType()
        {
            if (m_bindType != null) return m_bindType;
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(bindTypeName))
                this.m_bindType = ED.EditorUtils.GetTypeByName(bindTypeName);
#endif
            return m_bindType;
        }
#endif
    }
}