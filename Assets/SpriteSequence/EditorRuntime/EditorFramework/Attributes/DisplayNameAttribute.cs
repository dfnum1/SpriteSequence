/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	DisplayNameAttribute
作    者:	HappLI
描    述:	显示名称
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)]
    public class DisplayNameAttribute : DisplayAttribute
    {
        public DisplayNameAttribute(string displayName)
        {
#if UNITY_EDITOR
            this.displayName = displayName;
#endif
        }
        public DisplayNameAttribute(string displayName, string tip)
        {
#if UNITY_EDITOR
            this.strTips = tip;
            this.displayName = displayName;
#endif
        }
    }
}