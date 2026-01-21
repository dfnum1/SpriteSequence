/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	StateStringViewGUIAttribute
作    者:	HappLI
描    述:	显示表单
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class StateStringViewGUIAttribute : Attribute
    {
#if UNITY_EDITOR
        public string strField;
        public string strValue;
        public string strType;
        System.Type bindType;
#endif
        public StateStringViewGUIAttribute(string strField, string strValue, System.Type type)
        {
#if UNITY_EDITOR
            this.strField = strField;
            this.strValue = strValue;
            bindType = type;
            this.strType = null;
#endif
        }
        //------------------------------------------------------
        public StateStringViewGUIAttribute(string strField, string strValue, string type)
        {
#if UNITY_EDITOR
            this.strField = strField;
            this.strValue = strValue;
            bindType = null;
            this.strType = type;
#endif
        }
#if UNITY_EDITOR
        //------------------------------------------------------
        public System.Type GetBindType()
        {
            if (bindType != null) return bindType;
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(strType))
                this.bindType = ED.EditorUtils.GetTypeByName(strType);
#endif
            return bindType;
        }
#endif
    }
}