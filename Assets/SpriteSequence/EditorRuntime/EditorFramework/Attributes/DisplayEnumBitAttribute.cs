/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	DisplayEnumBitAttribute
作    者:	HappLI
描    述:	枚举类型进制位绘制
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    public class DisplayEnumBitAttribute : Attribute
    {
#if UNITY_EDITOR
        public string enumTypeName = "";
        private System.Type enumType;
        public bool bBitOffset = false;
#endif
        public DisplayEnumBitAttribute(System.Type enumType, bool bBitOffset = false)
        {
#if UNITY_EDITOR
            this.enumTypeName = "";
            this.enumType = enumType;
            this.bBitOffset = bBitOffset;
#endif
        }
        public DisplayEnumBitAttribute(string enumType, bool bBitOffset = false)
        {
#if UNITY_EDITOR
            this.enumTypeName = enumType;
            this.bBitOffset = bBitOffset;
#endif
        }
#if UNITY_EDITOR
        public System.Type GetEnumType()
        {
            if (enumType != null) return enumType;
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(enumTypeName))
                this.enumType = ED.EditorUtils.GetTypeByName(enumTypeName);
#endif
            return enumType;
        }
#endif
    }
}