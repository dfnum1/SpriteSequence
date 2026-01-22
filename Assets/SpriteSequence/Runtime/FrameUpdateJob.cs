/********************************************************************
生成日期:	01:20:2026
类    名: 	FrameUpdateJob
作    者:	HappLI
描    述:	Sprite的序列帧推帧、裁剪
*********************************************************************/
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
namespace Framework.SpriteSeq
{
    [BurstCompile]
    struct FrameUpdateJob : IJob
    {
        public NativeArray<FrameSequenceRenderer.DrawData> drawDatas;
        public float dt;
        public float fps;
        public int drawCount;
        public bool cullingCheck;
        public float4x4 cullingMatrix;
        //--------------------------------------------------------
        public void Execute()
        {
            for (int i = 0; i < drawDatas.Length && i < drawCount; ++i)
            {
                FrameSequenceRenderer.DrawData batch = drawDatas[i];

                if (cullingCheck)
                {
                    Vector3 pos = batch.matrix.GetColumn(3);
                    pos.x += batch.centerOffset.x;
                    pos.y += batch.centerOffset.y;
                    batch.isCulled = !InView(cullingMatrix, pos.x, pos.y);
                    if (batch.cullingSize > 0 && batch.isCulled)
                    {
                        if (InView(cullingMatrix, pos.x - batch.cullingSize, pos.y - batch.cullingSize)) batch.isCulled = false;
                        else if (InView(cullingMatrix, pos.x + batch.cullingSize, pos.y - batch.cullingSize)) batch.isCulled = false;
                        else if (InView(cullingMatrix, pos.x + batch.cullingSize, pos.y + batch.cullingSize)) batch.isCulled = false;
                        else if (InView(cullingMatrix, pos.x - batch.cullingSize, pos.y + batch.cullingSize)) batch.isCulled = false;
                    }
                }
                else batch.isCulled = false;
                //if (batch.isCulled)
                //    continue;

                if(batch.playing)
                    batch.frame += dt * fps;
                if (batch.frame >= batch.sequenceEnd)
                    batch.frame = batch.sequenceBegin;
                drawDatas[i] = batch;
            }
        }
        //--------------------------------------------------------
        public unsafe bool InView(float4x4 mvp, float posx, float posy, float fFactor = 1.1f)
        {
            float4 mvpPos = math.mul(mvp, new float4(posx, posy, 0, 1));
            float3 view = mvpPos.xyz / mvpPos.w;
            bool inview = (view.x >= -fFactor && view.x <= fFactor)
                          && (view.y >= -fFactor && view.y <= fFactor)
                          && (view.z >= -fFactor && view.z <= fFactor);
            return inview;
        }
    }
}
