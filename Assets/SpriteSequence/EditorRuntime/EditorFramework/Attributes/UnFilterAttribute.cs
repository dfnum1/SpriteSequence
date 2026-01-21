/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	UnFilterAttribute
作    者:	HappLI
描    述:	枚举弹窗pop不进行分类
*********************************************************************/
using System;
using System.Collections.Generic;

namespace Framework.DrawProps
{
    [AttributeUsage(AttributeTargets.Enum)]
    public class UnFilterAttribute : Attribute
    {
        public UnFilterAttribute()
        {
        }
    }
}