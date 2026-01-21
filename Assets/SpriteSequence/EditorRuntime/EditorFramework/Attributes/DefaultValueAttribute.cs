/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	DefaultValueAttribute
作    者:	HappLI
描    述:	缺省值
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)]
    public class DefaultValueAttribute : Attribute
    {
#if UNITY_EDITOR
        public string strValue;
#endif
        public DefaultValueAttribute(System.Object defValue)
        {
#if UNITY_EDITOR
            this.strValue = defValue.ToString();
#endif
        }
#if UNITY_EDITOR
        //-----------------------------------------------------
        public T ToValue<T>(T defVal = default)
        {
            try
            {
                if (string.IsNullOrEmpty(strValue))
                    return defVal;

                Type targetType = typeof(T);

                // 先特殊处理枚举
                if (targetType.IsEnum)
                {
                    // 支持名称和数字
                    if (Enum.TryParse(targetType, strValue, true, out object enumVal))
                        return (T)enumVal;
                    // 尝试数字转枚举
                    if (int.TryParse(strValue, out int intVal))
                        return (T)Enum.ToObject(targetType, intVal);
                    return defVal;
                }

                // 支持所有常见数值类型
                if (targetType == typeof(byte))
                    return (T)(object)byte.Parse(strValue);
                if (targetType == typeof(bool))
                    return  (T)(object)strValue.Equals("true", StringComparison.OrdinalIgnoreCase);
                if (targetType == typeof(short))
                    return (T)(object)short.Parse(strValue);
                if (targetType == typeof(ushort))
                    return (T)(object)ushort.Parse(strValue);
                if (targetType == typeof(int))
                    return (T)(object)int.Parse(strValue);
                if (targetType == typeof(uint))
                    return (T)(object)uint.Parse(strValue);
                if (targetType == typeof(long))
                    return (T)(object)long.Parse(strValue);
                if (targetType == typeof(ulong))
                    return (T)(object)ulong.Parse(strValue);
                if (targetType == typeof(float))
                    return (T)(object)float.Parse(strValue);
                if (targetType == typeof(double))
                    return (T)(object)double.Parse(strValue);
                if (targetType == typeof(decimal))
                    return (T)(object)decimal.Parse(strValue);

                // 其它类型尝试通用转换
                return (T)Convert.ChangeType(strValue, targetType);
            }
            catch
            {
                return defVal;
            }
        }
#endif
    }
    //-----------------------------------------------------
    [AttributeUsage(AttributeTargets.Field)]
    public class EditFloatAttribute : Attribute
    {
#if UNITY_EDITOR
        public int factor = 1000;
#endif
        public EditFloatAttribute(int factor = 1000)
        {
#if UNITY_EDITOR
            this.factor = factor;
#endif
        }
    }
}