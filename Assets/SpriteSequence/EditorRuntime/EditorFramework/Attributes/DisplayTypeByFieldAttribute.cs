using System;

namespace Framework.DrawProps
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class DisplayTypeByFieldAttribute : Attribute
    {
#if UNITY_EDITOR
        public string byTypeName;
        System.Type byType;
        public string fieldName;
        public string fieldValue;
        public bool bBit = false;
        public bool bEnumBitOffset = false;
#endif
        public DisplayTypeByFieldAttribute(string fieldName, string fieldValue, System.Type byType, bool bBit = false, bool bEnumBitOffset = false)
        {
#if UNITY_EDITOR
            this.fieldName = fieldName;
            this.fieldValue = fieldValue.ToLower();
            this.byType = byType;
            this.bBit = bBit;
            this.bEnumBitOffset = bEnumBitOffset;
#endif
        }
        public DisplayTypeByFieldAttribute(string fieldName, string fieldValue, string byType, bool bBit = false, bool bEnumBitOffset = false)
        {
#if UNITY_EDITOR
            this.fieldName = fieldName;
            this.fieldValue = fieldValue.ToLower();
            this.byTypeName = byType;
            this.bBit = bBit;
            this.bEnumBitOffset = bEnumBitOffset;
#endif
        }
#if UNITY_EDITOR
        //-------------------------------------------------
        public System.Type GetDisType()
        {
            if (byType != null) return byType;
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(byTypeName))
                this.byType = ED.EditorUtils.GetTypeByName(byTypeName);
#endif
            return byType;
        }
#endif
    }
}