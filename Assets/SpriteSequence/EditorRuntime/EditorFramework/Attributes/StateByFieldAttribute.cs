/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	StateByFieldAttribute
作    者:	HappLI
描    述:	根据某属性显示状态
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class StateByFieldAttribute : Attribute
    {
#if UNITY_EDITOR
        public string fieldName;
        public System.Collections.Generic.List<string> fieldValue = new System.Collections.Generic.List<string>();
        public bool IsContain;
#endif
        public StateByFieldAttribute(string fieldName, string fieldValue, bool IsContain = true)
        {
#if UNITY_EDITOR
            this.fieldName = fieldName;
            this.fieldValue.Add(fieldValue.ToLower());
            this.IsContain = IsContain;
#endif
        }
        public StateByFieldAttribute(string fieldName, string[] fieldValue, bool IsContain = true)
        {
#if UNITY_EDITOR
            this.fieldName = fieldName;
            if (fieldValue == null) return;
            for (int i = 0; i < fieldValue.Length; ++i)
            this.fieldValue.Add(fieldValue[i].ToLower());
            this.IsContain = IsContain;
#endif
        }
    }
}