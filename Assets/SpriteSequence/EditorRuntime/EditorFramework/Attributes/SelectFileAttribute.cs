/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	SelectFileAttribute
作    者:	HappLI
描    述:	选择文件
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    public class SelectFileAttribute : Attribute
    {
#if UNITY_EDITOR
        public string root;
        public string subRoot;
        public string extend="*.*";
        public bool includeExtend = true;
#endif
        public SelectFileAttribute()
        {
#if UNITY_EDITOR
            this.root = null;
#endif
        }
        public SelectFileAttribute(string root, string subRoot = "", string extend = null, bool includeExtend = true)
        {
#if UNITY_EDITOR
            this.root = root;
            this.subRoot = subRoot;
            this.extend = extend;
            this.includeExtend = includeExtend;
#endif
        }
    }
}