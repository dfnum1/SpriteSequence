#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Framework.ED
{
    public class TimeArea : ZoomableArea
    {
        public enum TimeFormat
        {
            None,
            Seconds,
            Frames
        }

        private class Styles2
        {
            public GUIStyle timelineTick = "AnimationTimelineTick";

            public GUIStyle playhead = "AnimationPlayHead";
        }

        public enum TimeRulerDragMode
        {
            None,
            Start,
            End,
            Dragging,
            Cancel
        }

        [SerializeField]
        private TickHandler m_HTicks;

        [SerializeField]
        private TickHandler m_VTicks;

        private List<float> m_TickCache = new List<float>(1000);

        internal const int kTickRulerDistMin = 3;

        internal const int kTickRulerDistFull = 80;

        internal const int kTickRulerDistLabel = 40;

        internal const float kTickRulerHeightMax = 0.7f;

        internal const float kTickRulerFatThreshold = 0.5f;

        private static Styles2 timeAreaStyles;

        private static float s_OriginalTime;

        private static float s_PickOffset;

        public TickHandler hTicks
        {
            get
            {
                return m_HTicks;
            }
            set
            {
                m_HTicks = value;
            }
        }

        public TickHandler vTicks
        {
            get
            {
                return m_VTicks;
            }
            set
            {
                m_VTicks = value;
            }
        }

        private static void InitStyles()
        {
            if (timeAreaStyles == null)
            {
                timeAreaStyles = new Styles2();
            }
        }

        public TimeArea(bool minimalGUI)
            : this(minimalGUI, enableSliderZoomHorizontal: true, enableSliderZoomVertical: true)
        {
        }

        public TimeArea(bool minimalGUI, bool enableSliderZoom)
            : this(minimalGUI, enableSliderZoom, enableSliderZoom)
        {
        }

        public TimeArea(bool minimalGUI, bool enableSliderZoomHorizontal, bool enableSliderZoomVertical)
            : base(minimalGUI, enableSliderZoomHorizontal, enableSliderZoomVertical)
        {
            float[] tickModulos = new float[29]
            {
                1E-07f, 5E-07f, 1E-06f, 5E-06f, 1E-05f, 5E-05f, 0.0001f, 0.0005f, 0.001f, 0.005f,
                0.01f, 0.05f, 0.1f, 0.5f, 1f, 5f, 10f, 50f, 100f, 500f,
                1000f, 5000f, 10000f, 50000f, 100000f, 500000f, 1000000f, 5000000f, 1E+07f
            };
            hTicks = new TickHandler();
            hTicks.SetTickModulos(tickModulos);
            vTicks = new TickHandler();
            vTicks.SetTickModulos(tickModulos);
        }

        public void SetTickMarkerRanges()
        {
            hTicks.SetRanges(base.shownArea.xMin, base.shownArea.xMax, base.drawRect.xMin, base.drawRect.xMax);
            vTicks.SetRanges(base.shownArea.yMin, base.shownArea.yMax, base.drawRect.yMin, base.drawRect.yMax);
        }

        public void DrawMajorTicks(Rect position, float frameRate)
        {
            GUI.BeginGroup(position);
            if (Event.current.type != EventType.Repaint)
            {
                GUI.EndGroup();
                return;
            }

            InitStyles();
            UIDrawUtils.ApplyWireMaterial();
            SetTickMarkerRanges();
            hTicks.SetTickStrengths(3f, 80f, sqrt: true);
            Color textColor = timeAreaStyles.timelineTick.normal.textColor;
            textColor.a = 0.1f;
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                GL.Begin(7);
            }
            else
            {
                GL.Begin(1);
            }

            Rect theShownArea = base.shownArea;
            for (int i = 0; i < hTicks.tickLevels; i++)
            {
                float num = hTicks.GetStrengthOfLevel(i) * 0.9f;
                if (!(num > 0.5f))
                {
                    continue;
                }

                m_TickCache.Clear();
                hTicks.GetTicksAtLevel(i, excludeTicksFromHigherlevels: true, m_TickCache);
                for (int j = 0; j < m_TickCache.Count; j++)
                {
                    if (!(m_TickCache[j] < 0f))
                    {
                        int num2 = Mathf.RoundToInt(m_TickCache[j] * frameRate);
                        float x = FrameToPixel(num2, frameRate, position, theShownArea);
                        DrawVerticalLineFast(x, 0f, position.height, textColor);
                    }
                }
            }

            GL.End();
            GUI.EndGroup();
        }

        public void TimeRuler(Rect position, float frameRate)
        {
            TimeRuler(position, frameRate, labels: true, useEntireHeight: false, 1f, TimeFormat.Seconds);
        }

        public void TimeRuler(Rect position, float frameRate, bool labels, bool useEntireHeight, float alpha, TimeFormat timeFormat)
        {
            Color color = GUI.color;
            GUI.BeginGroup(position);
            InitStyles();
            UIDrawUtils.ApplyWireMaterial();
            Color backgroundColor = GUI.backgroundColor;
            SetTickMarkerRanges();
            hTicks.SetTickStrengths(3f, 80f, sqrt: true);
            Color textColor = timeAreaStyles.timelineTick.normal.textColor;
            textColor.a = 0.75f * alpha;
            if (Event.current.type == EventType.Repaint)
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    GL.Begin(7);
                }
                else
                {
                    GL.Begin(1);
                }

                Rect theShownArea = base.shownArea;
                for (int i = 0; i < hTicks.tickLevels; i++)
                {
                    float num = hTicks.GetStrengthOfLevel(i) * 0.9f;
                    m_TickCache.Clear();
                    hTicks.GetTicksAtLevel(i, excludeTicksFromHigherlevels: true, m_TickCache);
                    for (int j = 0; j < m_TickCache.Count; j++)
                    {
                        if (!(m_TickCache[j] < base.hRangeMin) && !(m_TickCache[j] > base.hRangeMax))
                        {
                            int num2 = Mathf.RoundToInt(m_TickCache[j] * frameRate);
                            float num3 = (useEntireHeight ? position.height : (position.height * Mathf.Min(1f, num) * 0.7f));
                            float x = FrameToPixel(num2, frameRate, position, theShownArea);
                            DrawVerticalLineFast(x, position.height - num3 + 0.5f, position.height - 0.5f, new Color(1f, 1f, 1f, num / 0.5f) * textColor);
                        }
                    }
                }

                GL.End();
            }

            if (labels)
            {
                int levelWithMinSeparation = hTicks.GetLevelWithMinSeparation(40f);
                m_TickCache.Clear();
                hTicks.GetTicksAtLevel(levelWithMinSeparation, excludeTicksFromHigherlevels: false, m_TickCache);
                for (int k = 0; k < m_TickCache.Count; k++)
                {
                    if (!(m_TickCache[k] < base.hRangeMin) && !(m_TickCache[k] > base.hRangeMax))
                    {
                        int num4 = Mathf.RoundToInt(m_TickCache[k] * frameRate);
                        float num5 = Mathf.Floor(FrameToPixel(num4, frameRate, position));
                        string text = FormatTickTime(m_TickCache[k], frameRate, timeFormat);
                        GUI.Label(new Rect(num5 + 3f, -1f, 40f, 20f), text, timeAreaStyles.timelineTick);
                    }
                }
            }

            GUI.EndGroup();
            GUI.backgroundColor = backgroundColor;
            GUI.color = color;
        }

        public static void DrawPlayhead(float x, float yMin, float yMax, float thickness, float alpha)
        {
            if (Event.current.type == EventType.Repaint)
            {
                InitStyles();
                float num = thickness * 0.5f;
                Color color = timeAreaStyles.playhead.normal.textColor*alpha;
                if (thickness > 1f)
                {
                    Rect rect = Rect.MinMaxRect(x - num, yMin, x + num, yMax);
                    EditorGUI.DrawRect(rect, color);
                }
                else
                {
                    DrawVerticalLine(x, yMin, yMax, color);
                }
            }
        }

        public static void DrawVerticalLine(float x, float minY, float maxY, Color color)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Color color2 = Handles.color;
                UIDrawUtils.ApplyWireMaterial();
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    GL.Begin(7);
                }
                else
                {
                    GL.Begin(1);
                }

                DrawVerticalLineFast(x, minY, maxY, color);
                GL.End();
                Handles.color = color2;
            }
        }

        public static void DrawVerticalLineFast(float x, float minY, float maxY, Color color)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                GL.Color(color);
                GL.Vertex(new Vector3(x - 0.5f, minY, 0f));
                GL.Vertex(new Vector3(x + 0.5f, minY, 0f));
                GL.Vertex(new Vector3(x + 0.5f, maxY, 0f));
                GL.Vertex(new Vector3(x - 0.5f, maxY, 0f));
            }
            else
            {
                GL.Color(color);
                GL.Vertex(new Vector3(x, minY, 0f));
                GL.Vertex(new Vector3(x, maxY, 0f));
            }
        }

        public TimeRulerDragMode BrowseRuler(Rect position, ref float time, float frameRate, bool pickAnywhere, GUIStyle thumbStyle)
        {
            int controlID = GUIUtility.GetControlID(3126789, FocusType.Passive);
            return BrowseRuler(position, controlID, ref time, frameRate, pickAnywhere, thumbStyle);
        }

        public TimeRulerDragMode BrowseRuler(Rect position, int id, ref float time, float frameRate, bool pickAnywhere, GUIStyle thumbStyle)
        {
            Event current = Event.current;
            Rect position2 = position;
            if (time != -1f)
            {
                position2.x = Mathf.Round(TimeToPixel(time, position)) - (float)thumbStyle.overflow.left;
                position2.width = thumbStyle.fixedWidth + (float)thumbStyle.overflow.horizontal;
            }

            switch (current.GetTypeForControl(id))
            {
                case EventType.Repaint:
                    if (time != -1f)
                    {
                        bool flag = position.Contains(current.mousePosition);
                        position2.x += thumbStyle.overflow.left;
                        thumbStyle.Draw(position2, id == GUIUtility.hotControl, flag || id == GUIUtility.hotControl, on: false, hasKeyboardFocus: false);
                    }

                    break;
                case EventType.MouseDown:
                    if (position2.Contains(current.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        s_PickOffset = current.mousePosition.x - TimeToPixel(time, position);
                        current.Use();
                        return TimeRulerDragMode.Start;
                    }

                    if (pickAnywhere && position.Contains(current.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        float num2 = SnapTimeToWholeFPS(PixelToTime(current.mousePosition.x, position), frameRate);
                        s_OriginalTime = time;
                        if (num2 != time)
                        {
                            GUI.changed = true;
                        }

                        time = num2;
                        s_PickOffset = 0f;
                        current.Use();
                        return TimeRulerDragMode.Start;
                    }

                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                    {
                        float num = SnapTimeToWholeFPS(PixelToTime(current.mousePosition.x - s_PickOffset, position), frameRate);
                        if (num != time)
                        {
                            GUI.changed = true;
                        }

                        time = num;
                        current.Use();
                        return TimeRulerDragMode.Dragging;
                    }

                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        current.Use();
                        return TimeRulerDragMode.End;
                    }

                    break;
                case EventType.KeyDown:
                    if (GUIUtility.hotControl == id && current.keyCode == KeyCode.Escape)
                    {
                        if (time != s_OriginalTime)
                        {
                            GUI.changed = true;
                        }

                        time = s_OriginalTime;
                        GUIUtility.hotControl = 0;
                        current.Use();
                        return TimeRulerDragMode.Cancel;
                    }

                    break;
            }

            return TimeRulerDragMode.None;
        }

        private float FrameToPixel(float i, float frameRate, Rect rect, Rect theShownArea)
        {
            return (i - theShownArea.xMin * frameRate) * rect.width / (theShownArea.width * frameRate);
        }

        public float FrameToPixel(float i, float frameRate, Rect rect)
        {
            return FrameToPixel(i, frameRate, rect, base.shownArea);
        }

        public float TimeField(Rect rect, int id, float time, float frameRate, TimeFormat timeFormat)
        {
            switch (timeFormat)
            {
                case TimeFormat.None:
                    {
                       // float time2 = EditorGUI.FloatField(rect, new Rect(0f, 0f, 0f, 0f), id, time, EditorGUI.kFloatFieldFormatString, EditorStyles.numberField, draggable: false);
                        float time2 = EditorGUI.FloatField(rect, time, EditorStyles.numberField);
                        return SnapTimeToWholeFPS(time2, frameRate);
                    }
                case TimeFormat.Frames:
                    {
                        int value = Mathf.RoundToInt(time * frameRate);
                     //   int num2 = EditorGUI.DoIntField(EditorGUI.s_RecycledEditor, rect, new Rect(0f, 0f, 0f, 0f), id, value, EditorGUI.kIntFieldFormatString, EditorStyles.numberField, draggable: false, 0f);
                        int num2 = EditorGUI.IntField(rect, value, EditorStyles.numberField);
                        return (float)num2 / frameRate;
                    }
                default:
                    {
                        string text = FormatTime(time, frameRate, TimeFormat.Seconds);
                        //string allowedletters = "0123456789.,:";
                        // text = EditorGUI.DoTextField(EditorGUI.s_RecycledEditor, id, rect, text, EditorStyles.numberField, allowedletters, out var changed, reset: false, multiline: false, passwordField: false);
                        text = EditorGUI.TextField(rect, text, EditorStyles.numberField);
                        bool changed = false;
                        if (changed && GUIUtility.keyboardControl == id)
                        {
                            GUI.changed = true;
                            text = text.Replace(',', '.');
                            int num = text.IndexOf(':');
                            float result3;
                            if (num >= 0)
                            {
                                string s = text.Substring(0, num);
                                string s2 = text.Substring(num + 1);
                                if (int.TryParse(s, out var result) && int.TryParse(s2, out var result2))
                                {
                                    return (float)result + (float)result2 / frameRate;
                                }
                            }
                            else if (float.TryParse(text, out result3))
                            {
                                return SnapTimeToWholeFPS(result3, frameRate);
                            }
                        }

                        return time;
                    }
            }
        }

        public float ValueField(Rect rect, int id, float value)
        {
          //  return EditorGUI.DoFloatField(EditorGUI.s_RecycledEditor, rect, new Rect(0f, 0f, 0f, 0f), id, value, EditorGUI.kFloatFieldFormatString, EditorStyles.numberField, draggable: false);
          return EditorGUI.FloatField(rect, value, EditorStyles.numberField);
        }

        public string FormatTime(float time, float frameRate, TimeFormat timeFormat)
        {
            if (timeFormat == TimeFormat.None)
            {
                return time.ToString("N" + ((frameRate == 0f) ? GetNumberOfDecimalsForMinimumDifference(base.shownArea.width / base.drawRect.width) : GetNumberOfDecimalsForMinimumDifference(1f / frameRate)), CultureInfo.InvariantCulture.NumberFormat);
            }

            int num = Mathf.RoundToInt(time * frameRate);
            if (timeFormat == TimeFormat.Seconds)
            {
                int totalWidth = ((frameRate == 0f) ? 1 : ((int)frameRate - 1).ToString().Length);
                string text = string.Empty;
                if (num < 0)
                {
                    text = "-";
                    num = -num;
                }

                return text + num / (int)frameRate + ":" + ((float)num % frameRate).ToString().PadLeft(totalWidth, '0');
            }

            return num.ToString();
        }

        public virtual string FormatTickTime(float time, float frameRate, TimeFormat timeFormat)
        {
            return FormatTime(time, frameRate, timeFormat);
        }

        public string FormatValue(float value)
        {
            return value.ToString("N" + GetNumberOfDecimalsForMinimumDifference(base.shownArea.height / base.drawRect.height), CultureInfo.InvariantCulture.NumberFormat);
        }


        internal static int GetNumberOfDecimalsForMinimumDifference(float minDifference)
        {
            return Mathf.Clamp(-Mathf.FloorToInt(Mathf.Log10(Mathf.Abs(minDifference))), 0, 15);
        }

        public float SnapTimeToWholeFPS(float time, float frameRate)
        {
            if (frameRate == 0f)
            {
                return time;
            }

            return Mathf.Round(time * frameRate) / frameRate;
        }

        public void DrawTimeOnSlider(float time, Color c, float maxTime, float leftSidePadding = 0f, float rightSidePadding = 0f)
        {
            if (base.hSlider)
            {
                if (base.styles.horizontalScrollbar == null)
                {
                    base.styles.InitGUIStyles(minimalGUI: false, base.allowSliderZoomHorizontal, base.allowSliderZoomVertical);
                }

                float num = TimeToPixel(0f, base.rect);
                float num2 = TimeToPixel(maxTime, base.rect);
                float num3 = TimeToPixel(base.shownAreaInsideMargins.xMin, base.rect) + base.styles.horizontalScrollbarLeftButton.fixedWidth + leftSidePadding;
                float num4 = TimeToPixel(base.shownAreaInsideMargins.xMax, base.rect) - (base.styles.horizontalScrollbarRightButton.fixedWidth + rightSidePadding);
                float num5 = (TimeToPixel(time, base.rect) - num) * (num4 - num3) / (num2 - num) + num3;
                if (!(num5 > base.rect.xMax - (base.styles.horizontalScrollbarLeftButton.fixedWidth + leftSidePadding + 3f)))
                {
                    float num6 = base.styles.sliderWidth - base.styles.visualSliderWidth;
                    float num7 = ((base.vSlider && base.hSlider) ? num6 : 0f);
                    Rect rect = new Rect(base.drawRect.x + 1f, base.drawRect.yMax - num6, base.drawRect.width - num7, base.styles.sliderWidth);
                    Vector2 vector = new Vector2(num5, rect.yMin);
                    Vector2 vector2 = new Vector2(num5, rect.yMax);
                    Rect rect2 = Rect.MinMaxRect(vector.x - 0.5f, vector.y, vector2.x + 0.5f, vector2.y);
                    EditorGUI.DrawRect(rect2, c);
                }
            }
        }
    }
}
#endif