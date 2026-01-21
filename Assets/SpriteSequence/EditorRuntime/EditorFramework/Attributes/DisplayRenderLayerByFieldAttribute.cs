/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	DisplayRenderLayerByFieldAttribute
作    者:	HappLI
描    述:	根据属性选渲染层级
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class DisplayRenderLayerByFieldAttribute : Attribute
    {
#if UNITY_EDITOR
        public string fieldName;
        public string fieldValue;
#endif
        public DisplayRenderLayerByFieldAttribute(string fieldName, string fieldValue)
        {
#if UNITY_EDITOR
            this.fieldName = fieldName;
            this.fieldValue = fieldValue.ToLower();
#endif
        }
    }
}