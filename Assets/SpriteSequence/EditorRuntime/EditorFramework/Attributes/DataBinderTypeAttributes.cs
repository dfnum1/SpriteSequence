/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	BinderTypeAttribute
作    者:	HappLI
描    述:	数据类型绑定
*********************************************************************/
using System;

namespace Framework.Data
{
    public class DataBinderTypeAttribute : System.Attribute
    {
        public string strConfigName = "";
        public string strMainKeyField = "nID";
        public string strMainKeyType="";
        public string DataField = "datas";
        public DataBinderTypeAttribute(string strConfigName, string strMainKeyType="ushort", string strMainKeyField = "nID", string DataField="datas")
        {
            this.strConfigName = strConfigName;
            this.strMainKeyField = strMainKeyField;
            this.strMainKeyType = strMainKeyType;
            this.DataField = DataField;
        }
    }

    public class BinaryDiscardAttribute : System.Attribute
    {
        public int version;
        public BinaryDiscardAttribute(int version)
        {
            this.version = version;
        }
    }

    public class BinaryCodeMarcosAttribute : System.Attribute
    {
        public string marcos;
        public BinaryCodeMarcosAttribute(string marcos)
        {
            this.marcos = marcos;
        }
    }

    public class BinaryCodeAttribute : System.Attribute
    {
        public int version;
        public string savePath = "";
        public BinaryCodeAttribute(int version, string savePath="")
        {
            this.version = version;
            this.savePath = savePath;
        }
    }

    public class BinaryFieldVersionAttribute : System.Attribute
    {
        public int version;
        public BinaryFieldVersionAttribute(int version)
        {
            this.version = version;
        }
    }

    public class BinaryUnServerAttribute : System.Attribute
    {
        public BinaryUnServerAttribute()
        {

        }
    }

    public class BinaryServerCodeAttribute : System.Attribute
    {
        public string savePath="";
        public BinaryServerCodeAttribute(string savePath = "")
        {
            this.savePath = savePath;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DataBindSetAttribute : System.Attribute
    {
#if UNITY_EDITOR
        public string field = "";
#endif
        public DataBindSetAttribute(string field)
        {
#if UNITY_EDITOR
            this.field = field;
#endif
        }
    }
}