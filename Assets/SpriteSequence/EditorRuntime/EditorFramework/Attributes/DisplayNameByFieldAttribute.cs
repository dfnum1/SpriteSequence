/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	DisplayNameByFieldAttribute
作    者:	HappLI
描    述:	根据具体数据显示名称
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class DisplayNameByFieldAttribute : Attribute
    {
#if UNITY_EDITOR
        public string fieldName;
        public string strDisplayName;
        public string tips;
        public System.Collections.Generic.List<string> fieldValue = new System.Collections.Generic.List<string>();
#endif
        public DisplayNameByFieldAttribute(string fieldName, string fieldValue, string strDisplayName=null, string tips = null)
        {
#if UNITY_EDITOR
            this.strDisplayName = strDisplayName;
            this.fieldName = fieldName;
            this.fieldValue.Add(fieldValue.ToLower());
            this.tips = tips;
#endif
        }
        public DisplayNameByFieldAttribute(string fieldName, string[] fieldValue, string strDisplayName = null, string tips = null)
        {
#if UNITY_EDITOR
            this.strDisplayName = strDisplayName;
            this.fieldName = fieldName;
            if (fieldValue == null) return;
            for (int i = 0; i < fieldValue.Length; ++i)
                this.fieldValue.Add(fieldValue[i].ToLower());
            this.tips = tips;
#endif
        }
    }
}