/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	TableMappingAttribute
作    者:	HappLI
描    述:	表格映射
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    public class TableMappingAttribute : Attribute
    {
#if UNITY_EDITOR
        public string strSymbol = "";
#endif
        public TableMappingAttribute(string symbol="") 
        {
#if UNITY_EDITOR
            this.strSymbol = symbol;
#endif
        }
    }
}