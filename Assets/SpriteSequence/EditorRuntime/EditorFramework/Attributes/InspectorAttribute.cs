/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	InspectorAttribute
作    者:	HappLI
描    述:	自定义Inspector绘制
*********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Framework.DrawProps
{   
	[AttributeUsage(AttributeTargets.Class)]
    public class InspectorAttribute : Attribute
    {
#if UNITY_EDITOR
        public System.Type classType;
        public string method = "DrawInspector";
#endif
        public InspectorAttribute(System.Type classType, string method = "DrawInspector")
        {
#if UNITY_EDITOR
            this.classType = classType;
            this.method = method;
#endif
        }
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class AddInspectorAttribute : Attribute
    {
    }
    //-----------------------------------------------------
    [AttributeUsage(AttributeTargets.Field)]
    public class RowFieldInspectorAttribute : Attribute
    {
#if UNITY_EDITOR
        public System.Type classType;
        public string method = "OnDrawFieldLineRow";
        MethodInfo m_method = null;

#endif
        public RowFieldInspectorAttribute(string method = "OnDrawFieldLineRow", System.Type classType = null)
        {
#if UNITY_EDITOR
            this.method = method;
            this.classType = classType;
#endif
        }
#if UNITY_EDITOR
        //-----------------------------------------------------
        public System.Object OnDrawLineRow(System.Object ownerData, System.Reflection.FieldInfo fieldInfo, System.Object parentData, System.Reflection.FieldInfo parentFieldInfo)
        {
            if(m_method == null)
            {
                System.Type ownerType = classType;
                if (ownerType == null) ownerType = fieldInfo.DeclaringType;
                m_method = ownerType.GetMethod(method, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                if (m_method == null)
                {
                    m_method = ownerType.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                }
                if (m_method == null)
                    m_method = ownerType.GetMethod(method, BindingFlags.Public | BindingFlags.Instance);
                if (m_method == null)
                    m_method = ownerType.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance);
            }
            if (m_method == null)
                return ownerData;

            System.Object pCall = ownerData;
            if (m_method.IsStatic) pCall = null;
            if (m_method.ReturnType == ownerData.GetType())
            {
                if (m_method.GetParameters().Length == 1)
                    ownerData = m_method.Invoke(pCall, new object[] { fieldInfo });
                else if (m_method.GetParameters().Length == 2)
                    ownerData = m_method.Invoke(pCall, new object[] { ownerData, fieldInfo });
                else if (m_method.GetParameters().Length == 3)
                    ownerData = m_method.Invoke(pCall, new object[] { ownerData, fieldInfo, parentData });
                else if (m_method.GetParameters().Length == 4)
                    ownerData = m_method.Invoke(pCall, new object[] { ownerData, fieldInfo, parentData, parentFieldInfo });
            }
            else
            {
                if (m_method.GetParameters().Length == 1)
                    m_method.Invoke(pCall, new object[] { fieldInfo });
                else if (m_method.GetParameters().Length == 2)
                    m_method.Invoke(pCall, new object[] { ownerData, fieldInfo });
                else if (m_method.GetParameters().Length == 3)
                    m_method.Invoke(pCall, new object[] { ownerData, fieldInfo, parentData });
                else if (m_method.GetParameters().Length == 4)
                    m_method.Invoke(pCall, new object[] { ownerData, fieldInfo, parentData, parentFieldInfo });
            }
            return ownerData;
        }
#endif
    }
    //-----------------------------------------------------
    [AttributeUsage(AttributeTargets.Field)]
    public class FieldInspectorAttribute : Attribute
    {
#if UNITY_EDITOR
        public System.Type classType;
        public string method = "OnFieldInspector";
        MethodInfo m_method = null;

#endif
        public FieldInspectorAttribute(string method = "OnFieldInspector", System.Type classType = null)
        {
#if UNITY_EDITOR
            this.method = method;
            this.classType = classType;
#endif
        }
#if UNITY_EDITOR
        //-----------------------------------------------------
        public System.Object OnInspector(System.Object ownerData, System.Reflection.FieldInfo fieldInfo, System.Object parentData, System.Reflection.FieldInfo parentFieldInfo)
        {
            if (m_method == null)
            {
                System.Type ownerType = classType;
                if (ownerType == null) ownerType = fieldInfo.DeclaringType;
                m_method = ownerType.GetMethod(method, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                if (m_method == null)
                {
                    m_method = ownerType.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                }
                if (m_method == null)
                    m_method = ownerType.GetMethod(method, BindingFlags.Public | BindingFlags.Instance);
                if (m_method == null)
                    m_method = ownerType.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance);
            }
            if (m_method == null)
                return ownerData;

            System.Object pCall = ownerData;
            if (m_method.IsStatic) pCall = null;
            if (m_method.ReturnType == ownerData.GetType())
            {
                if (m_method.GetParameters().Length == 1)
                    ownerData = m_method.Invoke(pCall, new object[] { fieldInfo });
                else if (m_method.GetParameters().Length == 2)
                    ownerData = m_method.Invoke(pCall, new object[] { ownerData, fieldInfo });
                else if (m_method.GetParameters().Length == 3)
                    ownerData = m_method.Invoke(pCall, new object[] { ownerData, fieldInfo, parentData });
                else if (m_method.GetParameters().Length == 4)
                    ownerData = m_method.Invoke(pCall, new object[] { ownerData, fieldInfo, parentData, parentFieldInfo });
            }
            else
            {
                if (m_method.GetParameters().Length == 1)
                    m_method.Invoke(pCall, new object[] { fieldInfo });
                else if (m_method.GetParameters().Length == 2)
                    m_method.Invoke(pCall, new object[] { ownerData, fieldInfo });
                else if (m_method.GetParameters().Length == 3)
                    m_method.Invoke(pCall, new object[] { ownerData, fieldInfo, parentData });
                else if (m_method.GetParameters().Length == 4)
                    m_method.Invoke(pCall, new object[] { ownerData, fieldInfo, parentData, parentFieldInfo });
            }
            return ownerData;
        }
#endif
    }
}