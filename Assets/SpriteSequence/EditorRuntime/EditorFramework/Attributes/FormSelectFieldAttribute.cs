/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	FormSelectFieldAttribute
作    者:	HappLI
描    述:	表格字段属性
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    public class FormSelectFieldAttribute : Attribute
    {
#if UNITY_EDITOR
        public string fieldName;
#endif
        public FormSelectFieldAttribute(string fieldName)
        {
#if UNITY_EDITOR
            this.fieldName = fieldName;
#endif
        }
    }
}