/********************************************************************
生成日期:		11:06:2020
类    名: 	EditorTimer
作    者:	HappLI
描    述:	编辑器下的时间帧计算
*********************************************************************/
#if UNITY_EDITOR
using UnityEngine;

namespace Framework.ED
{
    public class EditorTimer
    {
        public float m_PreviousTime;
        public float unScaleDeltaTime = 0.02f;
        public float fixedDeltaTime = 0.02f;
        public float deltaTime = 0f;
        public float m_currentSnap = 1f;

        //-----------------------------------------------------
        public void Update()
        {
            //if (Application.isPlaying)
            //{
            //    unScaleDeltaTime = Time.fixedDeltaTime;
            //    deltaTime = (float)(unScaleDeltaTime * m_currentSnap);
            //}
            //else
            {
                float curTime = Time.realtimeSinceStartup;
                m_PreviousTime = Mathf.Min(m_PreviousTime, curTime);//very important!!!

                unScaleDeltaTime = curTime - m_PreviousTime;
                deltaTime = (float)(unScaleDeltaTime * m_currentSnap);
            }

            m_PreviousTime = Time.realtimeSinceStartup;
        }
    }
}
#endif