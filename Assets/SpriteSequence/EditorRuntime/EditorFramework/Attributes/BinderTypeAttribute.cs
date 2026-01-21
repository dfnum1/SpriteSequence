/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	BinderTypeAttribute
作    者:	HappLI
描    述:	数据类型绑定
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    public class BinderTypeAttribute : Attribute
    {
#if UNITY_EDITOR
        public string bindName;
#endif
        public BinderTypeAttribute(string bindName)
        {
#if UNITY_EDITOR
            this.bindName = bindName;
#endif
        }
    }
}