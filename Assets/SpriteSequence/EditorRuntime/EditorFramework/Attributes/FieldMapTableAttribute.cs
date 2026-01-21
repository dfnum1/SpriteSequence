/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	FieldMapTableAttribute
作    者:	HappLI
描    述:	一对多表格属性
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    public class FieldMapTableAttribute : System.Attribute
    {
#if UNITY_EDITOR
        public System.Type table;
        public string strMapFunc;
        public string strMapField;
        public string strMapToField;
#endif
        public FieldMapTableAttribute(string strMapFunc = "", System.Type table = null, string strMapField = "", string strMapToField = "")
        {
#if UNITY_EDITOR
            this.strMapFunc = strMapFunc;
            this.table = table;
            this.strMapField = strMapField;
            this.strMapToField = strMapToField;
#endif
        }
    }
}