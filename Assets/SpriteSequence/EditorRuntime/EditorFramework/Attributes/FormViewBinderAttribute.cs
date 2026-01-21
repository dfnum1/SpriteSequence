/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	FormViewBinderAttribute
作    者:	HappLI
描    述:	表格视图
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum, AllowMultiple = true)]
    public class FormViewBinderAttribute : Attribute
    {
#if UNITY_EDITOR
        private System.Type bindTable;
        public string tableName = "";
        public string Field = "";
        public string KeyValue = "";
#endif
        public FormViewBinderAttribute(System.Type type, string field = "nID", string KeyValue = "")
        {
#if UNITY_EDITOR
            bindTable = type;
            tableName = "";
            this.Field = field;
            this.KeyValue = KeyValue;
#endif
        }

        public FormViewBinderAttribute(string tableName, string field = "nID", string KeyValue = "")
        {
#if UNITY_EDITOR
            this.tableName = tableName;
            this.Field = field;
            this.KeyValue = KeyValue;
#endif
        }
#if UNITY_EDITOR
        public System.Type GetTableType()
        {
            if (bindTable != null) return bindTable;
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(tableName))
            {
          //      bindTable = Data.DataEditorUtil.GetTableType(tableName);
            }
#endif
            return bindTable;
        }
#endif
    }
}