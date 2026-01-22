/********************************************************************
生成日期:	01:20:2026
类    名: 	FrameSequenceRenderer
作    者:	HappLI
描    述:	基于Sprite的序列帧渲染器
*********************************************************************/
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Framework.SpriteSeq
{
    //--------------------------------------------------------
    //! FrameSequenceRenderer
    //--------------------------------------------------------
    internal class FrameSequenceRenderer
    {
#if UNITY_2022_1_OR_NEWER
        static int BatchMaxCount = 8191;
#else
        static int BatchMaxCount = 1023;
#endif
        static Matrix4x4[]          ms_arrInstacneMatrices;
        static float[]              ms_arrInstanceFrames;
        static Vector4[]            ms_arrInstanceColors;

        static int                  _VtPack = Shader.PropertyToID("_VtPack");
        static int                  _VtIndex = Shader.PropertyToID("_VtIndex");
        static int                  _MainTex = Shader.PropertyToID("_MainTex");
        static int                  _GPU_Frame_PixelSegmentation = Shader.PropertyToID("_GPU_Frame_PixelSegmentation");
        static int                  _GPU_Frame_ColorSegmentation = Shader.PropertyToID("_GPU_Frame_ColorSegmentation");
        public struct DrawData
        {
            public int guid;
            public Matrix4x4 matrix;
            public float frame;
            public Vector4 color;
            public int sequenceBegin;
            public int sequenceEnd;
            public int order;
            public float cullingSize;
            public Vector2 centerOffset;
            public bool visible;
            public bool isCulled;
        }


        NativeArray<DrawData>       m_arrNativeDraws;
        private bool                m_bDirtyOrder = false;


        SpriteSequenceData          m_pData;
        MaterialPropertyBlock       m_mpBlock;

        int                         m_nCount = 0;
        float                       m_fFps = 12.0f;

        Material                    m_pMaterial = null;
        Mesh                        m_pDrawMesh = null;

        //! job
        JobHandle                   m_SeqFrameJobHandle;

        NativeHashMap<int, int>     m_guidToIndex;
        //--------------------------------------------------------
        internal void SetData(SpriteSequenceData pData)
        {
            m_pData = pData;
            if(m_pDrawMesh !=null)
            {
                if(m_pDrawMesh.vertexCount != pData.indexPerFrame)
                {
                    DestortyMesh();
                }
            }
            if (m_pDrawMesh == null)
            {
                m_pDrawMesh = new Mesh();
                m_pDrawMesh.name = "SpriteSeqGPUMesh";
                if (pData.indexPerFrame <= 0) pData.indexPerFrame = pData.vertexPerFrame;

                List<ushort> vIndices = new List<ushort>(pData.indexPerFrame);
                for (int i = 0; i < pData.indexPerFrame; ++i) vIndices.Add((ushort)i);

                m_pDrawMesh.SetVertices(new Vector3[pData.indexPerFrame]);
                m_pDrawMesh.SetTriangles(vIndices, 0);
                m_pDrawMesh.UploadMeshData(true);
                m_pDrawMesh.hideFlags |= HideFlags.DontSave;
            }

            m_fFps = Mathf.Max(1, pData.frameRate);

            if (m_pMaterial == null) m_pMaterial = new Material(Shader.Find("Custom/VtPackSpriteSeq"));
            m_pMaterial.hideFlags |= HideFlags.DontSave;
            m_pMaterial.enableInstancing = true;
            m_pMaterial.SetTexture(_VtPack, m_pData.vtPack);
            m_pMaterial.SetTexture(_VtIndex, m_pData.vtIndex);
            m_pMaterial.SetTexture(_MainTex, m_pData.atlasTexture);
        }
        //--------------------------------------------------------
        public void Init()
        {
            m_nCount = 0;
            if (m_pData == null)
                return;

            m_mpBlock = new MaterialPropertyBlock();
            PrepareInit();
        }
        //--------------------------------------------------------
        void PrepareInit()
        {
            if (!m_arrNativeDraws.IsCreated)
                m_arrNativeDraws = new NativeArray<DrawData>(BatchMaxCount, Allocator.Persistent);
        }
        //--------------------------------------------------------
        public void SetFps(float fps)
        {
            m_fFps = fps;
        }
        //--------------------------------------------------------
        public void ClearSequence()
        {
            if(m_mpBlock!=null) m_mpBlock.Clear();
            m_nCount = 0;
            if (m_guidToIndex.IsCreated) m_guidToIndex.Clear();
        }
        //--------------------------------------------------------
        public void AddSequence(int guid, string label, int order =0)
        {
            if (m_guidToIndex.IsCreated && m_guidToIndex.ContainsKey(guid))
                return;

            var subSequence = m_pData.GetSequence(label);
            if(!subSequence.IsValid())
            {
                Debug.LogError($"SpriteSequenceManager AddSequence faild, not found label:{label}");
                return;
            }

            PrepareInit();
            if(m_nCount>= m_arrNativeDraws.Length)
            {
                int growthSize = m_arrNativeDraws.Length * 2;
                var newArray = new NativeArray<DrawData>(growthSize, Allocator.Persistent);
                NativeArray<DrawData>.Copy(m_arrNativeDraws, newArray, m_arrNativeDraws.Length);
                m_arrNativeDraws.Dispose();
                m_arrNativeDraws = newArray;
            }

            if (!m_guidToIndex.IsCreated)
            {
                m_guidToIndex = new NativeHashMap<int, int>(BatchMaxCount, Allocator.Persistent);
            }
            m_guidToIndex[guid] = m_nCount;
            if (order == 0) order = subSequence.sortingOrder;

            DrawData draw = new DrawData();
            draw.guid = guid;
            draw.matrix = Matrix4x4.identity;
            draw.color = Color.white;
            draw.frame = subSequence.beginFrame;
            draw.sequenceBegin = subSequence.beginFrame;
            draw.sequenceEnd = subSequence.endFrame;
            draw.visible = true;
            draw.isCulled = false;
            draw.order = order;
            draw.cullingSize = 0;
            draw.centerOffset = subSequence.centerOffset;

            m_bDirtyOrder = true;
            m_arrNativeDraws[m_nCount] = draw;

            m_nCount++;
        }
        //--------------------------------------------------------
        public void RemoveSequence(int guid)
        {
            if (!m_guidToIndex.IsCreated || !m_guidToIndex.TryGetValue(guid, out int idx))
                return;

            m_guidToIndex.Remove(guid);

            int lastIdx = m_nCount - 1;
            if (idx != lastIdx)
            {
                int lastGuid = m_arrNativeDraws[lastIdx].guid;
                m_arrNativeDraws[idx] = m_arrNativeDraws[lastIdx];
                m_guidToIndex[lastGuid] = idx;
            }
            m_nCount--;
        }
        //--------------------------------------------------------
        public void SetPosition(int guid, Vector3 pos)
        {
            if (!m_guidToIndex.IsCreated || !m_guidToIndex.TryGetValue(guid, out int idx))
                return;
            var draw = m_arrNativeDraws[idx];
            Quaternion rot = draw.matrix.rotation;
            Vector3 scale = draw.matrix.lossyScale;
            draw.matrix = Matrix4x4.TRS(pos, rot, scale);
            m_arrNativeDraws[idx] = draw;
        }
        //--------------------------------------------------------
        public void SetEulerAngle(int guid, Vector3 eulerAngle)
        {
            if (!m_guidToIndex.IsCreated || !m_guidToIndex.TryGetValue(guid, out int idx))
                return;
            var draw = m_arrNativeDraws[idx];
            Vector3 pos = draw.matrix.GetColumn(3);
            Vector3 scale = draw.matrix.lossyScale;
            draw.matrix = Matrix4x4.TRS(pos, Quaternion.Euler(eulerAngle), scale);
            m_arrNativeDraws[idx] = draw;
        }
        //--------------------------------------------------------
        public void SetScale(int guid, Vector3 scale)
        {
            if (!m_guidToIndex.IsCreated || !m_guidToIndex.TryGetValue(guid, out int idx))
                return;
            var draw = m_arrNativeDraws[idx];
            Vector3 pos = draw.matrix.GetColumn(3);
            Quaternion rot = draw.matrix.rotation;
            draw.matrix = Matrix4x4.TRS(pos, rot, scale);
            m_arrNativeDraws[idx] = draw;
        }
        //--------------------------------------------------------
        public Matrix4x4 GetWorldMatrix(int guid)
        {
            if (!m_guidToIndex.IsCreated || !m_guidToIndex.TryGetValue(guid, out int idx))
                return Matrix4x4.identity;
            return m_arrNativeDraws[idx].matrix;
        }
        //--------------------------------------------------------
        public void SetColor(int guid, Color color)
        {
            if (!m_guidToIndex.IsCreated || !m_guidToIndex.TryGetValue(guid, out int idx))
                return;
            var draw = m_arrNativeDraws[idx];
            draw.color = color;
            m_arrNativeDraws[idx] = draw;
        }
        //--------------------------------------------------------
        public void SetSortingOrder(int guid, int order)
        {
            if (!m_guidToIndex.IsCreated || !m_guidToIndex.TryGetValue(guid, out int idx))
                return;
            var draw = m_arrNativeDraws[idx];
            order = order + (int)(draw.matrix.GetPosition().z*100);
            if (draw.order == order)
                return;
            draw.order = order;
            m_bDirtyOrder = true;
            m_arrNativeDraws[idx] = draw;
        }
        //--------------------------------------------------------
        public void SetCullingSize(int guid, float size)
        {
            if (!m_guidToIndex.IsCreated || !m_guidToIndex.TryGetValue(guid, out int idx))
                return;
            var draw = m_arrNativeDraws[idx];
            draw.cullingSize = size;
            m_arrNativeDraws[idx] = draw;
        }
        //--------------------------------------------------------
        public void SetVisible(int guid, bool bVisible)
        {
            if (!m_guidToIndex.IsCreated || !m_guidToIndex.TryGetValue(guid, out int idx))
                return;
            var draw = m_arrNativeDraws[idx];
            draw.visible = bVisible;
            m_arrNativeDraws[idx] = draw;
        }
        //--------------------------------------------------------
        public bool IsVisible(int guid)
        {
            if (!m_guidToIndex.IsCreated || !m_guidToIndex.TryGetValue(guid, out int idx))
                return false;
            return m_arrNativeDraws[idx].visible;
        }
        //--------------------------------------------------------
        public void SetSequenceRange(int guid, int begin, int end)
        {
            if (!m_guidToIndex.IsCreated || !m_guidToIndex.TryGetValue(guid, out int idx))
                return;
            var draw = m_arrNativeDraws[idx];
            draw.sequenceBegin = begin;
            draw.sequenceEnd = end;
            if (draw.frame < begin) draw.frame = begin;
            else if (draw.frame > end) draw.frame = end;
            m_arrNativeDraws[idx] = draw;
        }
        //--------------------------------------------------------
        void UpdateJob(float deltaTime, Matrix4x4 cullingMatrix, bool bCullingCheck = true)
        {
            if(m_bDirtyOrder)
            {
                var sortJob = new SortUpdateJob
                {
                    drawDatas = m_arrNativeDraws,
                    guideToIndexs = m_guidToIndex,
                    count = m_nCount
                };
                var jobDep = sortJob.Schedule();

                var job = new FrameUpdateJob();
                job.fps = m_fFps;
                job.dt = deltaTime;
                job.drawCount = m_nCount;
                job.drawDatas = m_arrNativeDraws;
                job.cullingCheck = bCullingCheck;
                job.cullingMatrix = cullingMatrix;
                m_SeqFrameJobHandle = job.Schedule(jobDep);
                m_SeqFrameJobHandle.Complete();
            }
            else
            {
                var job = new FrameUpdateJob();
                job.fps = m_fFps;
                job.dt = deltaTime;
                job.drawCount = m_nCount;
                job.drawDatas = m_arrNativeDraws;
                job.cullingCheck = bCullingCheck;
                job.cullingMatrix = cullingMatrix;
                m_SeqFrameJobHandle = job.Schedule();
                m_SeqFrameJobHandle.Complete();
            }
            m_bDirtyOrder = false;
        }
        //--------------------------------------------------------
        public void CameraRender(Camera camera, int drawLayer =0)
        {
            if (m_nCount <= 0 ||
                m_arrNativeDraws == null ||
                m_pDrawMesh == null || 
                m_pMaterial == null)
                return;

            if(camera!=null)
                UpdateJob(Time.deltaTime, camera.cullingMatrix, true);
            else
                UpdateJob(Time.deltaTime, Matrix4x4.identity, false);


            if (m_mpBlock == null) m_mpBlock = new MaterialPropertyBlock();
            if (ms_arrInstacneMatrices == null || ms_arrInstacneMatrices.Length < BatchMaxCount)
                ms_arrInstacneMatrices = new Matrix4x4[BatchMaxCount];
            if (ms_arrInstanceFrames == null || ms_arrInstanceFrames.Length < BatchMaxCount)
                ms_arrInstanceFrames = new float[BatchMaxCount];
            if (ms_arrInstanceColors == null || ms_arrInstanceColors.Length < BatchMaxCount)
                ms_arrInstanceColors = new Vector4[BatchMaxCount];

            int batchCount = 0;
            for (int i = 0; i < m_nCount; ++i)
            {
                var draw = m_arrNativeDraws[i];
                if (!draw.visible || draw.isCulled) continue;

                ms_arrInstacneMatrices[batchCount] = draw.matrix;
                ms_arrInstanceFrames[batchCount] = draw.frame;
                ms_arrInstanceColors[batchCount] = draw.color;
                batchCount++;

                if (batchCount >= BatchMaxCount)
                {
                    m_mpBlock.SetFloatArray(_GPU_Frame_PixelSegmentation, ms_arrInstanceFrames);
                    m_mpBlock.SetVectorArray(_GPU_Frame_ColorSegmentation, ms_arrInstanceColors);
                    Graphics.DrawMeshInstanced(m_pDrawMesh, 0, m_pMaterial, ms_arrInstacneMatrices, batchCount, m_mpBlock, ShadowCastingMode.Off, false, drawLayer, camera);
                    batchCount = 0;
                }
            }
            if (batchCount > 0)
            {
                m_mpBlock.SetFloatArray(_GPU_Frame_PixelSegmentation, ms_arrInstanceFrames);
                m_mpBlock.SetVectorArray(_GPU_Frame_ColorSegmentation, ms_arrInstanceColors);
                Graphics.DrawMeshInstanced(m_pDrawMesh, 0, m_pMaterial, ms_arrInstacneMatrices, batchCount, m_mpBlock, ShadowCastingMode.Off, false, drawLayer, camera);
            }
        }
#if UNITY_EDITOR
        //--------------------------------------------------------
        internal void EditorRender(float deltaTime, Camera camera, int drawLayer)
        {
            if (m_nCount <= 0 ||
                m_arrNativeDraws == null ||
                m_pDrawMesh == null ||
                m_pMaterial == null)
                return;

            UpdateJob(deltaTime, camera.cullingMatrix, true);

            if (!Application.isPlaying)
                JobHandle.ScheduleBatchedJobs();
            for (int i = 0; i < m_nCount; ++i)
            {
                var batch = m_arrNativeDraws[i];
                if (!batch.visible || batch.isCulled) continue;
                    m_pMaterial.SetFloat(_GPU_Frame_PixelSegmentation, batch.frame);
                    m_pMaterial.SetVector(_GPU_Frame_ColorSegmentation, batch.color);

                    m_pMaterial.SetPass(0);
                    Graphics.DrawMeshNow(m_pDrawMesh, batch.matrix);
                m_arrNativeDraws[i] = batch;
                 //   Graphics.DrawMesh(m_pDrawMesh, batch.matrices[f],m_pMaterial, drawLayer, camera);
            }
        }
#endif
        //--------------------------------------------------------
        void DestortyMesh()
        {
            if (m_pDrawMesh)
            {
#if UNITY_EDITOR
                if (Application.isPlaying) Object.Destroy(m_pDrawMesh);
                else Object.DestroyImmediate(m_pDrawMesh);
#else
                Object.Destroy(m_pDrawMesh);
#endif
                m_pDrawMesh = null;
            }
        }
        //--------------------------------------------------------
        public void Destroy()
        {
            m_bDirtyOrder = false;
            if (m_pMaterial)
            {
#if UNITY_EDITOR
                if(Application.isPlaying) Object.Destroy(m_pMaterial);
                else Object.DestroyImmediate(m_pMaterial);
#else
                Object.Destroy(m_pMaterial);
#endif
                m_pMaterial = null;
            }
            DestortyMesh();
            m_pData = null;
            m_SeqFrameJobHandle.Complete();
            if(m_arrNativeDraws.IsCreated)
            {
                m_arrNativeDraws.Dispose();
            }
            if (m_guidToIndex.IsCreated)
                m_guidToIndex.Dispose();
        }
    }
}
