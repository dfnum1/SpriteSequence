/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	StringViewPluginAttribute
作    者:	HappLI
描    述:	字符串显示为指定插件
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    public class StringViewPluginAttribute : Attribute
    {
#if UNITY_EDITOR
        public string userPlugin;
#endif
        public StringViewPluginAttribute(string userPlugin)
        {
#if UNITY_EDITOR
            this.userPlugin = userPlugin;
#endif
        }
    }
}