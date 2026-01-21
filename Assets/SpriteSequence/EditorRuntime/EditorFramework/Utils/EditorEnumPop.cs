/********************************************************************
生成日期:		11:07:2020
类    名: 	EditorEnumPop
作    者:	HappLI
描    述:	枚举类型绘制
*********************************************************************/
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Framework.ED
{
    public class EditorEnumPop
    {
        public static List<string> EnumPops = new List<string>();
        public static List<Enum> EnumValuePops = new List<Enum>();
        //-----------------------------------------------------
        public static int PopEnum(string strDisplayName, int curVar, System.Type enumType, GUILayoutOption[] layOps = null, bool bIndexAdd = false)
        {
            Enum val = (Enum)Enum.ToObject(enumType, curVar);
            val = PopEnum(new GUIContent(strDisplayName), val, enumType, layOps, bIndexAdd);
            curVar = Convert.ToInt32(val);
            return curVar;
        }
        //-----------------------------------------------------
        public static int PopEnum(GUIContent strDisplayName, int curVar, System.Type enumType, GUILayoutOption[] layOps = null, bool bIndexAdd = false)
        {
            Enum val = (Enum)Enum.ToObject(enumType, curVar);
            val = PopEnum(strDisplayName, val, enumType, layOps, bIndexAdd);
            curVar = Convert.ToInt32(val);
            return curVar;
        }
        //-----------------------------------------------------
        public static int PopEnum(string strDisplayName, int curVar, string enumTypeName, GUILayoutOption[] layOps = null, bool bIndexAdd = false)
        {
            return PopEnum(new GUIContent(strDisplayName), curVar, enumTypeName, layOps, bIndexAdd);
        }
        //-----------------------------------------------------
        public static int PopEnum(GUIContent strDisplayName, int curVar, string enumTypeName, GUILayoutOption[] layOps = null, bool bIndexAdd = false)
        {
            System.Type enumType = ED.EditorUtils.GetTypeByName(enumTypeName);
            if (enumType == null)
            {
                if (strDisplayName==null || string.IsNullOrEmpty(strDisplayName.text))
                    curVar = EditorGUILayout.IntField(curVar, layOps);
                else
                    curVar = EditorGUILayout.IntField(strDisplayName, curVar, layOps);
                return curVar;
            }
            Enum val = (Enum)Enum.ToObject(enumType, curVar);
            val = PopEnum(strDisplayName, val, enumType, layOps, bIndexAdd);
            curVar = Convert.ToInt32(val);
            return curVar;
        }
        //-----------------------------------------------------
        internal static bool IsExpandByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            bool bEpxand = false;
            if (ms_vEnumBitExpands.TryGetValue(name, out bEpxand))
                return bEpxand;
            return false;
        }
        //-----------------------------------------------------
        internal static bool IsExpandByName(GUIContent name)
        {
            if (name == null || string.IsNullOrEmpty(name.text)) return false;
            bool bEpxand = false;
            if (ms_vEnumBitExpands.TryGetValue(name.text, out bEpxand))
                return bEpxand;
            return false;
        }
        //-----------------------------------------------------
        internal static void SetExpandByName(string name, bool bEpxand)
        {
            if (string.IsNullOrEmpty(name)) return;
            ms_vEnumBitExpands[name] = bEpxand;
        }
        //-----------------------------------------------------
        internal static void SetExpandByName(GUIContent name, bool bEpxand)
        {
            if (name == null || string.IsNullOrEmpty(name.text)) return;
            ms_vEnumBitExpands[name.text] = bEpxand;
        }
        //-----------------------------------------------------
        static Dictionary<string, bool> ms_vEnumBitExpands = new Dictionary<string, bool>();
        public static int PopEnumBit(string strDispalayName, int flag, System.Type enumType, GUILayoutOption[] layOps = null,bool bEnumBitOffset = false)
        {
            return PopEnumBit(new GUIContent(strDispalayName), flag, enumType, layOps, bEnumBitOffset);
        }
        //-----------------------------------------------------
        public static int PopEnumBit(GUIContent strDispalayName, int flag, System.Type enumType, GUILayoutOption[] layOps = null, bool bEnumBitOffset = false)
        {
            bool bEpxand = IsExpandByName(strDispalayName);
            bEpxand = EditorGUILayout.Foldout(bEpxand, strDispalayName);
            SetExpandByName(strDispalayName, bEpxand);
            if (bEpxand)
            {
                EditorGUI.indentLevel++;
                foreach (Enum v in Enum.GetValues(enumType))
                {
                    string strName = Enum.GetName(enumType, v);
                    FieldInfo fi = enumType.GetField(strName);
                    if (fi.IsDefined(typeof(Framework.DrawProps.DisableAttribute)))
                    {
                        continue;
                    }
                    int flagValue = Convert.ToInt32(v);
                    if (bEnumBitOffset) flagValue = 1 << flagValue;
                    if (fi.IsDefined(typeof(Framework.DrawProps.DisplayAttribute)))
                    {
                        strName = fi.GetCustomAttribute<Framework.DrawProps.DisplayAttribute>().displayName;
                    }
                    if (fi.IsDefined(typeof(InspectorNameAttribute)))
                    {
                        strName = fi.GetCustomAttribute<InspectorNameAttribute>().displayName;
                    }
                    if (fi.IsDefined(typeof(DrawProps.StateByTypeAttribute)))
                    {
                        DrawProps.StateByTypeAttribute typeSets = fi.GetCustomAttribute<DrawProps.StateByTypeAttribute>();
                        if (!typeSets.typeSets.Contains(enumType))
                        {
                            continue;
                        }
                    }

                    bool bToggle = EditorGUILayout.Toggle(strName, (flag & flagValue) != 0);
                    if (bToggle)
                    {
                        flag |= (int)flagValue;
                    }
                    else flag &= (int)(~flagValue);
                }
                EditorGUI.indentLevel--;
            }
            return flag;
        }
        //-----------------------------------------------------
        public static Enum PopEnum(string strDisplayName, Enum curVar, System.Type enumType = null, GUILayoutOption[] layOps = null, bool bIndexAdd = false)
        {
            return PopEnum(new GUIContent(strDisplayName), curVar, enumType, layOps, bIndexAdd);
        }
        //-----------------------------------------------------
        public static Enum PopEnum(GUIContent strDisplayName, Enum curVar, System.Type enumType = null, GUILayoutOption[] layOps = null, bool bIndexAdd = false)
        {
            if (enumType == null)
                enumType = curVar.GetType();
            EnumPops.Clear();
            EnumValuePops.Clear();
            int index = -1;
            foreach (Enum v in Enum.GetValues(enumType))
            {
                FieldInfo fi = enumType.GetField(v.ToString());
                string strTemName = v.ToString();
                if (fi != null && fi.IsDefined(typeof(Framework.DrawProps.DisableAttribute)))
                    continue;
                if (fi != null && fi.IsDefined(typeof(Framework.DrawProps.DisplayAttribute)))
                {
                    strTemName = fi.GetCustomAttribute<Framework.DrawProps.DisplayAttribute>().displayName;
                }
                if (fi != null && fi.IsDefined(typeof(InspectorNameAttribute)))
                {
                    strTemName = fi.GetCustomAttribute<InspectorNameAttribute>().displayName;
                }
                if (bIndexAdd) strTemName += "[" + Convert.ToInt32(v).ToString() + "]";
                EnumPops.Add(strTemName);
                EnumValuePops.Add(v);
                if (v.ToString().CompareTo(curVar.ToString()) == 0)
                    index = EnumPops.Count - 1;
            }
            if (EnumPops.Count > 10 && !enumType.IsDefined(typeof(Framework.DrawProps.UnFilterAttribute), false))
            {
                //filter
                for (int i = 0; i < EnumPops.Count; ++i)
                {
                    if (EnumPops[i].Contains("/")) continue;
                    string filter = EnumValuePops[i].ToString().Substring(0, 1).ToUpper();
                    EnumPops[i] = filter + "/" + EnumPops[i];
                }
            }
            if (strDisplayName == null || string.IsNullOrEmpty(strDisplayName.text))
                index = EditorGUILayout.Popup(index, EnumPops.ToArray(), layOps);
            else
            {
                index = EditorGUILayout.Popup(strDisplayName, index, EnumPops.ToArray(), layOps);
            }
            if (index >= 0 && index < EnumValuePops.Count)
            {
                curVar = EnumValuePops[index];
            }
            EnumPops.Clear();
            EnumValuePops.Clear();

            return curVar;
        }
    }
}
#endif