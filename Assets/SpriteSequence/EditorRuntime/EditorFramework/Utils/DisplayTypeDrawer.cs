/********************************************************************
生成日期:		11:07:2020
类    名: 	DisplayDrawxxx
作    者:	HappLI
描    述:	Inspector 显示绘制
*********************************************************************/
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Framework.DrawProps
{
    public class DisplayDrawTypeAttribute : UnityEngine.PropertyAttribute
    {
#if UNITY_EDITOR
        public System.Type m_drawType;
        private string m_DrawTypeName;
        public bool bEnumBitOffset = false;
        public string userMethod;
        public System.Type callType;
#endif
        public DisplayDrawTypeAttribute(System.Type drawType, bool bEnumBitOffset = false, int order =0)
        {
#if UNITY_EDITOR
            this.m_drawType = drawType;
            this.bEnumBitOffset = bEnumBitOffset;
            this.order = order;
            m_DrawTypeName = null;
#endif
        }
        public DisplayDrawTypeAttribute(string drawType, bool bEnumBitOffset = false, int order = 0)
        {
#if UNITY_EDITOR
            m_DrawTypeName = drawType;
            this.bEnumBitOffset = bEnumBitOffset;
            this.m_drawType = null;
            this.order = order;
#endif
        }
        public DisplayDrawTypeAttribute(System.Type drawType, string userMethod, System.Type callType = null)
        {
#if UNITY_EDITOR
            m_DrawTypeName = null;
            this.m_drawType = drawType;
            this.order = order;
            this.userMethod = userMethod;
            this.callType = callType;
#endif
        }

#if UNITY_EDITOR
        public System.Type GetDisplayType()
        {
            if (m_drawType != null) return m_drawType;
            if (!string.IsNullOrEmpty(m_DrawTypeName))
            {
                m_drawType = ED.EditorUtils.GetTypeByName(m_DrawTypeName);
            }
            return m_drawType;
        }
        //-----------------------------------------------------
        System.Reflection.MethodInfo m_pMethod = null;
        public bool CallUserMethod(ref System.Object pData, System.Reflection.FieldInfo fieldInfo)
        {
            if(m_pMethod == null && !string.IsNullOrEmpty(userMethod))
            {
                if (callType != null)
                {
                    m_pMethod = callType.GetMethod(userMethod, BindingFlags.Instance| BindingFlags.Static| BindingFlags.Public| BindingFlags.NonPublic);
                }
                else
                {
                    m_pMethod = callType.GetMethod(userMethod, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if(m_pMethod == null)
                        m_pMethod = callType.GetMethod(userMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }
            }
            if (m_pMethod == null)
                return false;
            if (m_pMethod.ReturnType != pData.GetType())
                return false;

            if(m_pMethod.IsStatic)
            {
                if (m_pMethod.GetParameters().Length != 2)
                    return false;
                pData = m_pMethod.Invoke(null, new object[] { pData, fieldInfo });
            }
            else
            {
                pData = m_pMethod.Invoke(pData, new object[] { fieldInfo });
            }
            return true;
        }
#endif
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DisplayDrawTypeAttribute))]
    class DisplayDrawTypeDrawer : PropertyDrawer
    {
        public List<string> EnumPops = new List<string>();
        public List<System.Enum> EnumValuePops = new List<System.Enum>();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float width = position.width;
            position.width = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(position, label);
            position.x += position.width;

            DisplayDrawTypeAttribute drawTypeAttr = this.attribute as DisplayDrawTypeAttribute;
            if(drawTypeAttr != null)
            {
                System.Type displayType = drawTypeAttr.GetDisplayType();
                if(displayType!=null)
                {
                    if(displayType.IsEnum)
                    {
                        EnumPops.Clear();
                        EnumValuePops.Clear();
                        foreach (System.Enum v in System.Enum.GetValues(displayType))
                        {
                            System.Reflection.FieldInfo fi = displayType.GetField(v.ToString());
                            string strTemName = v.ToString();
                            if (fi != null && fi.IsDefined(typeof(DrawProps.DisableAttribute)))
                            {
                                continue;
                            }
                            if (fi != null && fi.IsDefined(typeof(Framework.DrawProps.DisplayAttribute)))
                            {
                                strTemName = fi.GetCustomAttribute<Framework.DrawProps.DisplayAttribute>().displayName;
                            }
                            EnumPops.Add(strTemName);
                            EnumValuePops.Add(v);
                        }

                        if (property.propertyType == SerializedPropertyType.Integer)
                        {
                            position.width = (width - position.width) / 1.5f;

                            System.Enum curVar = (System.Enum)System.Enum.ToObject(displayType, property.intValue);
                            int index = EditorGUI.Popup(position, EnumValuePops.IndexOf(curVar), EnumPops.ToArray());
                            if (index >= 0 && index < EnumValuePops.Count)
                                curVar = EnumValuePops[index];
                            property.intValue = System.Convert.ToInt32(curVar);
                        }
                    }
                }
            }
            position.x += position.width;
            EditorGUI.PropertyField(position, property, GUIContent.none);
        }
    }
#endif
}