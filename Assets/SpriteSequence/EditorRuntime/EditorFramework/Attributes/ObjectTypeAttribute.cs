/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	ObjectTypeAttribute
作    者:	HappLI
描    述:	对象类型属性
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    public class ObjectTypeAttribute : Attribute
    {
#if UNITY_EDITOR
        public System.Type objType;
        public bool bBitOffset = false;
#endif
        public ObjectTypeAttribute(System.Type objType = null)
        {
#if UNITY_EDITOR
            this.objType = objType==null? typeof(UnityEngine.Object): objType;
#endif
        }
    }
    public class ObjPathFieldAttribute : Attribute
    {
#if UNITY_EDITOR
        public string fieldName;
#endif
        public ObjPathFieldAttribute(string fieldName)
        {
#if UNITY_EDITOR
            this.fieldName = fieldName;
#endif
        }
    }
}