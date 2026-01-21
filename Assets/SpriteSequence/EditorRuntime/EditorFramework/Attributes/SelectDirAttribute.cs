/********************************************************************
生成日期:		1:11:2020 10:06
类    名: 	SelectDirAttribute
作    者:	HappLI
描    述:	选择目录
*********************************************************************/
using System;

namespace Framework.DrawProps
{
    public class SelectDirAttribute : Attribute
    {
#if UNITY_EDITOR
        public string root;
#endif
        public SelectDirAttribute()
        {
#if UNITY_EDITOR
            this.root = null;
#endif
        }
        public SelectDirAttribute(string root)
        {
#if UNITY_EDITOR
            this.root = root;
#endif
        }
    }
}