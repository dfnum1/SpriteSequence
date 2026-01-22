/********************************************************************
生成日期:	01:20:2026
类    名: 	SpriteSequenceManager
作    者:	HappLI
描    述:	基于Sprite的序列帧动画管理类
*********************************************************************/
using System.Collections.Generic;
using UnityEngine;

namespace Framework.SpriteSeq
{
    public class SpriteSequenceManager
    {
        private Dictionary<long, FrameSequenceRenderer> m_vRenders;
        private Dictionary<int, long> m_vGuidKey = null;
        //--------------------------------------------------------
        public void AddSequence(int guid, SpriteSequenceData sequenceData, string label, int sortingOrder = 0)
        {
            if (sequenceData == null) return;
            if (m_vGuidKey != null && m_vGuidKey.ContainsKey(guid))
                return;

            long id = ((long)sequenceData.vtPack.GetInstanceID())<<32 | (long)sequenceData.atlasTexture.GetInstanceID();
            FrameSequenceRenderer renderer =null;
            if (m_vRenders == null )
            {
                m_vRenders = new Dictionary<long, FrameSequenceRenderer>(64);

                renderer = new FrameSequenceRenderer();
                renderer.SetData(sequenceData);
                renderer.Init();
                m_vRenders[id] = renderer;
            }
            else
            {
                if(!m_vRenders.TryGetValue(id, out renderer))
                {
                    renderer = new FrameSequenceRenderer();
                    renderer.SetData(sequenceData);
                    renderer.Init();
                    m_vRenders[id] = renderer;
                }
            }
            renderer.AddSequence(guid, label);
            if (m_vGuidKey == null)
                m_vGuidKey = new Dictionary<int, long>(64);
            m_vGuidKey[guid] = id;
        }
        //--------------------------------------------------------
        public void RemoveSequence(int guid)
        {
            if (m_vGuidKey == null)
                return;
            if(m_vGuidKey.TryGetValue(guid, out var id))
            {
                if (m_vRenders.TryGetValue(id, out var renderer))
                {
                    renderer.RemoveSequence(guid);
                }
                m_vGuidKey.Remove(guid);
            }
        }
        //--------------------------------------------------------
        public void SetPosition(int guid, Vector3 pos)
        {
            if (m_vGuidKey == null)
                return;
            if (m_vGuidKey.TryGetValue(guid, out var id))
            {
                if (m_vRenders.TryGetValue(id, out var renderer))
                {
                    renderer.SetPosition(guid, pos);
                }
            }
        }
        //--------------------------------------------------------
        public void SetEulerAngle(int guid, Vector3 eulerAngle)
        {
            if (m_vGuidKey == null)
                return;
            if (m_vGuidKey.TryGetValue(guid, out var id))
            {
                if (m_vRenders.TryGetValue(id, out var renderer))
                {
                    renderer.SetEulerAngle(guid, eulerAngle);
                }
            }
        }
        //--------------------------------------------------------
        public void SetScale(int guid, Vector3 scale)
        {
            if (m_vGuidKey == null)
                return;
            if (m_vGuidKey.TryGetValue(guid, out var id))
            {
                if (m_vRenders.TryGetValue(id, out var renderer))
                {
                    renderer.SetScale(guid, scale);
                }
            }
        }
        //--------------------------------------------------------
        public void SetColor(int guid, Color color)
        {
            if (m_vGuidKey == null)
                return;
            if (m_vGuidKey.TryGetValue(guid, out var id))
            {
                if (m_vRenders.TryGetValue(id, out var renderer))
                {
                    renderer.SetColor(guid, color);
                }
            }
        }
        //--------------------------------------------------------
        public void SetSequenceRange(int guid,int begin, int end)
        {
            if (m_vGuidKey == null)
                return;
            if (m_vGuidKey.TryGetValue(guid, out var id))
            {
                if (m_vRenders.TryGetValue(id, out var renderer))
                {
                    renderer.SetSequenceRange(guid, begin, end);
                }
            }
        }
        //--------------------------------------------------------
        public void SetSortingOrder(int guid, int order)
        {
            if (m_vGuidKey == null)
                return;
            if (m_vGuidKey.TryGetValue(guid, out var id))
            {
                if (m_vRenders.TryGetValue(id, out var renderer))
                {
                    renderer.SetSortingOrder(guid, order);
                }
            }
        }
        //--------------------------------------------------------
        public void SetCullingSize(int guid, float factor)
        {
            if (m_vGuidKey == null)
                return;
            if (m_vGuidKey.TryGetValue(guid, out var id))
            {
                if (m_vRenders.TryGetValue(id, out var renderer))
                {
                    renderer.SetCullingSize(guid, factor);
                }
            }
        }
        //--------------------------------------------------------
        public void SetVisible(int guid, bool bVisible)
        {
            if (m_vGuidKey == null)
                return;
            if (m_vGuidKey.TryGetValue(guid, out var id))
            {
                if (m_vRenders.TryGetValue(id, out var renderer))
                {
                    renderer.SetVisible(guid, bVisible);
                }
            }
        }
        //--------------------------------------------------------
        public bool IsVisible(int guid)
        {
            if (m_vGuidKey == null)
                return false;
            if (m_vGuidKey.TryGetValue(guid, out var id))
            {
                if (m_vRenders.TryGetValue(id, out var renderer))
                {
                    return renderer.IsVisible(guid);
                }
            }
            return false;
        }
        //--------------------------------------------------------
        public void Render(Camera camera = null)
        {
            if (m_vRenders == null)
                return;
            foreach(var renderer in m_vRenders.Values)
            {
                renderer.CameraRender(camera);
            }
        }
        //--------------------------------------------------------
        public void Destroy()
        {
            if (m_vRenders == null)
                return;
            foreach (var renderer in m_vRenders.Values)
            {
                renderer.Destroy();
            }
            m_vRenders.Clear();
        }
    }
}
