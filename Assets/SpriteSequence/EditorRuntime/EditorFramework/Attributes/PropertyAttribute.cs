/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	PropertyAttribute
作    者:	HappLI
描    述:	属性绘制
*********************************************************************/
using System;
using System.Collections.Generic;

namespace Framework.DrawProps
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public class PropertyAttribute : Attribute
    {
#if UNITY_EDITOR
        public int version = 0;
        public string displayName { get; set; }
#endif
        public PropertyAttribute()
        {
#if UNITY_EDITOR
            this.displayName = null;
            this.version = 0;
#endif
        }
        public PropertyAttribute(string displayName, int version = 0)
        {
#if UNITY_EDITOR
this.displayName = displayName;
this.version = version;
#endif
        }
    }

    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple =true)]
    public class SplitPropertyAttribute : Attribute
    {
#if UNITY_EDITOR
        public int version = 0;
        public string displayName { get; set; }
        public string fieldName;
#endif
        public SplitPropertyAttribute(string displayName, string fieldName, int version = 0)
        {
#if UNITY_EDITOR
            this.displayName = displayName;
            this.fieldName = fieldName;
            this.version = version;
#endif
        }
    }
}