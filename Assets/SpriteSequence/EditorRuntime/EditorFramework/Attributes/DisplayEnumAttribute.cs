/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	DisplayEnumAttribute
作    者:	HappLI
描    述:	枚举类型绘制
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    public class DisplayEnumAttribute : Attribute
    {
#if UNITY_EDITOR
        public string enumTypeName="";
        private System.Type enumType;

        public string strField = "";
        public string[] groups = null;
#endif
        public DisplayEnumAttribute(System.Type enumType)
        {
#if UNITY_EDITOR
            this.enumType = enumType;
            this.enumTypeName = "";
#endif
        }
        public DisplayEnumAttribute(System.Type enumType, string field, string strGroup)
        {
#if UNITY_EDITOR
            this.enumType = enumType;
            this.enumTypeName = "";
            this.strField = field;
            if (!string.IsNullOrEmpty(strGroup))
                this.groups = new string[1] { strGroup };
#endif
        }
        public DisplayEnumAttribute(System.Type enumType, string field, string[] groups)
        {
#if UNITY_EDITOR
            this.enumType = enumType;
            this.strField = field;
            this.groups = groups;
            this.enumTypeName = "";
#endif
        }
        public DisplayEnumAttribute(string enumType)
        {
#if UNITY_EDITOR
            this.enumTypeName = enumType;
#endif

        }
        public DisplayEnumAttribute(string enumType, string field, string strGroup)
        {
#if UNITY_EDITOR
            this.enumTypeName = enumType;
            this.strField = field;
            if (!string.IsNullOrEmpty(strGroup))
                this.groups = new string[1] { strGroup };
#endif
        }
        public DisplayEnumAttribute(string enumType, string field, string[] groups)
        {
#if UNITY_EDITOR
            this.enumTypeName = enumType;
            this.strField = field;
            this.groups = groups;
#endif
        }
#if UNITY_EDITOR
        public System.Type GetEnumType()
        {
            if (enumType != null) return enumType;
#if UNITY_EDITOR
            if(!string.IsNullOrEmpty(enumTypeName))
                this.enumType = ED.EditorUtils.GetTypeByName(enumTypeName);
#endif
            return enumType;
        }
#endif
    }
}