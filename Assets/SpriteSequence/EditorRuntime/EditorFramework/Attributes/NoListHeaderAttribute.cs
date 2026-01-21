/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	NoListHeaderAttribute
作    者:	HappLI
描    述:	列表不显示列表头
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)]
    public class NoListHeaderAttribute : Attribute
    {
        public NoListHeaderAttribute()
        {
        }
    }
}