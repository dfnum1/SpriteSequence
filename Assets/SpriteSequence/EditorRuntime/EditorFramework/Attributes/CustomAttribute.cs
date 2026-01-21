/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	CustomAttribute
作    者:	HappLI
描    述:	自定义绘制
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class CustomAttribute : Attribute
    {
        public CustomAttribute()
        {

        }
    }
}