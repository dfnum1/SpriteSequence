/********************************************************************
生成日期:	01:20:2026
类    名: 	SpriteSequenceData
作    者:	HappLI
描    述:	基于Sprite的序列帧动画数据
*********************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Framework.SpriteSeq
{
    public class SpriteSequenceData : ScriptableObject
    {
        [System.Serializable]
        public struct SubSequence
        {
            public string label;
            public int beginFrame;
            public int endFrame;
            public int sortingOrder;
            public float cullingSize;
            public Vector2 centerOffset;
            public bool IsValid()
            {
                return !string.IsNullOrEmpty(label) && beginFrame >= 0 && endFrame >= beginFrame;
            }
        }
        public static SubSequence Null = new SubSequence() { label = null, beginFrame = -1, endFrame = -1 };
#if UNITY_EDITOR
        public SpriteAtlas altas;
#endif

        [SerializeField]
        private Texture2D m_atlasTexture;

        public Texture2D atlasTexture { get { return m_atlasTexture; } }

        [SerializeField]
        private Texture2D m_vtPack;
        public Texture2D vtPack { get { return m_vtPack; } }

        [SerializeField]
        private Texture2D m_vtIndex;
        public Texture2D vtIndex { get { return m_vtIndex; } }

        public int frameRate = 12;
        public int vertexPerFrame;
        public int indexPerFrame;
        public List<SubSequence> subSequeneces;

        //--------------------------------------------------------
        public SubSequence GetSequence(string label)
        {
            if (string.IsNullOrEmpty(label))
                return Null;
            foreach (var seq in subSequeneces)
            {
                if (seq.label.Equals(label, StringComparison.OrdinalIgnoreCase))
                {
                    return seq;
                }
            }
            return Null;
        }
    }
}