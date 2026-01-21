/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	DisplayAttribute
作    者:	HappLI
描    述:	显示名称
*********************************************************************/
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Framework.DrawProps
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public class DisplayAttribute : UnityEngine.PropertyAttribute
    {
#if UNITY_EDITOR
        public string strTips = "";
        public string displayName { get; set; }
#endif
        public DisplayAttribute()
        {
#if UNITY_EDITOR
            this.displayName = null;
#endif
        }
        public DisplayAttribute(string displayName)
        {
#if UNITY_EDITOR
this.displayName = displayName;
#endif
        }
        public DisplayAttribute(string displayName, string tip)
        {
#if UNITY_EDITOR
            this.strTips = tip;
            this.displayName = displayName;
#endif
        }
    }
    //------------------------------------------------------
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public class DisplayLabelWidthAttribute : UnityEngine.PropertyAttribute
    {
#if UNITY_EDITOR
        public float labelWidth;
#endif
        public DisplayLabelWidthAttribute(float labelWidth)
        {
#if UNITY_EDITOR
            this.labelWidth = labelWidth;
#endif
        }
    }
    //------------------------------------------------------
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(DisplayAttribute))]
    public class PropertyNameAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DisplayAttribute disp = (attribute as DisplayAttribute);
            EditorGUI.PropertyField(position, property, new GUIContent(disp.displayName, disp.strTips), property.hasChildren);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);
            if (property.isExpanded)
            {
                if (property.propertyType == SerializedPropertyType.Generic)
                {
                    return baseHeight + EditorGUIUtility.singleLineHeight * property.CountInProperty();
                }
            }
            return baseHeight;
        }
    }
#endif
}