/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	StateByBitAttribute
作    者:	HappLI
描    述:	根据某属性显示状态
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class StateByBitAttribute : Attribute
    {
#if UNITY_EDITOR
        public string fieldName;
        public System.Collections.Generic.List<int> fieldValue = new System.Collections.Generic.List<int>();
#endif
        public StateByBitAttribute(string fieldName, int fieldValue)
        {
#if UNITY_EDITOR
            this.fieldName = fieldName;
            this.fieldValue.Add(fieldValue);
#endif
        }
        public StateByBitAttribute(string fieldName, int[] fieldValue)
        {
#if UNITY_EDITOR
            this.fieldName = fieldName;
            if (fieldValue == null) return;
            for (int i = 0; i < fieldValue.Length; ++i)
                this.fieldValue.Add(fieldValue[i]);
#endif
        }
    }
}