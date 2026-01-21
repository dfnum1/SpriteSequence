/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	DisableAttribute
作    者:	HappLI
描    述:	不显示
*********************************************************************/
using System;
using System.Collections.Generic;

namespace Framework.DrawProps
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public class DisableAttribute : Attribute
    {
        public DisableAttribute()
        {
        }
    }
    //-----------------------------------------------------
    public class UnEditAttribute : Attribute
    {
    }
}