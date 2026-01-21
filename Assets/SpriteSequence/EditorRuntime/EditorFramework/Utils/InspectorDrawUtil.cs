/********************************************************************
生成日期:		11:10:2020
类    名: 	HandleUtilityWrapper
作    者:	HappLI
描    述:	属性绘制工具
*********************************************************************/
#if UNITY_EDITOR
using Framework.DrawProps;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Framework.ED
{
    public interface UndoHandler
    {
        void RegisterUndoData(object data);
    }

    public static class InspectorDrawUtil
    {
        public delegate void OnFieldDirtyDelegate(object data, FieldInfo field, object prevValue);
        struct FieldScope : IDisposable
        {
            bool bEnable;
            System.Object data;
            System.Object prevData;
            FieldInfo field;
            OnFieldDirtyDelegate onDirty;
			float lastLabelWidth;
            public FieldScope(System.Object data, FieldInfo field, bool bEnable, OnFieldDirtyDelegate onDirty)
            {
                this.data = data;
                this.prevData = field.GetValue(data);
                this.field = field;
                this.bEnable = bEnable;
                this.onDirty = onDirty;
                EditorGUI.BeginDisabledGroup(!bEnable);
                EditorGUI.BeginChangeCheck();
				lastLabelWidth = EditorGUIUtility.labelWidth;
				if(field.IsDefined(typeof(DisplayLabelWidthAttribute)))
				{
					float labelWidth = field.GetCustomAttribute<DisplayLabelWidthAttribute>().labelWidth;
					if (labelWidth >= 0)
						EditorGUIUtility.labelWidth = labelWidth;
                }
            }
            public void Dispose()
            {
				EditorGUIUtility.labelWidth = lastLabelWidth;
                if (EditorGUI.EndChangeCheck())
                {
                    if (ms_UndoHandler != null)
                    {
                        var curData = field.GetValue(data);
						if (!ms_bInnerCallUndo)
						{
							field.SetValue(data, prevData);
							ms_UndoHandler.RegisterUndoData(data);
						}
                        field.SetValue(data, curData);
                    }
                    ms_bChangeChecked = true;
					ms_bInnerCallUndo = false;
                    if (onDirty != null) onDirty(data, field, prevData);
                }
                EditorGUI.EndDisabledGroup();
            }
        }
        public static List<string> BindSlots = new List<string>();
        //-----------------------------------------------------
        struct TempOrderAttr
        {
            public int order;
            public System.Attribute attr;
        }
        static List<TempOrderAttr> ms_TempOrderAttrs = new List<TempOrderAttr>();
        struct PluginInspector
        {
            public System.Type classType;
            public MethodInfo method;
            public Framework.DrawProps.InspectorAttribute attr;
        }
        //-----------------------------------------------------
        static Dictionary<System.Type, PluginInspector> ms_PluginInspectors = new Dictionary<System.Type, PluginInspector>();
        static Dictionary<string, PluginInspector> ms_StringPluginInspectors = new Dictionary<string, PluginInspector>();
        static bool m_bInitedCheckInspector = false;
        static bool ms_bChangeChecked = false;
		static bool ms_bInnerCallUndo = false;
        static UndoHandler ms_UndoHandler = null;
        public static void CheckInspector()
        {
            if (ms_PluginInspectors.Count > 0 || m_bInitedCheckInspector) return;
            m_bInitedCheckInspector = true;
            ms_PluginInspectors.Clear();
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types = assembly.GetTypes();
                for (int i = 0; i < types.Length; ++i)
                {
                    Type tp = types[i];
                    if (tp.IsDefined(typeof(Framework.DrawProps.InspectorAttribute), false))
                    {
                        Framework.DrawProps.InspectorAttribute pAttr = tp.GetCustomAttribute<Framework.DrawProps.InspectorAttribute>();
                        if (pAttr.classType != null)
                        {
                            PluginInspector inspector = new PluginInspector() { classType = tp, attr = pAttr };
                            inspector.method = tp.GetMethod(pAttr.method, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                            if(inspector.method==null)
                            {
                                inspector.method = tp.GetMethod(pAttr.method, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                            }
                            if (inspector.method != null)
                            {
                                ms_PluginInspectors[pAttr.classType] = inspector;
                                ms_StringPluginInspectors[pAttr.classType.FullName.Replace("+", ".").ToLower()] = inspector;
                            }
                        }
                    }
                    if (tp.IsDefined(typeof(Framework.DrawProps.StringViewPluginAttribute), false))
                    {
                        Framework.DrawProps.StringViewPluginAttribute pAttr = tp.GetCustomAttribute<Framework.DrawProps.StringViewPluginAttribute>();
                        if (!string.IsNullOrEmpty(pAttr.userPlugin))
                        {
                            var method = tp.GetMethod(pAttr.userPlugin, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                            if (method == null)
                            {
                                method = tp.GetMethod(pAttr.userPlugin, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                            }
                            if (method != null && method.GetParameters()!=null &&
                                method.GetParameters().Length>0)
                            {
								if(method.GetParameters()[0].ParameterType == method.ReturnType)
								{
                                    PluginInspector pluginInspector = new PluginInspector();
                                    pluginInspector.method = method;
                                    ms_StringPluginInspectors[pAttr.userPlugin.ToLower()] = pluginInspector;
                                }
                            }
                        }
                    }
                }
            }
        }
        //-----------------------------------------------------
        public static bool DrawStringUserPlugin(System.Object pData, System.Reflection.FieldInfo fieldInfo, GUIContent strDisplayName, ref string strValue, string pluginDraw)
        {
            CheckInspector();
            PluginInspector pluginInsepcotr;
            if(ms_StringPluginInspectors.TryGetValue(pluginDraw.ToLower(), out pluginInsepcotr))
            {
                if(pluginInsepcotr.method!=null && (pluginInsepcotr.method.ReturnType == typeof(string) || pluginInsepcotr.method.ReturnType == typeof(System.Object)))
                {
                    var parames = pluginInsepcotr.method.GetParameters();
                    if (parames[0].ParameterType == typeof(System.Object))
                    {
						if (parames.Length == 3)
						{
                            if (parames[2].ParameterType == typeof(GUIContent))
                                pData = pluginInsepcotr.method.Invoke(null, new object[] { pData, fieldInfo, strDisplayName });
							else
                                pData = pluginInsepcotr.method.Invoke(null, new object[] { pData, fieldInfo, strDisplayName.text });
                        }
                        else if (parames.Length == 2)
                        {
							pData = pluginInsepcotr.method.Invoke(null, new object[] { pData, fieldInfo });
						}
                        var strObj = fieldInfo.GetValue(pData);
						if (strObj != null) strValue = strObj.ToString();
						else strValue = "";
                    }
                    else
                    {
                        if (parames.Length == 2)
                        {
                            if (parames[1].ParameterType == typeof(GUIContent))
                                strValue = (string)pluginInsepcotr.method.Invoke(null, new object[] { strValue, strDisplayName });
                            else
                                strValue = (string)pluginInsepcotr.method.Invoke(null, new object[] { strValue, strDisplayName.text });
                        }
                        else
                        {
                            strValue = (string)pluginInsepcotr.method.Invoke(null, new object[] { strValue });
                        }
                    }

                    return true;
                }
            }
            return false;
        }
        //-----------------------------------------------------
        public static bool DrawStringUserPlugin(System.Object pData, System.Reflection.FieldInfo fieldInfo, GUIContent strDisplayName, ref string strValue, System.Type pluginDraw)
        {
            CheckInspector();
            PluginInspector pluginInsepcotr;
            if (ms_StringPluginInspectors.TryGetValue(pluginDraw.FullName.Replace("+",".").ToLower(), out pluginInsepcotr))
            {
                if (pluginInsepcotr.method != null && pluginInsepcotr.method.ReturnType == typeof(string))
                {
					var parames= pluginInsepcotr.method.GetParameters();
                    if (parames[0].ParameterType == typeof(System.Object))
                    {
                        if (parames.Length == 3)
                        {
                            if (parames[2].ParameterType == typeof(GUIContent))
                                pData = pluginInsepcotr.method.Invoke(null, new object[] { pData, fieldInfo, strDisplayName });
                            else
                                pData = pluginInsepcotr.method.Invoke(null, new object[] { pData, fieldInfo, strDisplayName.text });
                        }
                        else if(parames.Length ==2)
                        {
                            pData = pluginInsepcotr.method.Invoke(null, new object[] { pData, fieldInfo });
                        }
                        var strObj = fieldInfo.GetValue(pData);
                        if (strObj != null) strValue = strObj.ToString();
                        else strValue = "";
                    }
                    else
                    {
                        if (parames.Length == 2)
                        {
                            if (parames[1].ParameterType == typeof(GUIContent))
                                strValue = (string)pluginInsepcotr.method.Invoke(null, new object[] { strValue, strDisplayName });
                            else
                                strValue = (string)pluginInsepcotr.method.Invoke(null, new object[] { strValue, strDisplayName.text });
                        }
                        else
                        {
                            strValue = (string)pluginInsepcotr.method.Invoke(null, new object[] { strValue });
                        }
                    }

                    return true;
                }
            }
            return false;
        }
        //-----------------------------------------------------
        public static bool DrawPluginInspector(ref System.Object data, System.Object parentData, System.Reflection.FieldInfo parentFieldInfo)
        {
            CheckInspector();
            PluginInspector inspector;
            if (ms_PluginInspectors.TryGetValue(data.GetType(), out inspector))
            {
                if (inspector.method != null)
                {
					if (inspector.method.ReturnType != null && inspector.method.ReturnType == data.GetType())
					{
						if (inspector.method.GetParameters().Length == 1)
							data = inspector.method.Invoke(null, new object[] { data });
						else if (inspector.method.GetParameters().Length == 2)
							data = inspector.method.Invoke(null, new object[] { data, parentData });
                        else if (inspector.method.GetParameters().Length == 3)
                            data = inspector.method.Invoke(null, new object[] { data, parentData, parentFieldInfo });
                    }
					else
					{
                        if (inspector.method.GetParameters().Length == 1)
                            inspector.method.Invoke(null, new object[] { data });
                        else if (inspector.method.GetParameters().Length == 2)
							inspector.method.Invoke(null, new object[] { data, parentData });
                        else if (inspector.method.GetParameters().Length == 3)
                            inspector.method.Invoke(null, new object[] { data, parentData, parentFieldInfo });
                    }
                    return true;
                }
            }
            return false;
        }
        //------------------------------------------------------
        private static System.Type realType;
        private static PropertyInfo s_property_handleWireMaterial;

        private static void InitType()
        {
            if (realType == null)
            {
                realType = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.HandleUtility");
                s_property_handleWireMaterial = realType.GetProperty("handleWireMaterial", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            }
        }

        public static Material handleWireMaterial
        {
            get
            {
                InitType();
                return (s_property_handleWireMaterial.GetValue(null, null) as Material);
            }
        }
        static object ms_pCopyData = null;
        //-----------------------------------------------------
        public static int PopEnum(GUIContent strDisplayName, int curVar, System.Type enumType, GUILayoutOption[] layOps = null, bool bIndexAdd = false)
        {
            return EditorEnumPop.PopEnum(strDisplayName, curVar, enumType, layOps, bIndexAdd);
        }
        //-----------------------------------------------------
        public static int PopEnum(string strDisplayName, int curVar, string enumTypeName, GUILayoutOption[] layOps = null, bool bIndexAdd = false)
        {
            return EditorEnumPop.PopEnum(strDisplayName, curVar, enumTypeName, layOps, bIndexAdd);
        }
        //-----------------------------------------------------
        public static Enum PopEnum(string strDisplayName, Enum curVar, System.Type enumType = null, GUILayoutOption[] layOps = null, bool bIndexAdd = false)
        {
            return EditorEnumPop.PopEnum(strDisplayName, curVar, enumType, layOps, bIndexAdd);
        }
        //-----------------------------------------------------
        public static Enum PopEnum(GUIContent strDisplayName, Enum curVar, System.Type enumType = null, GUILayoutOption[] layOps = null, bool bIndexAdd = false)
        {
            return EditorEnumPop.PopEnum(strDisplayName, curVar, enumType, layOps, bIndexAdd);
        }
        //-----------------------------------------------------
        public static string[] RENDER_LAYERS_POP = new string[] { "BackLayer", "MiddleLayer", "ForeLayer", "EffectLayer", "UI", "UI_3D", "UIBG_3D" };
        public static int[] RENDER_LAYERS_VALUE = new int[]
        {
            LayerMask.NameToLayer(RENDER_LAYERS_POP[0]),
            LayerMask.NameToLayer(RENDER_LAYERS_POP[1]),
            LayerMask.NameToLayer(RENDER_LAYERS_POP[2]),
            LayerMask.NameToLayer(RENDER_LAYERS_POP[3]),
            LayerMask.NameToLayer(RENDER_LAYERS_POP[4]),
            LayerMask.NameToLayer(RENDER_LAYERS_POP[5]),
            LayerMask.NameToLayer(RENDER_LAYERS_POP[6]),
        };
        public static int PopRenderLayer(GUIContent strDisplayName, int curVar, GUILayoutOption[] layOps = null)
        {
            int index = -1;
            for(int i = 0; i < RENDER_LAYERS_VALUE.Length; ++i)
            {
                if(RENDER_LAYERS_VALUE[i] == curVar)
                {
                    index = i;
                }
            }
            if (strDisplayName == null || string.IsNullOrEmpty(strDisplayName.text))
                index = EditorGUILayout.Popup(index, RENDER_LAYERS_POP, layOps);
            else
                index = EditorGUILayout.Popup(strDisplayName, index, RENDER_LAYERS_POP, layOps);
            if (index >= 0 && index < RENDER_LAYERS_VALUE.Length)
            {
                curVar = RENDER_LAYERS_VALUE[index];
            }
            return curVar;
        }
        //------------------------------------------------------
        public static int PopRenderLayerMask(string strDisplayName, int curVar, GUILayoutOption[] layOps = null)
        {
            if(!string.IsNullOrEmpty(strDisplayName))
                EditorGUILayout.LabelField(strDisplayName);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < RENDER_LAYERS_POP.Length; ++i)
            {
                int mask = LayerMask.NameToLayer(RENDER_LAYERS_POP[i]);
                bool toggle = EditorGUILayout.Toggle(RENDER_LAYERS_POP[i], (curVar & (1 << mask)) != 0);
                if (toggle) curVar |= 1 << mask;
                else curVar &= ~(1 << mask);
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
            return curVar;
        }
        //------------------------------------------------------
        public static int DrawBitView(GUIContent strDispalyName, int nValue, DrawProps.BitViewAttribute[] bitAttrs)
        {
            EditorGUILayout.BeginVertical();
            bool bEpxand = EditorEnumPop.IsExpandByName(strDispalyName);
            bEpxand = EditorGUILayout.Foldout(bEpxand, strDispalyName);
            EditorEnumPop.SetExpandByName(strDispalyName, bEpxand);
            if(bEpxand)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < bitAttrs.Length; ++i)
                {
                    bool toggle = EditorGUILayout.Toggle(bitAttrs[i].displayName, (nValue & (1 << bitAttrs[i].offset)) != 0);
                    if (toggle) nValue |= 1 << bitAttrs[i].offset;
                    else nValue &= ~(1 << bitAttrs[i].offset);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            return nValue;
        }
        //-----------------------------------------------------
        static HashSet<string> ms_vFilterFields = new HashSet<string>();
        static Dictionary<System.Type, int> ms_vTypeOrder = new Dictionary<Type, int>();
		static Stack<List<FieldInfo>> ms_vDrawFields = new Stack<List<FieldInfo>>();
        public static System.Object DrawProperty(System.Object data, BindingFlags flags, List<string> IngoreFields = null,
            System.Action<string> OnSubDraw = null, OnFieldDirtyDelegate OnChangeField = null,
			System.Object parentData = null, System.Reflection.FieldInfo parentFieldInfo = null)
        {
            if (DrawPluginInspector(ref data, parentData, parentFieldInfo))
                return data;
            FieldInfo[] finfos = data.GetType().GetFields(flags);

			List<FieldInfo> drawFields = null;
			if (ms_vDrawFields.Count > 0) drawFields = ms_vDrawFields.Pop();
			else drawFields = new List<FieldInfo>(finfos.Length);
            drawFields.Clear();
            ms_vFilterFields.Clear();
			ms_vTypeOrder.Clear();

            var curType = data.GetType();
            while(curType!=null)
            {
                ms_vTypeOrder[curType] = ms_vTypeOrder.Count;
                curType = curType.BaseType;
            }

            for (int i = 0; i < finfos.Length; ++i)
            {
                if (IngoreFields != null && IngoreFields.Contains(finfos[i].Name))
                    continue;
                if (ms_vFilterFields.Contains(finfos[i].Name))
                    continue;

                if (!finfos[i].IsDefined(typeof(ObjPathFieldAttribute), false))
                {
                    if (finfos[i].IsNotSerialized)
                        continue;
                }
				if(finfos[i].IsPrivate)
				{
					if(!finfos[i].IsDefined(typeof(SerializeField), false))
					{
						continue;
					}
				}
                drawFields.Add(finfos[i]);
            }

            drawFields.Sort((f1, f2) => {
                ms_vTypeOrder.TryGetValue(f1.DeclaringType, out var order1);
                ms_vTypeOrder.TryGetValue(f2.DeclaringType, out var order2);
                if (order1 != order2)
                    return order2 - order1; // 父类在前
                                            // 同一类声明，按声明顺序
                return f1.MetadataToken.CompareTo(f2.MetadataToken);
            });

            foreach (var db in drawFields)
            {
                data = DrawPropertyField(data, db, OnSubDraw, null, OnChangeField);
            }

			var methods = data.GetType().GetMethods(BindingFlags.Instance|BindingFlags.Public| BindingFlags.NonPublic| BindingFlags.DeclaredOnly);
			if(methods!=null)
			{
                for(int i = 0; i < methods.Length; ++i)
				{
					if ( methods[i].IsDefined(typeof(DrawProps.AddInspectorAttribute), false))
					{
						 methods[i].Invoke(data, null);
                    }
                }
            }

			drawFields.Clear();
			if(ms_vDrawFields.Count<16) ms_vDrawFields.Push(drawFields);
            return data;
        }
        //-----------------------------------------------------
        public static void BeginChangeCheck(UndoHandler handle = null)
        {
            ms_UndoHandler = handle;
            ms_bChangeChecked = false;
        }
        //-----------------------------------------------------
        public static bool EndChangeCheck()
        {
            bool bChange = ms_bChangeChecked;
            ms_bChangeChecked = false;
            ms_UndoHandler = null;
            return bChange;
        }
        //-----------------------------------------------------
		public static void RegisterUndo(System.Object data,string stepName="")
		{
			if (ms_UndoHandler != null)
			{
				ms_UndoHandler.RegisterUndoData(data);
				ms_bInnerCallUndo = true;
            }
        }
        //-----------------------------------------------------
        public static System.Object DrawProperty(System.Object data, List<string> IngoreFields, System.Action<string> OnSubDraw = null, OnFieldDirtyDelegate OnChangeField = null, System.Object pParentData = null, System.Reflection.FieldInfo parentFieldInfo = null)
        {
            return DrawProperty(data, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, IngoreFields,OnSubDraw, OnChangeField, pParentData, parentFieldInfo);
        }
        //------------------------------------------------------
        public static System.Object DrawPropertyByFieldName(System.Object data, string strField, System.Action<string> OnSubDraw = null, string displayName = null, OnFieldDirtyDelegate OnChangeField = null, System.Object pParentData = null, System.Reflection.FieldInfo parentFieldInfo = null)
        {
            return DrawPropertyField(data, data.GetType().GetField(strField), OnSubDraw, displayName, OnChangeField, pParentData,parentFieldInfo);
        }
        //------------------------------------------------------
        public static Action<System.Object, FieldInfo, bool> OnDrawField = null;
        //------------------------------------------------------
        public static System.Object DrawPropertyField(System.Object data, FieldInfo finfo, System.Action<string> OnSubDraw = null, string displayName = null, OnFieldDirtyDelegate OnChangeField = null, System.Object pParentData = null, System.Reflection.FieldInfo parentFieldInfo = null)
        {
			if (finfo == null)
				return data;


            bool isUnEdit = finfo.IsDefined(typeof(UnEditAttribute));
            using (new FieldScope(data,finfo, !isUnEdit, OnChangeField))
            {
	            if (finfo == null) return data;
	            if (finfo.IsDefined(typeof(Framework.DrawProps.DisableAttribute)))
	            {
	                if (OnSubDraw != null) OnSubDraw(finfo.Name);
	                return data;
	            }

	            if (finfo.IsDefined(typeof(HideInInspector)))
	            {
	                if (OnSubDraw != null) OnSubDraw(finfo.Name);
	                return data;
	            }

				if(finfo.IsDefined(typeof(FieldInspectorAttribute)))
				{
                    FieldInspectorAttribute varFiledDraw = finfo.GetCustomAttribute<FieldInspectorAttribute>();
                    data = varFiledDraw.OnInspector(data, finfo, pParentData, parentFieldInfo);
					return data;
                }

                RangeAttribute rangAttr = null;
	            if (finfo.IsDefined(typeof(RangeAttribute)))
	            {
	                rangAttr = finfo.GetCustomAttribute<RangeAttribute>();
	            }

                DrawProps.EditFloatAttribute editFloatAttr = null;
                if (finfo.IsDefined(typeof(DrawProps.EditFloatAttribute)))
                {
                    editFloatAttr = finfo.GetCustomAttribute<DrawProps.EditFloatAttribute>();
                }
	            if (finfo.IsDefined(typeof(DrawProps.StateByFieldAttribute)))
	            {
	                DrawProps.StateByFieldAttribute[] attrs = (DrawProps.StateByFieldAttribute[])finfo.GetCustomAttributes<DrawProps.StateByFieldAttribute>();
	                bool bHasDraw = false;
	                for(int i = 0; i < attrs.Length; ++i)
	                {
	                    FieldInfo byField = data.GetType().GetField(attrs[i].fieldName, BindingFlags.Public | BindingFlags.Instance);
	                    if (byField != null)
	                    {
	                        var valObj = byField.GetValue(data);
	                        string val = valObj?.ToString().ToLower();
                            if (byField.FieldType == typeof(string))
                            {
                                if (string.IsNullOrEmpty(val))
                                {
                                    val = "null";
                                }
                            }
                            if (attrs[i].IsContain)
	                        {
	                            if (attrs[i].fieldValue.Contains(val))
	                            {
	                                bHasDraw = true;
	                                break;
	                            }
	                        }
	                        else
	                        {
	                            if (!attrs[i].fieldValue.Contains(val))
	                            {
	                                bHasDraw = true;
	                                break;
	                            }
	                        }
	                    }
	                }
	                if(!bHasDraw)
	                {
	                    if (OnSubDraw != null) OnSubDraw(finfo.Name);
	                    return data;
	                }
	            }

	            if (finfo.IsDefined(typeof(DrawProps.StateByTypeAttribute)))
	            {
	                DrawProps.StateByTypeAttribute typeSets = finfo.GetCustomAttribute<DrawProps.StateByTypeAttribute>();
	                if (!typeSets.typeSets.Contains(data.GetType()))
	                {
	                    return data;
	                }
	            }

	            if (finfo.IsDefined(typeof(DrawProps.StateByBitAttribute)))
	            {
	                DrawProps.StateByBitAttribute[] attrs = (DrawProps.StateByBitAttribute[])finfo.GetCustomAttributes<DrawProps.StateByBitAttribute>();
	                for (int i = 0; i < attrs.Length; ++i)
	                {
	                    FieldInfo byField = data.GetType().GetField(attrs[i].fieldName, BindingFlags.Public | BindingFlags.Instance);
	                    if (byField != null)
	                    {
	                        string val = byField.GetValue(data).ToString();
	                        int flag = 0;
	                        if (int.TryParse(val, out flag))
	                        {
	                            bool bInlude = false;
	                            for(int j =0; j < attrs[i].fieldValue.Count; ++j)
	                            {
	                               if( (attrs[i].fieldValue[j]&flag) !=0 )
	                                {
	                                    bInlude = true;
	                                    break;
	                                }
	                            }

	                            if (!bInlude)
	                            {
	                                if (OnSubDraw != null) OnSubDraw(finfo.Name);
	                                return data;
	                            }
	                        }
	                    }
	                }
	            }
				string strTips = "";
	            string strDisplayName = finfo.Name;
	            if (finfo.IsDefined(typeof(Framework.DrawProps.DisplayAttribute)))
	            {
					var displayAttr = finfo.GetCustomAttribute<Framework.DrawProps.DisplayAttribute>();
                    strDisplayName = displayAttr.displayName;
					strTips = displayAttr.strTips;
                }
                if (finfo.IsDefined(typeof(InspectorNameAttribute)))
                {
                    strDisplayName = finfo.GetCustomAttribute<InspectorNameAttribute>().displayName;
                }

                if (finfo.IsDefined(typeof(DrawProps.DisplayNameByFieldAttribute)))
	            {
	                DrawProps.DisplayNameByFieldAttribute[] attr = (DrawProps.DisplayNameByFieldAttribute[])finfo.GetCustomAttributes<DrawProps.DisplayNameByFieldAttribute>();
	                for(int i = 0; i < attr.Length; ++i)
	                {
	                    FieldInfo byField = data.GetType().GetField(attr[i].fieldName, BindingFlags.Public | BindingFlags.Instance);
	                    if (byField != null)
	                    {
							var varObj = byField.GetValue(data);

                            string val = varObj!=null?varObj.ToString().ToLower():"null";
							if (byField.FieldType == typeof(string) && string.IsNullOrEmpty(val))
								val = "null";
	                        if (!string.IsNullOrEmpty(attr[i].strDisplayName) && attr[i].fieldValue.Contains(val))
	                        {
								strTips = attr[i].tips;
                                strDisplayName = attr[i].strDisplayName;
	                            break;
	                        }
	                    }
	                }
	            }

	            if (!string.IsNullOrEmpty(displayName))
	                strDisplayName = displayName;

                GUIContent displayNameContent = new GUIContent(strDisplayName, strTips);

                bool dislayRenderLayer = false;
	            if (finfo.IsDefined(typeof(DrawProps.DisplayRenderLayerAttribute)))
	            {
	                dislayRenderLayer = true;
	            }
	            if (finfo.IsDefined(typeof(DrawProps.DisplayRenderLayerByFieldAttribute)))
	            {
	                DrawProps.DisplayRenderLayerByFieldAttribute[] typeSets = (DrawProps.DisplayRenderLayerByFieldAttribute[])finfo.GetCustomAttributes<DrawProps.DisplayRenderLayerByFieldAttribute>();
	                for (int i = 0; i < typeSets.Length; ++i)
	                {
	                    FieldInfo byField = data.GetType().GetField(typeSets[i].fieldName, BindingFlags.Public | BindingFlags.Instance);
	                    if (byField != null)
	                    {
	                        string val = byField.GetValue(data).ToString().ToLower();
	                        if (val.CompareTo(typeSets[i].fieldValue) == 0)
	                        {
	                            dislayRenderLayer = true;
	                            break;
	                        }
	                    }
	                }
	            }
	            bool bEnumBitOffset = false;
	            System.Type byType = null;
	            bool bBit = false;
	            if (finfo.IsDefined(typeof(DrawProps.DisplayEnumBitAttribute)))
	            {
	                DrawProps.DisplayEnumBitAttribute attr = finfo.GetCustomAttribute<DrawProps.DisplayEnumBitAttribute>();
	                byType = attr.GetEnumType();
	                bEnumBitOffset = attr.bBitOffset;
	                bBit = true;
	            }
	            if (finfo.IsDefined(typeof(DrawProps.DisplayTypeByFieldAttribute)))
	            {
	                DrawProps.DisplayTypeByFieldAttribute[] typeSets = (DrawProps.DisplayTypeByFieldAttribute[])finfo.GetCustomAttributes<DrawProps.DisplayTypeByFieldAttribute>();
	                for (int i = 0; i < typeSets.Length; ++i)
	                {
	                    FieldInfo byField = data.GetType().GetField(typeSets[i].fieldName, BindingFlags.Public | BindingFlags.Instance);
	                    if (byField != null)
	                    {
	                        string val = byField.GetValue(data).ToString().ToLower();
	                        if (val.CompareTo(typeSets[i].fieldValue) == 0)
	                        {
	                            if(bBit && typeSets[i].bBit)
	                            {
	                                bEnumBitOffset = typeSets[i].bEnumBitOffset;
	                                byType = typeSets[i].GetDisType();
	                            }
	                            else
	                            {
	                                bBit = typeSets[i].bBit;
	                                bEnumBitOffset = typeSets[i].bEnumBitOffset;
	                                byType = typeSets[i].GetDisType();
	                            }
	                            break;
	                        }
	                    }
	                }
	            }
	            if (bBit && byType != null && byType.IsEnum)
	            {
	                int flag = 0;
	                int.TryParse(finfo.GetValue(data).ToString(), out flag);
	                {
	                    if (OnDrawField != null) OnDrawField(data, finfo, true);
	                    EditorGUILayout.LabelField(displayNameContent);
	                    EditorGUI.indentLevel++;
	                    foreach (Enum v in Enum.GetValues(byType))
	                    {
	                        string strName = Enum.GetName(byType, v);
	                        FieldInfo fi = byType.GetField(strName);
	                        if (fi.IsDefined(typeof(Framework.DrawProps.DisableAttribute)))
	                        {
	                            continue;
	                        }
	                        int flagValue = Convert.ToInt32(v);
	                        if (bEnumBitOffset) flagValue = 1 << flagValue;
							string tips = "";
	                        if (fi.IsDefined(typeof(Framework.DrawProps.DisplayAttribute)))
	                        {
								var dispAttr = fi.GetCustomAttribute<Framework.DrawProps.DisplayAttribute>();

                                strName = dispAttr.displayName;
                                tips = dispAttr.strTips;
                            }
	                        if (fi.IsDefined(typeof(DrawProps.StateByTypeAttribute)))
	                        {
	                            DrawProps.StateByTypeAttribute typeSets = fi.GetCustomAttribute<DrawProps.StateByTypeAttribute>();
	                            if (!typeSets.typeSets.Contains(data.GetType()))
	                            {
	                                continue;
	                            }
	                        }

	                        bool bToggle = EditorGUILayout.Toggle(new GUIContent(strName, tips), (flag & flagValue) != 0);
	                        if (bToggle)
	                        {
	                            flag |= (int)flagValue;
	                        }
	                        else flag &= (int)(~flagValue);
	                    }

	                    if (finfo.FieldType == typeof(byte)) finfo.SetValue(data, (byte)flag);
	                    else if (finfo.FieldType == typeof(short)) finfo.SetValue(data, (short)flag);
	                    else if (finfo.FieldType == typeof(ushort)) finfo.SetValue(data, (ushort)flag);
	                    else if (finfo.FieldType == typeof(int)) finfo.SetValue(data, (int)flag);
	                    else if (finfo.FieldType == typeof(uint)) finfo.SetValue(data, (uint)flag);
	                    else if (finfo.FieldType == typeof(long)) finfo.SetValue(data, (long)flag);
	                    else if (finfo.FieldType == typeof(string)) finfo.SetValue(data, flag.ToString());
	                    EditorGUI.indentLevel--;

	                    if (OnSubDraw != null) OnSubDraw(finfo.Name);
	                    if (OnDrawField != null) OnDrawField(data, finfo, false);
	                    return data;
	                }
	            }

	            string strField = "";
	            System.Type tableType = null;
	            if (finfo.IsDefined(typeof(DrawProps.FormSelectFieldAttribute)))
	            {
	                DrawProps.FormSelectFieldAttribute attri = finfo.GetCustomAttribute<DrawProps.FormSelectFieldAttribute>();
	                if (!string.IsNullOrEmpty(attri.fieldName))
	                {
	                    FieldInfo byField = data.GetType().GetField(attri.fieldName, BindingFlags.Public | BindingFlags.Instance);
	                    string strSelectValue = "";
	                    if(byField!=null)
	                    {
	                        strSelectValue = byField.GetValue(data).ToString();
	                        DrawProps.FormViewBinderAttribute[] form_attri = (DrawProps.FormViewBinderAttribute[])finfo.GetCustomAttributes<DrawProps.FormViewBinderAttribute>();
	                        if(form_attri!=null && form_attri.Length>0)
	                        {
	                            for(int j =0; j < form_attri.Length; ++j)
	                            {
	                                if (form_attri[j].GetTableType() != null && !string.IsNullOrEmpty(form_attri[j].Field) && form_attri[j].KeyValue.CompareTo(strSelectValue) ==0)
	                                {
	                                    strField = form_attri[j].Field;
	                                    tableType = form_attri[j].GetTableType();
	                                    break;
	                                }
	                            }
	                        }
	                    }
	                }
	            }
	            else if (finfo.IsDefined(typeof(DrawProps.FormViewBinderAttribute)))
	            {
	                DrawProps.FormViewBinderAttribute[] attri = (DrawProps.FormViewBinderAttribute[])finfo.GetCustomAttributes<DrawProps.FormViewBinderAttribute>();
	                if(attri.Length>0)
	                {
	                    if (attri[0].GetTableType() != null && !string.IsNullOrEmpty(attri[0].Field))
	                    {
	                        strField = attri[0].Field;
	                        tableType = attri[0].GetTableType();
	                    }
	                }

	            }

	            bool bOnlyDisplayEnumGUI = false;
	            if (finfo.IsDefined(typeof(DrawProps.DisplayEnumAttribute)))
	            {
	                bOnlyDisplayEnumGUI = true;
	                DrawProps.DisplayEnumAttribute attr = finfo.GetCustomAttribute<DrawProps.DisplayEnumAttribute>();
	                if (!string.IsNullOrEmpty(attr.strField) && attr.groups != null)
	                {
	                    bOnlyDisplayEnumGUI = false;
	                    FieldInfo byField = data.GetType().GetField(attr.strField, BindingFlags.Public | BindingFlags.Instance);
	                    if (byField != null)
	                    {
	                        string val = byField.GetValue(data).ToString().ToLower();
	                        for (int i = 0; i < attr.groups.Length; ++i)
	                        {
	                            if (attr.groups[i].ToLower().CompareTo(val) == 0)
	                            {
	                                bOnlyDisplayEnumGUI = true;
	                                byType = attr.GetEnumType();
	                                break;
	                            }
	                        }
	                    }
	                }
	                else
	                {
	                    bOnlyDisplayEnumGUI = true;
	                    byType = attr.GetEnumType();
	                }
	            }

				DrawProps.DisplayDrawTypeAttribute drawTypeAttr = null;

                bool bHorizontal = false;
	            if (finfo.IsDefined(typeof(DrawProps.DisplayDrawTypeAttribute)))
	            {
	                DrawProps.DisplayDrawTypeAttribute typeSets = finfo.GetCustomAttribute<DrawProps.DisplayDrawTypeAttribute>();
					drawTypeAttr = typeSets;

                    if (typeSets.GetDisplayType() != null)
	                {
	                    byType = typeSets.GetDisplayType();
	                    bEnumBitOffset = typeSets.bEnumBitOffset;
	                }
	            }

				DrawProps.RowFieldInspectorAttribute drawLineRow = null;
                if (finfo.IsDefined(typeof(DrawProps.RowFieldInspectorAttribute)))
                {
                    drawLineRow = finfo.GetCustomAttribute<DrawProps.RowFieldInspectorAttribute>();
                }

                if (tableType != null || (byType != null && byType.IsEnum) || finfo.IsDefined(typeof(DrawProps.DefaultValueAttribute)) || OnSubDraw != null || drawLineRow!=null)
	                bHorizontal = true;

	            if (finfo.IsDefined(typeof(DrawProps.DisplayDrawTypeAttribute)))
	                bHorizontal = false;

                if (finfo.FieldType.IsArray ||(finfo.FieldType.IsGenericType && finfo.FieldType.GenericTypeArguments != null && finfo.FieldType.GenericTypeArguments.Length == 1))
                {
					bHorizontal = false;
                }

	            if (bHorizontal)
	            {
	                EditorGUILayout.BeginHorizontal();
	            }

	                FieldInfo objFilePathField = null;
	                DrawProps.ObjPathFieldAttribute pathFiledAttr = finfo.GetCustomAttribute<DrawProps.ObjPathFieldAttribute>();
	                if (pathFiledAttr != null)
	                {
	                    objFilePathField = data.GetType().GetField(pathFiledAttr.fieldName, BindingFlags.Public | BindingFlags.Instance);
	                    if (objFilePathField == null)
	                        objFilePathField = data.GetType().GetField(pathFiledAttr.fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
	                    if (objFilePathField != null && objFilePathField.FieldType != typeof(string))
	                        objFilePathField = null;
	                }
	            DrawProps.BitViewAttribute[] bitViewAttrs = (DrawProps.BitViewAttribute[])finfo.GetCustomAttributes<DrawProps.BitViewAttribute>();

	            if (OnDrawField != null) OnDrawField(data, finfo, true);
                //     if(!bOnlyDisplayEnumGUI)
                if (drawTypeAttr ==null || !drawTypeAttr.CallUserMethod(ref data, finfo))
                {
	                if (finfo.FieldType == typeof(string))
	                {
	                    object fieldObj = finfo.GetValue(data);
	                    string strValue = fieldObj!=null? fieldObj.ToString():"";

	                    if (finfo.IsDefined(typeof(DrawProps.BindSlotAttribute)) || (byType != null && byType == typeof(DrawProps.BindSlotAttribute)))
	                    {
	                        if (InspectorDrawUtil.BindSlots == null) InspectorDrawUtil.BindSlots = new List<string>();
	                        if (!BindSlots.Contains("Root")) BindSlots.Insert(0, "Root");
	                        if (!BindSlots.Contains("RootTop")) BindSlots.Insert(0, "RootTop");
	                        if (!BindSlots.Contains("CameraRoot")) BindSlots.Insert(0, "CameraRoot");
	                        if (!BindSlots.Contains("SceneRoots")) BindSlots.Insert(0, "SceneRoots");
	                        if (!BindSlots.Contains("Particles")) BindSlots.Insert(0, "Particles");
	                        if (!BindSlots.Contains("ActorSystems")) BindSlots.Insert(0, "ActorSystems");
	                        if (!BindSlots.Contains("Players")) BindSlots.Insert(0, "Players");
	                        if (!BindSlots.Contains("Monsters")) BindSlots.Insert(0, "Monsters");
	                        if (!BindSlots.Contains("Elements")) BindSlots.Insert(0, "Elements");
	                        if (!BindSlots.Contains("Summons")) BindSlots.Insert(0, "Summons");
	                        if (!BindSlots.Contains("CenterRode")) BindSlots.Insert(0, "CenterRode");
	                        if (!BindSlots.Contains("None")) BindSlots.Insert(0, "None");

	                        EditorGUILayout.BeginHorizontal();
	                        //! default slot
	                        string strSlot = strValue;
	                        strSlot = EditorGUILayout.TextField(displayNameContent, strSlot);

	                        int index = -1;
	                        if (!string.IsNullOrEmpty(strSlot))
	                            index = BindSlots.IndexOf(strSlot);
	                        else
	                            index = BindSlots.IndexOf("None");
	                        index = EditorGUILayout.Popup(index, BindSlots.ToArray());
	                        if (index >= 0 && index < BindSlots.Count) strSlot = BindSlots[index];
	                        //      else strSlot = "";
	                        if (strSlot.CompareTo("None") == 0) strSlot = "";
	                        finfo.SetValue(data, strSlot);
	                        EditorGUILayout.EndHorizontal();
	                    }
	                    else if(finfo.IsDefined(typeof(DrawProps.StringViewPluginAttribute)))
	                    {
	                        DrawProps.StringViewPluginAttribute attri = (DrawProps.StringViewPluginAttribute)finfo.GetCustomAttribute<DrawProps.StringViewPluginAttribute>();
	                        if(!DrawStringUserPlugin(data, finfo, displayNameContent, ref strValue, attri.userPlugin))
	                        {
	                            strValue = EditorGUILayout.TextField(displayNameContent, strValue);
	                        }
	                        finfo.SetValue(data, strValue);
	                    }
	                    else if (finfo.IsDefined(typeof(DrawProps.StateStringViewGUIAttribute)))
	                    {
	                        DrawProps.StateStringViewGUIAttribute[] attri = (DrawProps.StateStringViewGUIAttribute[])finfo.GetCustomAttributes<DrawProps.StateStringViewGUIAttribute>();

	                        System.Type bindType = null;
	                        for(int i = 0; i < attri.Length; ++i)
	                        {
	                            if (string.IsNullOrEmpty(attri[i].strField)) continue;
	                            FieldInfo byField = data.GetType().GetField(attri[i].strField, BindingFlags.Public | BindingFlags.Instance);
	                            if (byField != null)
	                            {
	                                string val = byField.GetValue(data).ToString().ToLower();
	                                if (attri[i].strValue.ToLower().CompareTo(val) == 0)
	                                {
	                                    bOnlyDisplayEnumGUI = true;
	                                    bindType = attri[i].GetBindType();
	                                    break;
	                                }
	                            }
	                        }

	                        string strPath = strValue;
	                        if(bindType!=null)
	                        {
	                            if (!DrawStringUserPlugin(data, finfo, displayNameContent, ref strPath, bindType))
	                            {
	                                GUILayout.BeginHorizontal();
	                                UnityEngine.Object pAsset = EditorGUILayout.ObjectField(displayNameContent, AssetDatabase.LoadAssetAtPath(strPath, bindType), bindType, false);
	                                if (pAsset != null)
	                                    strPath = AssetDatabase.GetAssetPath(pAsset);
	                                else
	                                    strPath = "";
	                                if (!string.IsNullOrEmpty(strPath) && !strPath.Contains("Assets/Datas/"))
	                                    EditorGUILayout.HelpBox("必须是Assets/Datas/下的资源", MessageType.Error);
	                                GUILayout.EndHorizontal();
	                            }
	                        }
	                        else
	                        {
	                            strPath = EditorGUILayout.TextField(displayNameContent, strPath);
	                        }
	                        finfo.SetValue(data, strPath);
	                    }
	                    else if (finfo.IsDefined(typeof(DrawProps.StringViewAttribute)))
	                    {
	                        DrawProps.StringViewAttribute[] attris = (DrawProps.StringViewAttribute[])finfo.GetCustomAttributes<DrawProps.StringViewAttribute>();
	                        ms_TempOrderAttrs.Clear();
	                        for (int x = 0; x < attris.Length; ++x)
	                            ms_TempOrderAttrs.Add(new TempOrderAttr() { order = attris[x].order, attr = attris[x] });
	                        ms_TempOrderAttrs.Sort((TempOrderAttr a0, TempOrderAttr a1) => { return a0.order - a1.order; });
	                        bool bDraw = false;
	                        for(int x =0; x < ms_TempOrderAttrs.Count; ++x)
	                        {
	                            DrawProps.StringViewAttribute attri = ms_TempOrderAttrs[x].attr as DrawProps.StringViewAttribute;
	                            if (attri.GetBindType() != null)
	                            {
	                                if (!DrawStringUserPlugin(data, finfo, displayNameContent, ref strValue, attri.GetBindType()))
	                                {
	                                    string strPath = strValue;
	                                    GUILayout.BeginHorizontal();
	                                    UnityEngine.Object pAsset = EditorGUILayout.ObjectField(displayNameContent, AssetDatabase.LoadAssetAtPath(strPath, attri.GetBindType()), attri.GetBindType(), false);
	                                    if (pAsset != null)
	                                        strPath = AssetDatabase.GetAssetPath(pAsset);
	                                    else
	                                        strPath = "";
	                                    if (!string.IsNullOrEmpty(strPath) && !strPath.Contains("Assets/Datas/"))
	                                        EditorGUILayout.HelpBox("必须是Assets/Datas/下的资源", MessageType.Error);
	                                    GUILayout.EndHorizontal();
	                                    finfo.SetValue(data, strPath);
	                                }
	                                else
	                                {
	                                    finfo.SetValue(data, strValue);
	                                }
	                                bDraw = true;
	                                break;
	                            }
	                        }
	                        if(!bDraw)
	                        {
	                            finfo.SetValue(data, EditorGUILayout.TextField(displayNameContent, strValue));
	                        }
	                    }
	                    else if (finfo.IsDefined(typeof(DrawProps.SelectFileAttribute)))
	                    {
	                        GUILayout.BeginHorizontal();
	                        string strPath = strValue;
	                        strPath = EditorGUILayout.TextField(displayNameContent, strPath);
	                        if (GUILayout.Button("...", new GUILayoutOption[] { GUILayout.Width(30) }))
	                        {
	                            DrawProps.SelectFileAttribute attri = finfo.GetCustomAttribute<DrawProps.SelectFileAttribute>();
	                            string strRoot = attri.root;
	                            if (string.IsNullOrEmpty(attri.root))
	                                strRoot = Application.dataPath;
	                            else
	                            {
	                                strRoot = Application.dataPath + attri.root;
	                            }
	                            string file = EditorUtility.OpenFilePanel("select file", strRoot, attri.extend).Replace("\\", "/");
	                            if(!string.IsNullOrEmpty(file))
	                            {
	                                if (file.Contains(strRoot))
	                                {
	                                    string sub = file;
	                                    if (!attri.includeExtend)
	                                    {
	                                        sub = System.IO.Path.GetDirectoryName(file) + "/" + System.IO.Path.GetFileNameWithoutExtension(file);
	                                    }
	                                    sub = sub.Replace(strRoot, attri.subRoot);

	                                    if (!string.IsNullOrEmpty(sub))
	                                    {
	                                        strPath = sub;
	                                    }
	                                    else
	                                    {
	                                        EditorUtility.DisplayDialog("提示", "无效路径:" + file, "好的");
	                                    }
	                                }
	                                else
	                                {
	                                    EditorUtility.DisplayDialog("提示", "必须选择目录[" + strRoot + "]下的文件", "好的");
	                                }
	                            }
   
	                        }
	                        finfo.SetValue(data, strPath);
	                        GUILayout.EndHorizontal();
	                    }
	                    else if (finfo.IsDefined(typeof(DrawProps.SelectDirAttribute)))
	                    {
	                        GUILayout.BeginHorizontal();
	                        string strPath = strValue;
	                        strPath = EditorGUILayout.TextField(displayNameContent, strPath);
	                        if (GUILayout.Button("...", new GUILayoutOption[] { GUILayout.Width(30) }))
	                        {
	                            DrawProps.SelectDirAttribute attri = finfo.GetCustomAttribute<DrawProps.SelectDirAttribute>();
	                            string strRoot = attri.root;
	                            if (string.IsNullOrEmpty(attri.root))
	                                strRoot = Application.dataPath;
	                            else
	                            {
	                                strRoot = Application.dataPath + attri.root;
	                            }
	                            string file = EditorUtility.OpenFolderPanel(displayNameContent.text, strRoot,"").Replace("\\", "/");
	                            strPath = file.Replace("\\", "/").Replace(Application.dataPath, "Assets");
	                        }
	                        finfo.SetValue(data, strPath);
	                        GUILayout.EndHorizontal();
	                    }
	                    else if (byType != null)
	                    {
	                        string strSrc = strValue;
	                        if (byType == typeof(bool))
	                        {
	                            byte temp;
	                            byte.TryParse(strSrc, out temp);
	                            strSrc = ((byte)(EditorGUILayout.Toggle(displayNameContent, temp != 0) ? 1 : 0)).ToString();
	                        }
	                        else if (byType == typeof(byte))
	                        {
	                            byte temp;
	                            byte.TryParse(strSrc, out temp);
	                            strSrc = ((byte)EditorGUILayout.IntField(displayNameContent, temp)).ToString();
	                        }
	                        else if (byType == typeof(short))
	                        {
	                            short temp;
	                            short.TryParse(strSrc, out temp);
	                            strSrc = ((short)EditorGUILayout.IntField(displayNameContent, temp)).ToString();
	                        }
	                        else if (byType == typeof(ushort))
	                        {
	                            ushort temp;
	                            ushort.TryParse(strSrc, out temp);
	                            strSrc = ((ushort)EditorGUILayout.IntField(displayNameContent, temp)).ToString();
	                        }
	                        else if (byType == typeof(int))
	                        {
	                            int temp;
	                            int.TryParse(strSrc, out temp);
	                            strSrc = EditorGUILayout.IntField(displayNameContent, temp).ToString();
	                        }
	                        else if (byType == typeof(uint))
	                        {
	                            uint temp;
	                            uint.TryParse(strSrc, out temp);
	                            strSrc = ((uint)EditorGUILayout.IntField(displayNameContent, (int)temp)).ToString();
	                        }
	                        else if (byType == typeof(float))
	                        {
	                            float temp;
	                            float.TryParse(strSrc, out temp);
	                            strSrc = EditorGUILayout.FloatField(displayNameContent, temp).ToString();
	                        }
	                        else if (byType == typeof(Color))
	                        {
	                            Color color;
	                            if (!ColorUtility.TryParseHtmlString("#"+strSrc, out color))
	                                color = Color.white;
	                            strSrc = ColorUtility.ToHtmlStringRGBA(EditorGUILayout.ColorField(displayNameContent, color, true, true, true));
	                        }
	                        else
	                            strSrc = EditorGUILayout.TextField(displayNameContent, strValue);

	                        finfo.SetValue(data, strSrc);
	                    }
	                    else
	                    {
	                        finfo.SetValue(data, EditorGUILayout.DelayedTextField(displayNameContent, strValue));
	                    }
	                }
	                else if (finfo.FieldType == typeof(int))
	                {
	                    if (byType != null && byType == typeof(bool))
	                    {
	                        finfo.SetValue(data, ((int)(EditorGUILayout.Toggle(displayNameContent, (int)finfo.GetValue(data) != 0) ? 1 : 0)));
	                    }
                        //else if (byType != null && byType == typeof(BinderUnityObject))
                        //{
                        //    finfo.SetValue(data, ((int)(ObjectBinderUtils.DrawBinderInspector((int)finfo.GetValue(data), displayName))));
                        //}
                        else if (dislayRenderLayer)
	                    {
	                        finfo.SetValue(data, PopRenderLayer(displayNameContent, (int)finfo.GetValue(data)));
	                    }
	                    else if(bitViewAttrs!=null && bitViewAttrs.Length>0)
	                    {
	                        int val = (int)finfo.GetValue(data);
	                        val = DrawBitView(displayNameContent, val, bitViewAttrs);
	                        finfo.SetValue(data, val);
	                    }
	                    else
	                    {
	                        int val = (int)finfo.GetValue(data);

	                            if(editFloatAttr!=null)
	                            {
	                                float valF = (float)val / (float)editFloatAttr.factor;
	                                if (rangAttr != null) valF = EditorGUILayout.Slider(displayNameContent, valF, rangAttr.min, rangAttr.max);
	                                else valF = EditorGUILayout.FloatField(displayNameContent, valF);

	                                val = (int)(valF * editFloatAttr.factor);
	                            }
	                            else
	                            {
	                                if (rangAttr != null) val = EditorGUILayout.IntSlider(displayNameContent, val, (int)rangAttr.min, (int)rangAttr.max);
	                                else val = EditorGUILayout.IntField(displayNameContent, val);
	                            }

	                            finfo.SetValue(data, val);
	                        }
	                    }
	                    else if (finfo.FieldType == typeof(uint))
	                    {
	                        if (byType != null && byType == typeof(bool))
	                        {
	                            finfo.SetValue(data, ((uint)(EditorGUILayout.Toggle(displayNameContent, (uint)finfo.GetValue(data) != 0) ? 1 : 0)));
	                        }
							//else if (byType != null && byType == typeof(BinderUnityObject))
							//{
							//	finfo.SetValue(data, ((int)(ObjectBinderUtils.DrawBinderInspector((int)finfo.GetValue(data), displayName))));
							//}
							else if (dislayRenderLayer)
	                        {
                            //  finfo.SetValue(data, (uint)PopRenderLayer(displayNameContent, (int)(uint)finfo.GetValue(data)));
                        }
                        else if (bitViewAttrs != null && bitViewAttrs.Length > 0)
	                        {
	                            uint val = (uint)finfo.GetValue(data);
	                            val = (uint)DrawBitView(displayNameContent, (int)val, bitViewAttrs);
	                            finfo.SetValue(data, val);
	                        }
	                        else
	                        {
	                            uint val1 = (uint)finfo.GetValue(data);
	                            int val = (int)val1;

	                            if (editFloatAttr != null)
	                            {
	                                float valF = (float)val / (float)editFloatAttr.factor;
	                                if (rangAttr != null) valF = EditorGUILayout.Slider(displayNameContent, valF, rangAttr.min, rangAttr.max);
	                                else valF = EditorGUILayout.FloatField(displayNameContent, valF);

	                                val = (int)Mathf.Max(0, (int)(valF * editFloatAttr.factor));
	                            }
	                            else
	                            {
	                                if (rangAttr != null) val = EditorGUILayout.IntSlider(displayNameContent, val, Mathf.Max(0, (int)rangAttr.min), (int)rangAttr.max);
	                                else val = EditorGUILayout.IntField(displayNameContent, val);
	                            }
	                            finfo.SetValue(data, (uint)val);
	                        }
	                    }
	                    else if (finfo.FieldType == typeof(short))
	                    {
	                        if (byType != null && byType == typeof(bool))
	                        {
	                            finfo.SetValue(data, ((short)(EditorGUILayout.Toggle(displayNameContent, (short)finfo.GetValue(data) != 0) ? 1 : 0)));
	                        }
	                        else if (dislayRenderLayer)
	                        {
                            //finfo.SetValue(data, (short)PopRenderLayer(displayNameContent, (int)(short)finfo.GetValue(data)));
                        }
                        else if (bitViewAttrs != null && bitViewAttrs.Length > 0)
	                        {
	                            short val = (short)finfo.GetValue(data);
	                            val = (short)DrawBitView(displayNameContent, (int)val, bitViewAttrs);
	                            finfo.SetValue(data, val);
	                        }
	                        else
	                        {
	                            short val1 = (short)finfo.GetValue(data);
	                            int val = (int)val1;
	                            if (editFloatAttr != null)
	                            {
	                                float valF = (float)val / (float)editFloatAttr.factor;
	                                if (rangAttr != null) valF = EditorGUILayout.Slider(displayNameContent, valF, rangAttr.min, rangAttr.max);
	                                else valF = EditorGUILayout.FloatField(displayNameContent, valF);

	                                val = (int)(valF * editFloatAttr.factor);
	                            }
	                            else
	                            {
	                                if (rangAttr != null) val = EditorGUILayout.IntSlider(displayNameContent, val, (short)rangAttr.min, (short)rangAttr.max);
	                                else val = EditorGUILayout.IntField(displayNameContent, val);
	                            }

	                        finfo.SetValue(data, (short)val);
	                    }

	                }
	                else if (finfo.FieldType == typeof(ushort))
	                {
	                    if (byType != null && byType == typeof(bool))
	                    {
	                        finfo.SetValue(data, ((ushort)(EditorGUILayout.Toggle(displayNameContent, (ushort)finfo.GetValue(data) != 0) ? 1 : 0)));
	                    }
	                    else if (dislayRenderLayer)
	                    {
	                        finfo.SetValue(data, (ushort)PopRenderLayer(displayNameContent, (int)(ushort)finfo.GetValue(data)));
	                    }
	                    else if (bitViewAttrs != null && bitViewAttrs.Length > 0)
	                    {
	                        ushort val = (ushort)finfo.GetValue(data);
	                        val = (ushort)DrawBitView(displayNameContent, (int)val, bitViewAttrs);
	                        finfo.SetValue(data, val);
	                    }
	                    else
	                    {
                            ushort val1 = (ushort)finfo.GetValue(data);
                            int val = (int)val1;
                            if (editFloatAttr != null)
                            {
                                float valF = (float)val / (float)editFloatAttr.factor;
                                if (rangAttr != null) valF = EditorGUILayout.Slider(displayNameContent, valF, rangAttr.min, rangAttr.max);
                                else valF = EditorGUILayout.FloatField(displayNameContent, valF);

                                val = Mathf.Max(0, (int)(valF * editFloatAttr.factor));
                            }
                            else
                            {
                                if (rangAttr != null) val = EditorGUILayout.IntSlider(displayNameContent, val, Mathf.Max(0, (ushort)rangAttr.min), (ushort)rangAttr.max);
                                else val = EditorGUILayout.IntField(displayNameContent, val);
                            }

                            finfo.SetValue(data, (ushort)val);
	                    }
  
	                }
	                else if (finfo.FieldType == typeof(float))
	                {
	                    float val = (float)finfo.GetValue(data);
	                    if (rangAttr != null) val = EditorGUILayout.Slider(displayNameContent, (float)val, (float)rangAttr.min, (float)rangAttr.max);
	                    else val = EditorGUILayout.FloatField(displayNameContent, val);
	                    finfo.SetValue(data, val);
	                }
	                else if (finfo.FieldType == typeof(double))
	                {
	                    double val = (double)finfo.GetValue(data);
	                    if (rangAttr != null)val = EditorGUILayout.Slider(displayNameContent, (float)val, (float)rangAttr.min, (float)rangAttr.max);
	                    else val = EditorGUILayout.DoubleField(displayNameContent, val);
	                    finfo.SetValue(data, val);
	                }
	                else if (finfo.FieldType == typeof(byte))
	                {
	                    if (byType != null && byType == typeof(bool))
	                    {
	                        finfo.SetValue(data, ((byte)(EditorGUILayout.Toggle(displayNameContent, (byte)finfo.GetValue(data) != 0) ? 1 : 0)));
	                    }
	                    else if (dislayRenderLayer)
	                    {
	                        finfo.SetValue(data, (byte)PopRenderLayer(displayNameContent, (int)(byte)finfo.GetValue(data)));
	                    }
	                    else if (bitViewAttrs != null && bitViewAttrs.Length > 0)
	                    {
	                        byte val = (byte)finfo.GetValue(data);
	                        val = (byte)DrawBitView(displayNameContent, (int)val, bitViewAttrs);
	                        finfo.SetValue(data, val);
	                    }
	                    else
	                    {
	                        byte val1 = (byte)finfo.GetValue(data);
	                        int val = (int)val1;
	                        if (rangAttr != null)
	                            val = EditorGUILayout.IntSlider(displayNameContent, val, (int)rangAttr.min, (int)rangAttr.max);
	                        else
	                            val = EditorGUILayout.IntField(displayNameContent, val);
	                        val = Mathf.Clamp(val, 0, 255);

	                        finfo.SetValue(data, (byte)val);
	                    }
	                }
	                else if (finfo.FieldType == typeof(Rect))
	                {
	                    Rect val = (Rect)finfo.GetValue(data);

	                    val = EditorGUILayout.RectField(displayNameContent, val);
	                    finfo.SetValue(data, val);
	                }
	#if !UNITY_5_1
	                else if (finfo.FieldType == typeof(RectInt))
	                {
	                    RectInt val = (RectInt)finfo.GetValue(data);

	                    val = EditorGUILayout.RectIntField(displayNameContent, val);
	                    finfo.SetValue(data, val);
	                }
	#endif
	                else if (finfo.FieldType == typeof(Vector4))
	                {
	                    Vector4 val = (Vector4)finfo.GetValue(data);
	                    if (byType != null && byType == typeof(float))
	                    {
	                        val.x = EditorGUILayout.FloatField(displayNameContent, val.x);
	                    }
	                    else if (byType != null && byType == typeof(Vector2))
	                    {
	                        Vector2 ret = EditorGUILayout.Vector2Field(displayNameContent, new Vector2(val.x, val.y));
	                        val.x = ret.x;
	                        val.y = ret.y;
	                    }
	                    else if (byType != null && byType == typeof(Vector3))
	                    {
	                        Vector3 ret = EditorGUILayout.Vector2Field(displayNameContent, new Vector3(val.x, val.y, val.z));
	                        val.x = ret.x;
	                        val.y = ret.y;
	                        val.z = ret.z;
	                    }
	                    else if (byType != null && byType == typeof(Color))
	                    {
	                        Color ret = EditorGUILayout.ColorField(displayNameContent, new Color(val.x, val.y, val.z, val.w));
	                        val.x = ret.r;
	                        val.y = ret.g;
	                        val.z = ret.b;
	                        val.w = ret.a;
	                    }
	                    else
	                    {
	                        val = EditorGUILayout.Vector4Field(displayNameContent, val);
	                    }
						if(rangAttr!=null)
						{
							val.x = Mathf.Clamp(val.x, rangAttr.min, rangAttr.max);
							val.y = Mathf.Clamp(val.y, rangAttr.min, rangAttr.max);
							val.z = Mathf.Clamp(val.z, rangAttr.min, rangAttr.max);
							val.w = Mathf.Clamp(val.w, rangAttr.min, rangAttr.max);
                        }
	                    finfo.SetValue(data, val);
	                }
	                else if (finfo.FieldType == typeof(Vector3))
	                {
	                    Vector3 val = (Vector3)finfo.GetValue(data);
	                    if (byType != null && byType == typeof(float))
	                    {
	                        val.x = EditorGUILayout.FloatField(displayNameContent, val.x);
	                    }
	                    else if (byType != null && byType == typeof(Vector2))
	                    {
	                        Vector2 ret = EditorGUILayout.Vector2Field(displayNameContent, new Vector2(val.x, val.y));
	                        val.x = ret.x;
	                        val.y = ret.y;
	                    }
	                    else if (byType != null && byType == typeof(Color))
	                    {
	                        Color ret = EditorGUILayout.ColorField(displayNameContent, new Color(val.x, val.y, val.z, 1));
	                        val.x = ret.r;
	                        val.y = ret.g;
	                        val.z = ret.b;
	                    }
	                    else
	                        val = EditorGUILayout.Vector3Field(displayNameContent, val);
                        if (rangAttr != null)
                        {
                            val.x = Mathf.Clamp(val.x, rangAttr.min, rangAttr.max);
                            val.y = Mathf.Clamp(val.y, rangAttr.min, rangAttr.max);
                            val.z = Mathf.Clamp(val.z, rangAttr.min, rangAttr.max);
                        }
                        finfo.SetValue(data, val);
	                }
	                else if (finfo.FieldType == typeof(Vector2))
	                {
	                    Vector2 val = (Vector2)finfo.GetValue(data);
	                    if (byType != null && byType == typeof(float))
	                    {
	                        val.x = EditorGUILayout.FloatField(displayNameContent, val.x);
	                    }
	                    else
	                        val = EditorGUILayout.Vector2Field(displayNameContent, val);
                        if (rangAttr != null)
                        {
                            val.x = Mathf.Clamp(val.x, rangAttr.min, rangAttr.max);
                            val.y = Mathf.Clamp(val.y, rangAttr.min, rangAttr.max);
                        }
                        finfo.SetValue(data, val);
	                }
	#if !UNITY_5_1
	                else if (finfo.FieldType == typeof(Vector3Int))
	                {
	                    Vector3Int val = (Vector3Int)finfo.GetValue(data);

	                    val = EditorGUILayout.Vector3IntField(displayNameContent, val);
                        if (rangAttr != null)
                        {
                            val.x = (int)Mathf.Clamp(val.x, rangAttr.min, rangAttr.max);
                            val.y = (int)Mathf.Clamp(val.y, rangAttr.min, rangAttr.max);
                            val.z = (int)Mathf.Clamp(val.z, rangAttr.min, rangAttr.max);
                        }
                        finfo.SetValue(data, val);
	                }
	                else if (finfo.FieldType == typeof(Vector2Int))
	                {
	                    Vector2Int val = (Vector2Int)finfo.GetValue(data);

	                    val = EditorGUILayout.Vector2IntField(displayNameContent, val);
                        if (rangAttr != null)
                        {
                            val.x = (int)Mathf.Clamp(val.x, rangAttr.min, rangAttr.max);
                            val.y = (int)Mathf.Clamp(val.y, rangAttr.min, rangAttr.max);
                        }
                        finfo.SetValue(data, val);
	                }
	#endif
	                else if (finfo.FieldType == typeof(Color))
	                {
	                    Color val = (Color)finfo.GetValue(data);

	                    val = EditorGUILayout.ColorField(new GUIContent(displayNameContent), val, true,true,true);
	                    finfo.SetValue(data, val);
	                }
	                else if (finfo.FieldType.IsArray)
	                {
	                    System.Type valType = finfo.FieldType.Assembly.GetType(finfo.FieldType.FullName.Replace("[]", ""));
	                    if (valType != null)
	                    {
	                        IEnumerable list = finfo.GetValue(data) as IEnumerable;
	                        object ret = DealGeneic(finfo, displayNameContent, list, finfo.FieldType, valType, tableType, strField);
	                        finfo.SetValue(data, (IEnumerable)ret);
	                    }
	                }
	                else if (finfo.FieldType.IsGenericType && finfo.FieldType.GenericTypeArguments != null && finfo.FieldType.GenericTypeArguments.Length == 1)
	                {
	                    IEnumerable list = finfo.GetValue(data) as IEnumerable;
	                    object ret = DealGeneic(finfo, displayNameContent, list, finfo.FieldType, finfo.FieldType.GenericTypeArguments[0], tableType, strField);
	                    finfo.SetValue(data, ret);
	                }
	                else if (finfo.FieldType.IsEnum)
	                {
	                    Enum val = (Enum)Enum.ToObject(finfo.FieldType, finfo.GetValue(data));
	                    val = PopEnum(displayNameContent, val, finfo.FieldType);
	                    finfo.SetValue(data, val);
	                }
	                else if (finfo.FieldType == typeof(bool))
	                {
	                    bool val = (bool)finfo.GetValue(data);
	                    finfo.SetValue(data, EditorGUILayout.Toggle(displayNameContent, val));
	                }
	                else if (finfo.FieldType == typeof(AnimationCurve))
	                {
	                    GUILayout.BeginHorizontal();
	                    AnimationCurve curve = (AnimationCurve)finfo.GetValue(data);
	                    if (curve == null) curve = new AnimationCurve();
	                    curve = EditorGUILayout.CurveField(displayNameContent, curve);
	                    if (GUILayout.Button("Clear", new GUILayoutOption[] { GUILayout.Width(50) }))
	                    {
	                        curve.keys = null;
	                    }
	                    finfo.SetValue(data, curve);
	                    GUILayout.EndHorizontal();
	                }
                    //else if (finfo.FieldType == typeof(Framework.Plugin.AT.AAgentTreeData))
                    //{
                    //    GUILayout.BeginHorizontal();
                    //    Framework.Plugin.AT.AAgentTreeData pAT = (Framework.Plugin.AT.AAgentTreeData)EditorGUILayout.ObjectField(displayNameContent, (Framework.Plugin.AT.AAgentTreeData)finfo.GetValue(data), finfo.FieldType);
                    //    finfo.SetValue(data, pAT);
                    //    if (GUILayout.Button("编辑"))
                    //    {
                    //        Framework.Plugin.AT.AgentTreeEditor.Editor(pAT, (UnityEngine.Object)data);
                    //    }
                    //    GUILayout.EndHorizontal();
                    //}
                    //else if (finfo.FieldType == typeof(Framework.Plugin.AT.AgentTreeCoreData))
                    //{
                    //    GUILayout.BeginHorizontal();
                    //    Framework.Plugin.AT.AgentTreeCoreData atData = (Framework.Plugin.AT.AgentTreeCoreData)finfo.GetValue(data);
                    //    atData.bEnable = EditorGUILayout.Toggle("是否启用", atData.bEnable);
                    //    finfo.SetValue(data, atData);
                    //    if (GUILayout.Button("编辑"))
                    //    {
                    //        Framework.Plugin.AT.AgentTreeEditor.Editor(atData, (UnityEngine.Object)data);
                    //    }
                    //    if (GUILayout.Button("复制"))
                    //    {
                    //        ms_pCopyData = null;
                    //        try
                    //        {
                    //            ms_pCopyData = JsonUtility.FromJson<Framework.Plugin.AT.AgentTreeCoreData>(JsonUtility.ToJson(atData));
                    //        }
                    //        catch
                    //        {

                    //        }
                    //    }
                    //    if (ms_pCopyData!=null && ms_pCopyData is Framework.Plugin.AT.AgentTreeCoreData && ms_pCopyData != atData && GUILayout.Button("黏贴"))
                    //    {
                    //        finfo.SetValue(data, (Framework.Plugin.AT.AgentTreeCoreData)ms_pCopyData);
                    //        ms_pCopyData = null;
                    //    }
                    //    if (GUILayout.Button("清理"))
                    //    {
                    //        if (EditorUtility.DisplayDialog("提示", "是否清理", "清理", "再想想"))
                    //            atData.Clear();
                    //    }
                    //    GUILayout.EndHorizontal();
                    //}
                    else if (finfo.FieldType == typeof(Texture2D))
	                    {
	                        var obj = (Texture2D)EditorGUILayout.ObjectField(displayNameContent, (Texture2D)finfo.GetValue(data), finfo.FieldType, false);
	                        finfo.SetValue(data, obj);
	                        if (objFilePathField != null)
	                            objFilePathField.SetValue(data, obj ? AssetDatabase.GetAssetPath(obj) : "");
	                    }
	                    else if (finfo.FieldType == typeof(Cubemap))
	                    {
	                        var obj = (Cubemap)EditorGUILayout.ObjectField(displayNameContent, (Cubemap)finfo.GetValue(data), finfo.FieldType, false);
	                        finfo.SetValue(data, obj);
	                        if (objFilePathField != null)
	                            objFilePathField.SetValue(data, obj ? AssetDatabase.GetAssetPath(obj) : "");
	                    }
	                    else if (finfo.FieldType == typeof(Texture3D))
	                    {
	                        var obj = (Texture3D)EditorGUILayout.ObjectField(displayNameContent, (Texture3D)finfo.GetValue(data), finfo.FieldType, false);
	                        finfo.SetValue(data, obj);
	                        if (objFilePathField != null)
	                            objFilePathField.SetValue(data, obj ? AssetDatabase.GetAssetPath(obj) : "");
	                    }
	                    else if (finfo.FieldType == typeof(Texture))
	                    {
	                        var obj = (Texture)EditorGUILayout.ObjectField(displayNameContent, (Texture)finfo.GetValue(data), finfo.FieldType, false);
	                        finfo.SetValue(data, obj);
	                        if (objFilePathField != null)
	                            objFilePathField.SetValue(data, obj ? AssetDatabase.GetAssetPath(obj) : "");
	                    }
	                    else if (finfo.FieldType == typeof(Material))
	                    {
	                        var obj = (Material)EditorGUILayout.ObjectField(displayNameContent, (Material)finfo.GetValue(data), finfo.FieldType, false);
	                        finfo.SetValue(data, obj);
	                        if (objFilePathField != null)
	                            objFilePathField.SetValue(data, obj ? AssetDatabase.GetAssetPath(obj) : "");
	                    }
	                    else if (finfo.FieldType == typeof(Shader))
	                    {
	                        var obj = (Shader)EditorGUILayout.ObjectField(displayNameContent, (Shader)finfo.GetValue(data), finfo.FieldType, false);
	                        finfo.SetValue(data, obj);
	                        if (objFilePathField != null)
	                            objFilePathField.SetValue(data, obj ? AssetDatabase.GetAssetPath(obj) : "");
	                    }
	                    else if (finfo.FieldType == typeof(Mesh))
	                    {
	                        var obj = (Mesh)EditorGUILayout.ObjectField(displayNameContent, (Mesh)finfo.GetValue(data), finfo.FieldType, false);
	                        finfo.SetValue(data, obj);
	                        if (objFilePathField != null)
	                            objFilePathField.SetValue(data, obj ? AssetDatabase.GetAssetPath(obj) : "");
	                    }
	                    else if (finfo.FieldType == typeof(MeshRenderer))
	                    {
	                        var obj = (MeshRenderer)EditorGUILayout.ObjectField(displayNameContent, (MeshRenderer)finfo.GetValue(data), finfo.FieldType, false);
	                        finfo.SetValue(data, obj);
	                        if (objFilePathField != null)
	                            objFilePathField.SetValue(data, obj ? AssetDatabase.GetAssetPath(obj) : "");
	                    }
	                    else if (finfo.FieldType == typeof(SkinnedMeshRenderer))
	                    {
	                        var obj = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(displayNameContent, (SkinnedMeshRenderer)finfo.GetValue(data), finfo.FieldType, false);
	                        finfo.SetValue(data, obj);
	                        if (objFilePathField != null)
	                            objFilePathField.SetValue(data, obj ? AssetDatabase.GetAssetPath(obj) : "");
	                    }
	                    else if (finfo.FieldType == typeof(GameObject))
	                    {
	                        var obj = (GameObject)EditorGUILayout.ObjectField(displayNameContent, (GameObject)finfo.GetValue(data), finfo.FieldType, false);
	                        finfo.SetValue(data, obj);
	                        if (objFilePathField != null)
	                            objFilePathField.SetValue(data, obj ? AssetDatabase.GetAssetPath(obj) : "");
	                    }
	                    else if (finfo.FieldType == typeof(AudioClip))
	                    {
	                        var obj = (AudioClip)EditorGUILayout.ObjectField(displayNameContent, (AudioClip)finfo.GetValue(data), finfo.FieldType, false);
	                        finfo.SetValue(data, obj);
	                        if (objFilePathField != null)
	                            objFilePathField.SetValue(data, obj ? AssetDatabase.GetAssetPath(obj) : "");
	                    }
	                    else if (finfo.FieldType == typeof(AnimationClip))
	                    {
	                        var obj = (AnimationClip)EditorGUILayout.ObjectField(displayNameContent, (AnimationClip)finfo.GetValue(data), finfo.FieldType, false);
	                        finfo.SetValue(data, obj);
	                        if (objFilePathField != null)
	                            objFilePathField.SetValue(data, obj ? AssetDatabase.GetAssetPath(obj) : "");
	                    }
	                    else if (IsScriptObject(finfo.FieldType))
	                    {
	                        var obj = (ScriptableObject)EditorGUILayout.ObjectField(displayNameContent, (ScriptableObject)finfo.GetValue(data), finfo.FieldType, false);
	                        finfo.SetValue(data, obj);
	                        if (objFilePathField != null)
	                            objFilePathField.SetValue(data, obj ? AssetDatabase.GetAssetPath(obj) : "");
	                    }
	                    else if (IsUnityObject(finfo.FieldType))
	                    {
	                        UnityEngine.Object objVal;
	                        objVal = EditorGUILayout.ObjectField(displayNameContent, (UnityEngine.Object)finfo.GetValue(data), finfo.FieldType, true);
	                        finfo.SetValue(data, objVal);
	                        if (objFilePathField != null)
	                            objFilePathField.SetValue(data, objVal ? AssetDatabase.GetAssetPath(objVal) : "");
	                    }
	                else 
	                {
						if (finfo.IsDefined(typeof(DisplayAttribute)))
						{
							EditorGUILayout.LabelField(displayNameContent);
						//	var rect = GUILayoutUtility.GetLastRect();
						//	UIDrawUtils.DrawColorLine(new Vector2(rect.x, rect.y), new Vector2(rect.xMax, rect.y), Color.white);
						}
	                    System.Object objValue = finfo.GetValue(data);
	                    MethodInfo method = finfo.FieldType.GetMethod("OnInspector");
	                    if(method!=null && objValue!=null)
	                    {
	                        method.Invoke(objValue, new System.Object[] { "" });
	                        finfo.SetValue(data, objValue);
	                    }
						else
						{
							objValue = DrawProperty(objValue, null, OnSubDraw, OnChangeField, data, finfo);
                            finfo.SetValue(data, objValue);
                        }
                    }
	            }

	            if (bOnlyDisplayEnumGUI && byType!= null && byType.IsEnum)
	            {
	                string obj = finfo.GetValue(data).ToString();
	                int enumInt = 0;
	                if (obj != null) int.TryParse(obj, out enumInt);
	                Enum val = (Enum)Enum.ToObject(byType, enumInt);

	                EditorEnumPop.EnumPops.Clear();
	                EditorEnumPop.EnumValuePops.Clear();
	                int index = -1;
	                foreach (Enum v in Enum.GetValues(byType))
	                {
	                    FieldInfo fi = byType.GetField(v.ToString());
	                    string strTemName = v.ToString();
	                    if (fi != null && fi.IsDefined(typeof(DrawProps.DisableAttribute)))
	                    {
	                        continue;
	                    }
	                    if (fi != null && fi.IsDefined(typeof(DrawProps.DisplayNameAttribute)))
	                    {
	                        strTemName = fi.GetCustomAttribute<DrawProps.DisplayNameAttribute>().displayName;
	                    }
	                    EditorEnumPop.EnumPops.Add(strTemName);
	                    EditorEnumPop.EnumValuePops.Add(v);
	                    if (v.ToString().CompareTo(val.ToString()) == 0)
	                        index = EditorEnumPop.EnumPops.Count - 1;
	                }

	                index = EditorGUILayout.Popup(index, EditorEnumPop.EnumPops.ToArray());
	                if (index >= 0 && index < EditorEnumPop.EnumValuePops.Count)
	                {
	                    val = EditorEnumPop.EnumValuePops[index];
	                }
	                int enumValue = Convert.ToInt32(val);
	                if(finfo.FieldType == typeof(int)) finfo.SetValue(data,  (int)enumValue);
	                else if (finfo.FieldType == typeof(uint)) finfo.SetValue(data, (uint)enumValue);
	                else if (finfo.FieldType == typeof(byte)) finfo.SetValue(data, (byte)enumValue);
	                else if (finfo.FieldType == typeof(short)) finfo.SetValue(data, (short)enumValue);
	                else if (finfo.FieldType == typeof(ushort)) finfo.SetValue(data, (ushort)enumValue);
	                else if (finfo.FieldType == typeof(string)) finfo.SetValue(data, enumValue.ToString());
	            }
	            if (tableType != null)
	            {
	                if (GUILayout.Button("...", new GUILayoutOption[] { GUILayout.Width(20) }))
	                {
	//                     Data.Data_Base pTable = Data.DataEditorUtil.GetTable<Data.Data_Base>(tableType);
	//                     if(pTable!=null)
	//                     {
	//                         MethodInfo openWindow = ED.EditorUtil.GetPluginEditorWindowOpenFunc("FormWindow");
	//                         if (openWindow != null)
	//                         {
	//                             List<object> binds = new List<object>();
	//                             binds.Add(data);
	//                             binds.Add(finfo);
	//                             openWindow.Invoke(null, new object[] { pTable, strField, binds });
	//                         }
	//                     }
	                }
	            }
	            if (OnSubDraw != null) OnSubDraw(finfo.Name);
				if(drawLineRow!=null)
				{
                    data = drawLineRow.OnDrawLineRow(data, finfo, pParentData, parentFieldInfo);
                }
	            if(finfo.IsDefined(typeof(DrawProps.DefaultValueAttribute)))
	            {
	                DrawProps.DefaultValueAttribute def = finfo.GetCustomAttribute<DrawProps.DefaultValueAttribute>();
                    if (!string.IsNullOrEmpty(def.strValue) && GUILayout.Button(EditorGUIUtility.IconContent("d_RotateTool"), GUILayout.Width(24), GUILayout.Height(18)))
                    {
	                    if(finfo.FieldType == typeof(byte))
	                    {
	                        byte defaultValue;
	                        if(byte.TryParse(def.strValue, out defaultValue)) finfo.SetValue(data, defaultValue);
	                    }
	                    else if (finfo.FieldType == typeof(short))
	                    {
	                        short defaultValue;
	                        if (short.TryParse(def.strValue, out defaultValue)) finfo.SetValue(data, defaultValue);
	                    }
	                    else if (finfo.FieldType == typeof(ushort))
	                    {
	                        ushort defaultValue;
	                        if (ushort.TryParse(def.strValue, out defaultValue)) finfo.SetValue(data, defaultValue);
	                    }
	                    else if (finfo.FieldType == typeof(int))
	                    {
	                        int defaultValue;
	                        if (int.TryParse(def.strValue, out defaultValue)) finfo.SetValue(data, defaultValue);
	                    }
	                    else if (finfo.FieldType == typeof(uint))
	                    {
	                        uint defaultValue;
	                        if (uint.TryParse(def.strValue, out defaultValue)) finfo.SetValue(data, defaultValue);
	                    }
	                    else if (finfo.FieldType == typeof(float))
	                    {
	                        float defaultValue;
	                        if (float.TryParse(def.strValue, out defaultValue)) finfo.SetValue(data, defaultValue);
	                    }
	                    else if (finfo.FieldType == typeof(long))
	                    {
	                        long defaultValue;
	                        if (long.TryParse(def.strValue, out defaultValue)) finfo.SetValue(data, defaultValue);
	                    }
	                    else if (finfo.FieldType == typeof(ulong))
	                    {
	                        ulong defaultValue;
	                        if (ulong.TryParse(def.strValue, out defaultValue)) finfo.SetValue(data, defaultValue);
	                    }
	                    else if (finfo.FieldType == typeof(double))
	                    {
	                        double defaultValue;
	                        if (double.TryParse(def.strValue, out defaultValue)) finfo.SetValue(data, defaultValue);
	                    }
	                    else if (finfo.FieldType == typeof(string))
	                    {
	                        double defaultValue;
	                        if (double.TryParse(def.strValue, out defaultValue)) finfo.SetValue(data, defaultValue.ToString());
	                    }
                        else if (finfo.FieldType.IsEnum)
                        {
                            if(Enum.TryParse(finfo.FieldType, def.strValue, true, out var enumVal))
								finfo.SetValue(data, enumVal);
                        }
                    }
                }
	            if (bHorizontal)
	                EditorGUILayout.EndHorizontal();
	            if (OnDrawField != null) OnDrawField(data, finfo, false);
			}
            return data;
        }
        //------------------------------------------------------
        static bool IsScriptObject(System.Type type)
        {
            System.Type cur = type;
            while(cur != null)
            {
                if (cur == typeof(ScriptableObject)) return true;
                cur = cur.BaseType;
            }
            return false;
        }
        //------------------------------------------------------
        static bool IsUnityObject(System.Type type)
        {
            System.Type cur = type;
            while (cur != null)
            {
                if (cur == typeof(UnityEngine.Object)) return true;
                cur = cur.BaseType;
            }
            return false;
        }
        static object DealGeneic(FieldInfo field, GUIContent strDisplayName, IEnumerable list, Type keyType, Type valType, Type tableType, string strTableField)
        {
            bool bNoHeader= field.IsDefined(typeof(DrawProps.NoListHeaderAttribute));
            int curLent = 0;
            List<object> tempLists = null;
            if (list != null)
            {
                tempLists = new List<object>();
                foreach (var db in list)
                {
                    tempLists.Add(db);
                    curLent++;
                }
            }


            int lent = list != null ? curLent : 0;
            if (!bNoHeader)
                lent = EditorGUILayout.IntField(strDisplayName, lent);
            else
                EditorGUILayout.LabelField(strDisplayName);
            if (lent > 0 && list == null)
            {
                tempLists = new List<object>();
                if (valType != null)
                {
                    for (int i = 0; i < lent; ++i)
                    {
                        if (valType == typeof(string))
                            tempLists.Add("");
                        else
                            tempLists.Add(System.Activator.CreateInstance(valType));
                    }
                }
            }
            if (lent <= 0 && list != null) tempLists = new List<object>();

            if (tempLists != null)
            {
                if (lent > 0 && lent != tempLists.Count)
                {
                    if (lent > tempLists.Count)
                    {
                        for (int i = tempLists.Count; i < lent; ++i)
                        {
                            if(valType == typeof(string))
                                tempLists.Add("");
                            else
                                tempLists.Add(System.Activator.CreateInstance(valType));
                        }
                    }
                    else
                    {
                        while (lent < tempLists.Count)
                            tempLists.RemoveAt(tempLists.Count - 1);
                    }
                }
                if (valType == typeof(byte))
                {
                    byte[] temps = new byte[tempLists.Count];
                    Array.Copy(tempLists.ToArray(), temps, tempLists.Count);
                    EditorGUILayout.BeginVertical();
                    EditorGUI.indentLevel++;
                    for (int j = 0; j < temps.Length; ++j)
                    {
                        temps[j] = (byte)EditorGUILayout.IntField("[" + j + "]", (int)temps[j]);
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                    if (keyType.Name.Contains("Array") || keyType.Name.Contains("[]"))
                    {
                        return temps;
                    }
                    if (keyType.Name.Contains("List"))
                    {
                        return new List<byte>(temps);
                    }
                    if (keyType.Name.Contains("HashSet"))
                    {
                        HashSet<byte> vT = new HashSet<byte>();
                        for (int j = 0; j < temps.Length; ++j)
                            vT.Add(temps[j]);
                        return vT;
                    }
                }
                else if (valType == typeof(int))
                {
                    int[] temps = new int[tempLists.Count];
                    Array.Copy(tempLists.ToArray(), temps, tempLists.Count);
                    EditorGUILayout.BeginVertical();
                    EditorGUI.indentLevel++;
                    for (int j = 0; j < temps.Length; ++j)
                    {
                        temps[j] = EditorGUILayout.IntField("[" + j + "]", temps[j]);
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                    if (keyType.Name.Contains("Array") || keyType.Name.Contains("[]"))
                    {
                        return temps;
                    }
                    if (keyType.Name.Contains("List"))
                    {
                        return new List<int>(temps);
                    }
                    if (keyType.Name.Contains("HashSet"))
                    {
                        HashSet<int> vT = new HashSet<int>();
                        for (int j = 0; j < temps.Length; ++j)
                            vT.Add(temps[j]);
                        return vT;
                    }
                }
                else if (valType == typeof(uint))
                {
                    uint[] temps = new uint[tempLists.Count];
                    Array.Copy(tempLists.ToArray(), temps, tempLists.Count);
                    EditorGUILayout.BeginVertical();
                    EditorGUI.indentLevel++;
                    for (int j = 0; j < temps.Length; ++j)
                    {
                        temps[j] = (uint)EditorGUILayout.IntField("[" + j + "]", (int)temps[j]);
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                    if (keyType.Name.Contains("Array") || keyType.Name.Contains("[]"))
                    {
                        return temps;
                    }
                    if (keyType.Name.Contains("List"))
                    {
                        return new List<uint>(temps);
                    }
                    if (keyType.Name.Contains("HashSet"))
                    {
                        HashSet<uint> vT = new HashSet<uint>();
                        for (int j = 0; j < temps.Length; ++j)
                            vT.Add(temps[j]);
                        return vT;
                    }
                }
                else if (valType == typeof(short))
                {
                    short[] temps = new short[tempLists.Count];
                    Array.Copy(tempLists.ToArray(), temps, tempLists.Count);
                    EditorGUILayout.BeginVertical();
                    EditorGUI.indentLevel++;
                    for (int j = 0; j < temps.Length; ++j)
                    {
                        temps[j] = (short)EditorGUILayout.IntField("[" + j + "]", temps[j]);
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                    if (keyType.Name.Contains("Array") || keyType.Name.Contains("[]"))
                    {
                        return temps;
                    }
                    if (keyType.Name.Contains("List"))
                    {
                        return new List<short>(temps);
                    }
                    if (keyType.Name.Contains("HashSet"))
                    {
                        HashSet<short> vT = new HashSet<short>();
                        for (int j = 0; j < temps.Length; ++j)
                            vT.Add(temps[j]);
                        return vT;
                    }
                }
                else if (valType == typeof(ushort))
                {
                    ushort[] temps = new ushort[tempLists.Count];
                    Array.Copy(tempLists.ToArray(), temps, tempLists.Count);
                    EditorGUILayout.BeginVertical();
                    EditorGUI.indentLevel++;
                    for (int j = 0; j < temps.Length; ++j)
                    {
                        temps[j] = (ushort)EditorGUILayout.IntField("[" + j + "]", temps[j]);
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                    if (keyType.Name.Contains("Array") || keyType.Name.Contains("[]"))
                    {
                        return temps;
                    }
                    if (keyType.Name.Contains("List"))
                    {
                        return new List<ushort>(temps);
                    }
                    if (keyType.Name.Contains("HashSet"))
                    {
                        HashSet<ushort> vT = new HashSet<ushort>();
                        for (int j = 0; j < temps.Length; ++j)
                            vT.Add(temps[j]);
                        return vT;
                    }
                }
                else if (valType == typeof(float))
                {
                    float[] temps = new float[tempLists.Count];
                    Array.Copy(tempLists.ToArray(), temps, tempLists.Count);
                    EditorGUILayout.BeginVertical();
                    EditorGUI.indentLevel++;
                    for (int j = 0; j < temps.Length; ++j)
                    {
                        temps[j] = (float)EditorGUILayout.FloatField("[" + j + "]", temps[j]);
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                    if (keyType.Name.Contains("Array") || keyType.Name.Contains("[]"))
                    {
                        return temps;
                    }
                    if (keyType.Name.Contains("List"))
                    {
                        return new List<float>(temps);
                    }
                    if (keyType.Name.Contains("HashSet"))
                    {
                        HashSet<float> vT = new HashSet<float>();
                        for (int j = 0; j < temps.Length; ++j)
                            vT.Add(temps[j]);
                        return vT;
                    }
                }
                else if (valType == typeof(Vector2))
                {
                    Vector2[] temps = new Vector2[tempLists.Count];
                    Array.Copy(tempLists.ToArray(), temps, tempLists.Count);
                    EditorGUILayout.BeginVertical();
                    EditorGUI.indentLevel++;
                    for (int j = 0; j < temps.Length; ++j)
                    {
                        temps[j] = (Vector2)EditorGUILayout.Vector2Field("[" + j + "]", temps[j]);
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                    if (keyType.Name.Contains("Array") || keyType.Name.Contains("[]"))
                    {
                        return temps;
                    }
                    if (keyType.Name.Contains("List"))
                    {
                        return new List<Vector2>(temps);
                    }
                    if (keyType.Name.Contains("HashSet"))
                    {
                        HashSet<Vector2> vT = new HashSet<Vector2>();
                        for (int j = 0; j < temps.Length; ++j)
                            vT.Add(temps[j]);
                        return vT;
                    }
                }
                else if (valType == typeof(Vector3))
                {
                    Vector3[] temps = new Vector3[tempLists.Count];
                    Array.Copy(tempLists.ToArray(), temps, tempLists.Count);
                    EditorGUILayout.BeginVertical();
                    EditorGUI.indentLevel++;
                    for (int j = 0; j < temps.Length; ++j)
                    {
                        temps[j] = (Vector3)EditorGUILayout.Vector3Field("[" + j + "]", temps[j]);
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                    if (keyType.Name.Contains("Array") || keyType.Name.Contains("[]"))
                    {
                        return temps;
                    }
                    if (keyType.Name.Contains("List"))
                    {
                        return new List<Vector3>(temps);
                    }
                    if (keyType.Name.Contains("HashSet"))
                    {
                        HashSet<Vector3> vT = new HashSet<Vector3>();
                        for (int j = 0; j < temps.Length; ++j)
                            vT.Add(temps[j]);
                        return vT;
                    }
                }
                else if (valType == typeof(string))
                {
                    string[] temps = new string[tempLists.Count];
                    Array.Copy(tempLists.ToArray(), temps, tempLists.Count);
                    EditorGUILayout.BeginVertical();
                    EditorGUI.indentLevel++;
                    for (int j = 0; j < temps.Length; ++j)
                    {
                        temps[j] = EditorGUILayout.TextField("[" + j + "]", temps[j]);
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                    if (keyType.Name.Contains("Array") || keyType.Name.Contains("[]"))
                    {
                        return temps;
                    }
                    if (keyType.Name.Contains("List"))
                    {
                        return new List<string>(temps);
                    }
                    if (keyType.Name.Contains("HashSet"))
                    {
                        HashSet<string> vT = new HashSet<string>();
                        for (int j = 0; j < temps.Length; ++j)
                            vT.Add(temps[j]);
                        return vT;
                    }
                }
                else if (valType.IsEnum)
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUI.indentLevel++;
                    Enum[] vTemps = new Enum[tempLists.Count];
                    try
                    {
                        for (int j = 0; j < tempLists.Count; ++j)
                        {
                            vTemps[j] = (Enum)tempLists[j];
                        }
                        for (int j = 0; j < vTemps.Length; ++j)
                        {
                            vTemps[j] = PopEnum("[" + j + "]", vTemps[j], valType);
                        }

                    }
                    catch (System.Exception ex)
                    {

                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                    if (keyType.Name.Contains("Array") || keyType.Name.Contains("[]"))
                    {
                        Array list1 = (Array)list;
                        if (list1 == null || list1.Length != vTemps.Length) list1 = Array.CreateInstance(valType, vTemps.Length);
                        for (int j = 0; j < vTemps.Length; ++j)
                        {
                            list1.SetValue(vTemps[j],j);
                        }
                        return list1;
                    }
                    if (keyType.Name.Contains("List"))
                    {
                        IList list1 = (IList)list;
                        if (list1 == null)
                        {
                            list1 = (IList)Activator.CreateInstance(keyType.GetType());
                            list = list1;
                        }
                        MethodInfo clearMethod = list.GetType().GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance);
                        MethodInfo addMethod = list.GetType().GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);
                        if (clearMethod != null) clearMethod.Invoke(list, null);
                        if (addMethod != null)
                        {
                            for (int j = 0; j < vTemps.Length; ++j)
                                addMethod.Invoke(list, new object[] { (Enum)vTemps[j] });
                        }
                        return new List<Enum>(vTemps);
                    }
                    if (keyType.Name.Contains("HashSet"))
                    {
                        if (list == null) list = (IEnumerable)Activator.CreateInstance(list.GetType());
                        MethodInfo clearMethod = list.GetType().GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance);
                        MethodInfo addMethod = list.GetType().GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);
                        if (clearMethod != null) clearMethod.Invoke(list, null);
                        if(addMethod!=null)
                        {
                            for (int j = 0; j < vTemps.Length; ++j)
                                addMethod.Invoke(list,new object[] { (Enum)vTemps[j] });
                        }
                        return list;
                    }
                }
                else if(valType.BaseType == typeof(ValueType) || valType.IsClass)
                {
                    System.Object[] temps = new System.Object[tempLists.Count];
                    Array.Copy(tempLists.ToArray(), temps, tempLists.Count);
                    EditorGUILayout.BeginVertical();
                    EditorGUI.indentLevel++;
                    for (int j = 0; j < temps.Length; ++j)
                    {
                        MethodInfo method = temps[j].GetType().GetMethod("OnInspector");
                        if (method != null && temps[j] != null)
                        {
                            System.Object result = method.Invoke(temps[j], new System.Object[] { j.ToString() });
                            if (result != null && result.GetType() == temps[j].GetType())
                                temps[j] = result;
                        }
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                    if (keyType.Name.Contains("Array") || keyType.Name.Contains("[]"))
                    {
                        Array list1 = (Array)list;
                        if (list1 == null || list1.Length != temps.Length) list1 = Array.CreateInstance(valType, temps.Length);
                        for (int j = 0; j < temps.Length; ++j)
                        {
                            list1.SetValue(temps[j], j);
                        }
                        return list1;
                    }
                    if (keyType.Name.Contains("List"))
                    {
                        IList list1 = (IList)list;
                        if (list1 == null)
                        {
                            list1 = (IList)Activator.CreateInstance(keyType);
                        }
                        MethodInfo clearMethod = list1.GetType().GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance);
                        MethodInfo addMethod = list1.GetType().GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);
                        if (clearMethod != null) clearMethod.Invoke(list1, null);
                        if (addMethod != null)
                        {
                            for (int j = 0; j < temps.Length; ++j)
                                addMethod.Invoke(list1, new object[] { temps[j] });
                        }
                        return list1;
                    }
                }
            }
            return list;
        }
    }
}
#endif