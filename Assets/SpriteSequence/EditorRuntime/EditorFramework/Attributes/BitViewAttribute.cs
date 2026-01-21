/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	BitViewAttribute
作    者:	HappLI
描    述:	二进制ui绘制属性
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum, AllowMultiple = true)]
    public class BitViewAttribute : Attribute
    {
#if UNITY_EDITOR
        public string displayName;
        public int offset;
#endif
        public BitViewAttribute(string displayName, int offset )
        {
#if UNITY_EDITOR
            this.displayName = displayName;
            this.offset = offset;
#endif
        }
    }
}