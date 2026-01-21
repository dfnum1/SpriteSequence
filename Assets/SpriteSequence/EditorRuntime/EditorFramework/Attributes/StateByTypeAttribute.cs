/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	StateByTypeAttribute
作    者:	HappLI
描    述:	显示为某个类型
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    public class StateByTypeAttribute : Attribute
    {
#if UNITY_EDITOR
        public System.Collections.Generic.HashSet<Type> typeSets = new System.Collections.Generic.HashSet<Type>();
#endif
        public StateByTypeAttribute(Type type)
        {
#if UNITY_EDITOR
            typeSets.Add(type);
#endif
        }
        public StateByTypeAttribute(Type[] types)
        {
#if UNITY_EDITOR
            if (types == null) return;
            for (int i = 0; i < types.Length; ++i)
                this.typeSets.Add(types[i]);
#endif
        }
    }
}