#if UNITY_EDITOR
/********************************************************************
生成日期:	11:06:2023
类    名: 	TimelinePanel
作    者:	HappLI
描    述:	时间刻度轴
*********************************************************************/
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework.ED
{
    public class TimelineTimeArea : TimeArea
    {

        public TimelineTimeArea(bool minimalGUI) : base(minimalGUI)
        {
        }

        public override string FormatTickTime(float time, float frameRate, TimeFormat timeFormat)
        {
            //time = m_State.timeReferenceMode == TimeReferenceMode.Global ?
           //     (float)m_State.editSequence.ToGlobalTime(time) : time;

            return FormatTime(time, frameRate, timeFormat);
        }
    }

    public class TimelinePanel
    {
        public enum EDrawMode
        {
            TimeSecond,
            FrameRate,
        }
        float[] m_StepModulos = new[] { 0.01f, 0.1f, 0.5f, 1, 5, 10, 50, 100, 500, 1000, 5000, 10000, 50000, 100000, 250000, 500000 };
        private static int  ms_nTimelineHash = "timelinecontrol".GetHashCode();

        private Rect        m_timeRect = new Rect(0, 0, 0, 0);
        private float       m_fViewTimeOffset = 0;
        private float       m_fBeginTime = 0;
        private float       m_fEndTime = 0;
        private float m_fTimeStep;
        private float m_fTimeInfoInterval;

        private float       m_FrameRate = 30;
        EDrawMode           m_DrawMode = EDrawMode.FrameRate;
        //------------------------------------------------------
        public void SetDrawMode(EDrawMode mode, float frameRate = 0)
        {
            m_DrawMode = mode;
            m_FrameRate = frameRate;
        }
        //------------------------------------------------------
        public float Draw(Rect rc, float time, float beginTime, float endTime)
        {
            m_timeRect = rc;
            m_fBeginTime = beginTime;
            m_fEndTime = endTime + m_fViewTimeOffset;
            float fTime = DrawTimelinePanel(time);
            Handles.DrawLine(new Vector3(rc.xMin, rc.yMax), new Vector3(rc.xMax, rc.yMax));
            return fTime;
        }
        //-----------------------------------------------------
        public float GetViewBeginTime()
        {
            return m_fBeginTime;
        }
        //-----------------------------------------------------
        public float GetViewEndTime()
        {
            return m_fEndTime + m_fViewTimeOffset;
        }
        //-----------------------------------------------------
        public float GetViewMaxTime()
        {
            return GetViewEndTime() - GetViewBeginTime();
        }
        //-----------------------------------------------------
        private float DrawTimelinePanel(float time)
        {
            Rect position = m_timeRect;
            m_fTimeStep = 0.01f;
            m_fTimeInfoInterval = 1000000f;
            for (var i = 0; i < m_StepModulos.Length; i++)
            {
                var count = GetViewMaxTime() / m_StepModulos[i];
                if (position.width / count > 50)
                {
                    m_fTimeInfoInterval = m_StepModulos[i];
                    m_fTimeStep = i > 0 ? m_StepModulos[i - 1] : m_fTimeStep;
                    break;
                }
            }

            int controlID = GUIUtility.GetControlID(ms_nTimelineHash, FocusType.Passive, position);
            Rect rect2 = new Rect((position.x + (position.width * time/ GetViewMaxTime())) - 5f, position.y + 2f, 10f, 20f);
            Event current = Event.current;
            EventType type = current.type;
            switch (type)
            {
                case EventType.MouseDown:
                    if (rect2.Contains(current.mousePosition))
                    {
                        GUIUtility.hotControl = controlID;
                        current.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                        current.Use();
                    }
                    break;
                case EventType.ScrollWheel:
                    {
                        var delta = current.delta.y;
                        if (delta > 0)
                        {
                            delta = 3;
                        }
                        else if (delta < 0)
                        {
                            delta = -3;
                        }

                        var t = Mathf.Abs(delta * 25) / m_timeRect.width * GetViewMaxTime();

                        var maxAdd = delta > 0 ? t : -t;

                        if (maxAdd > 0 && GetViewMaxTime() > 240)
                        {
                            Debug.Log("Exceed maximum range!");
                        }
                        else
                            m_fViewTimeOffset += maxAdd;
                    }
                    break;
                case EventType.MouseMove:
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        float introduced10 = Mathf.Clamp(current.mousePosition.x, position.x, position.x + position.width);
                        time = (introduced10 - position.x) / position.width* GetViewMaxTime();
                        current.Use();
                    }
                    break;

                default:
                    if (type == EventType.Repaint)
                    {
                        Rect rect = new Rect(position.x, position.y + position.height/2, position.width, position.height/2);
                        this.DrawTimeLine(rect);
                        Color backupColor = GUI.backgroundColor;
                        GUI.backgroundColor = Color.red;
                        GUI.skin.horizontalSliderThumb.Draw(rect2, new GUIContent(), controlID);
                        GUI.backgroundColor = backupColor;
                    }
                    break;
            }

            return time;
        }
        //-----------------------------------------------------
        private void DrawTimeLine(Rect rect)
        {
            if (Event.current.type == EventType.Repaint)
            {
                float timeInfoStart = Mathf.FloorToInt(GetViewBeginTime() / m_fTimeInfoInterval) * m_fTimeInfoInterval;
                float timeInfoEnd = Mathf.CeilToInt(GetViewEndTime() / m_fTimeInfoInterval) * m_fTimeInfoInterval;
                //时间步长间隔 time step interval
                if (rect.width / (GetViewMaxTime() / m_fTimeStep) > 6)
                {
                    for (var i = timeInfoStart; i <= timeInfoEnd; i += m_fTimeStep)
                    {
                        var posX = (i- GetViewBeginTime()) / GetViewMaxTime() * rect.width;
                        if (posX > rect.width) continue;
                        GUI.DrawTexture(new Rect(posX + rect.xMin, rect.yMax-6, 1, 6), EditorGUIUtility.whiteTexture);
                    }
                }

                //时间文字间隔 time text interval
                for (var i = timeInfoStart; i <= timeInfoEnd; i += m_fTimeInfoInterval)
                {
                    var posX = (i - GetViewBeginTime()) / GetViewMaxTime() * rect.width;
                    var rounded = Mathf.Round(i * 10) / 10;
                    if (posX > rect.width) continue;
                    GUI.DrawTexture(new Rect(posX + rect.xMin, rect.yMax-12, 1, 12), EditorGUIUtility.whiteTexture);

                    string text = "";
                    if(m_DrawMode == EDrawMode.TimeSecond)
                    {
                        text = rounded.ToString("0.00");
                    }
                    else
                    {
                        text =((int)(m_FrameRate * (i - GetViewBeginTime()))).ToString();
                    }
                    var size = GUI.skin.label.CalcSize(new GUIContent(text));
                    var stampRect = new Rect(posX + rect.xMin, rect.yMax - 10- size.y, size.x, size.y);
                    GUI.Box(stampRect, text, GUI.skin.label);
                }
            }
        }
    }
}
#endif