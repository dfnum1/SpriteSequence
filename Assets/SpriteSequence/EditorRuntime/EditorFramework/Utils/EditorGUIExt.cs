#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Framework.ED
{
    internal enum HighLevelEvent
    {
        None,
        Click,
        DoubleClick,
        ContextClick,
        BeginDrag,
        Drag,
        EndDrag,
        Delete,
        SelectionChanged,
        Copy,
        Paste
    }
    internal class EditorGUIExt
    {
        private class Styles
        {
            public GUIStyle selectionRect = "SelectionRect";
        }

        private class MinMaxSliderState
        {
            public float dragStartPos = 0f;

            public float dragStartValue = 0f;

            public float dragStartSize = 0f;

            public float dragStartValuesPerPixel = 0f;

            public float dragStartLimit = 0f;

            public float dragEndLimit = 0f;

            public int whereWeDrag = -1;
        }

        private enum DragSelectionState
        {
            None,
            DragSelecting,
            Dragging
        }

        private static Styles ms_Styles = new Styles();

        private static int repeatButtonHash = "repeatButton".GetHashCode();

        private static float nextScrollStepTime = 0f;

        private static int firstScrollWait = 250;

        private static int scrollWait = 30;

        private static int scrollControlID;

        private static MinMaxSliderState s_MinMaxSliderState;

        private static int kFirstScrollWait = 250;

        private static int kScrollWait = 30;

        private static DateTime s_NextScrollStepTime = DateTime.Now;

        private static Vector2 s_MouseDownPos = Vector2.zero;

        private static DragSelectionState s_MultiSelectDragSelection = DragSelectionState.None;

        private static Vector2 s_StartSelectPos = Vector2.zero;

        private static List<bool> s_SelectionBackup = null;

        private static List<bool> s_LastFrameSelections = null;

        internal static int s_MinMaxSliderHash = "MinMaxSlider".GetHashCode();

        private static bool adding = false;

        private static bool[] initSelections;

        private static int initIndex = 0;

        private static bool DoRepeatButton(Rect position, GUIContent content, GUIStyle style, FocusType focusType)
        {
            int controlID = GUIUtility.GetControlID(repeatButtonHash, focusType, position);
            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (position.Contains(Event.current.mousePosition))
                    {
                        GUIUtility.hotControl = controlID;
                        Event.current.Use();
                    }

                    return false;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                        return position.Contains(Event.current.mousePosition);
                    }

                    return false;
                case EventType.Repaint:
                    style.Draw(position, content, controlID, on: false, position.Contains(Event.current.mousePosition));
                    return controlID == GUIUtility.hotControl && position.Contains(Event.current.mousePosition);
                default:
                    return false;
            }
        }

        private static bool ScrollerRepeatButton(int scrollerID, Rect rect, GUIStyle style)
        {
            bool result = false;
            if (DoRepeatButton(rect, GUIContent.none, style, FocusType.Passive))
            {
                bool flag = scrollControlID != scrollerID;
                scrollControlID = scrollerID;
                if (flag)
                {
                    result = true;
                    nextScrollStepTime = Time.realtimeSinceStartup + 0.001f * (float)firstScrollWait;
                }
                else if (Time.realtimeSinceStartup >= nextScrollStepTime)
                {
                    result = true;
                    nextScrollStepTime = Time.realtimeSinceStartup + 0.001f * (float)scrollWait;
                }

                if (Event.current.type == EventType.Repaint)
                {
                    HandleUtility.Repaint();
                }
            }

            return result;
        }

        public static void MinMaxScroller(Rect position, int id, ref float value, ref float size, float visualStart, float visualEnd, float startLimit, float endLimit, GUIStyle slider, GUIStyle thumb, GUIStyle leftButton, GUIStyle rightButton, bool horiz)
        {
            float num = ((!horiz) ? (size * 10f / position.height) : (size * 10f / position.width));
            Rect position2;
            Rect rect;
            Rect rect2;
            if (horiz)
            {
                position2 = new Rect(position.x + leftButton.fixedWidth, position.y, position.width - leftButton.fixedWidth - rightButton.fixedWidth, position.height);
                rect = new Rect(position.x, position.y, leftButton.fixedWidth, position.height);
                rect2 = new Rect(position.xMax - rightButton.fixedWidth, position.y, rightButton.fixedWidth, position.height);
            }
            else
            {
                position2 = new Rect(position.x, position.y + leftButton.fixedHeight, position.width, position.height - leftButton.fixedHeight - rightButton.fixedHeight);
                rect = new Rect(position.x, position.y, position.width, leftButton.fixedHeight);
                rect2 = new Rect(position.x, position.yMax - rightButton.fixedHeight, position.width, rightButton.fixedHeight);
            }

            float num2 = Mathf.Min(visualStart, value);
            float num3 = Mathf.Max(visualEnd, value + size);
            MinMaxSlider(position2, ref value, ref size, num2, num3, num2, num3, slider, thumb, horiz);
            if (ScrollerRepeatButton(id, rect, leftButton))
            {
                value -= num * ((visualStart < visualEnd) ? 1f : (-1f));
            }

            if (ScrollerRepeatButton(id, rect2, rightButton))
            {
                value += num * ((visualStart < visualEnd) ? 1f : (-1f));
            }

            if (Event.current.type == EventType.MouseUp && Event.current.type == EventType.Used)
            {
                scrollControlID = 0;
            }

            if (startLimit < endLimit)
            {
                value = Mathf.Clamp(value, startLimit, endLimit - size);
            }
            else
            {
                value = Mathf.Clamp(value, endLimit, startLimit - size);
            }
        }

        public static void MinMaxSlider(Rect position, ref float value, ref float size, float visualStart, float visualEnd, float startLimit, float endLimit, GUIStyle slider, GUIStyle thumb, bool horiz)
        {
            DoMinMaxSlider(position, GUIUtility.GetControlID(s_MinMaxSliderHash, FocusType.Passive), ref value, ref size, visualStart, visualEnd, startLimit, endLimit, slider, thumb, horiz);
        }

        private static float ThumbSize(bool horiz, GUIStyle thumb)
        {
            if (horiz)
            {
                return (thumb.fixedWidth != 0f) ? thumb.fixedWidth : ((float)thumb.padding.horizontal);
            }

            return (thumb.fixedHeight != 0f) ? thumb.fixedHeight : ((float)thumb.padding.vertical);
        }

        internal static void DoMinMaxSlider(Rect position, int id, ref float value, ref float size, float visualStart, float visualEnd, float startLimit, float endLimit, GUIStyle slider, GUIStyle thumb, bool horiz)
        {
            Event current = Event.current;
            bool flag = size == 0f;
            float num = Mathf.Min(visualStart, visualEnd);
            float num2 = Mathf.Max(visualStart, visualEnd);
            float num3 = Mathf.Min(startLimit, endLimit);
            float num4 = Mathf.Max(startLimit, endLimit);
            MinMaxSliderState minMaxSliderState = s_MinMaxSliderState;
            if (GUIUtility.hotControl == id && minMaxSliderState != null)
            {
                num = minMaxSliderState.dragStartLimit;
                num3 = minMaxSliderState.dragStartLimit;
                num2 = minMaxSliderState.dragEndLimit;
                num4 = minMaxSliderState.dragEndLimit;
            }

            float num5 = 0f;
            float num6 = Mathf.Clamp(value, num, num2);
            float num7 = Mathf.Clamp(value + size, num, num2) - num6;
            float num8 = ((!(visualStart > visualEnd)) ? 1 : (-1));
            if (slider == null || thumb == null)
            {
                return;
            }

            Rect rect = thumb.margin.Remove(slider.padding.Remove(position));
            float num9 = ThumbSize(horiz, thumb);
            float num10;
            Rect position2;
            Rect position3;
            Rect position4;
            float num11;
            if (horiz)
            {
                float height = ((thumb.fixedHeight != 0f) ? thumb.fixedHeight : rect.height);
                num10 = (position.width - (float)slider.padding.horizontal - num9) / (num2 - num);
                position2 = new Rect((num6 - num) * num10 + rect.x, rect.y, num7 * num10 + num9, height);
                position3 = new Rect(position2.x, position2.y, thumb.padding.left, position2.height);
                position4 = new Rect(position2.xMax - (float)thumb.padding.right, position2.y, thumb.padding.right, position2.height);
                num11 = current.mousePosition.x - position.x;
            }
            else
            {
                float width = ((thumb.fixedWidth != 0f) ? thumb.fixedWidth : rect.width);
                num10 = (position.height - (float)slider.padding.vertical - num9) / (num2 - num);
                position2 = new Rect(rect.x, (num6 - num) * num10 + rect.y, width, num7 * num10 + num9);
                position3 = new Rect(position2.x, position2.y, position2.width, thumb.padding.top);
                position4 = new Rect(position2.x, position2.yMax - (float)thumb.padding.bottom, position2.width, thumb.padding.bottom);
                num11 = current.mousePosition.y - position.y;
            }

            switch (current.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (current.button != 0 || !position.Contains(current.mousePosition) || num - num2 == 0f)
                    {
                        break;
                    }

                    if (minMaxSliderState == null)
                    {
                        minMaxSliderState = (s_MinMaxSliderState = new MinMaxSliderState());
                    }

                    minMaxSliderState.dragStartLimit = startLimit;
                    minMaxSliderState.dragEndLimit = endLimit;
                    if (position2.Contains(current.mousePosition))
                    {
                        minMaxSliderState.dragStartPos = num11;
                        minMaxSliderState.dragStartValue = value;
                        minMaxSliderState.dragStartSize = size;
                        minMaxSliderState.dragStartValuesPerPixel = num10;
                        if (position3.Contains(current.mousePosition))
                        {
                            minMaxSliderState.whereWeDrag = 1;
                        }
                        else if (position4.Contains(current.mousePosition))
                        {
                            minMaxSliderState.whereWeDrag = 2;
                        }
                        else
                        {
                            minMaxSliderState.whereWeDrag = 0;
                        }

                        GUIUtility.hotControl = id;
                        current.Use();
                    }
                    else
                    {
                        if (slider == GUIStyle.none)
                        {
                            break;
                        }

                        if (size != 0f && flag)
                        {
                            if (horiz)
                            {
                                if (num11 > position2.xMax - position.x)
                                {
                                    value += size * num8 * 0.9f;
                                }
                                else
                                {
                                    value -= size * num8 * 0.9f;
                                }
                            }
                            else if (num11 > position2.yMax - position.y)
                            {
                                value += size * num8 * 0.9f;
                            }
                            else
                            {
                                value -= size * num8 * 0.9f;
                            }

                            minMaxSliderState.whereWeDrag = 0;
                            GUI.changed = true;
                            s_NextScrollStepTime = DateTime.Now.AddMilliseconds(kFirstScrollWait);
                            float num12 = (horiz ? current.mousePosition.x : current.mousePosition.y);
                            float num13 = (horiz ? position2.x : position2.y);
                            minMaxSliderState.whereWeDrag = ((num12 > num13) ? 4 : 3);
                        }
                        else
                        {
                            if (horiz)
                            {
                                value = (num11 - position2.width * 0.5f) / num10 + num - size * 0.5f;
                            }
                            else
                            {
                                value = (num11 - position2.height * 0.5f) / num10 + num - size * 0.5f;
                            }

                            minMaxSliderState.dragStartPos = num11;
                            minMaxSliderState.dragStartValue = value;
                            minMaxSliderState.dragStartSize = size;
                            minMaxSliderState.dragStartValuesPerPixel = num10;
                            minMaxSliderState.whereWeDrag = 0;
                            GUI.changed = true;
                        }

                        GUIUtility.hotControl = id;
                        value = Mathf.Clamp(value, num3, num4 - size);
                        current.Use();
                    }

                    break;
                case EventType.MouseDrag:
                    {
                        if (GUIUtility.hotControl != id)
                        {
                            break;
                        }

                        float num15 = (num11 - minMaxSliderState.dragStartPos) / minMaxSliderState.dragStartValuesPerPixel;
                        switch (minMaxSliderState.whereWeDrag)
                        {
                            case 0:
                                value = Mathf.Clamp(minMaxSliderState.dragStartValue + num15, num3, num4 - size);
                                break;
                            case 1:
                                value = minMaxSliderState.dragStartValue + num15;
                                size = minMaxSliderState.dragStartSize - num15;
                                if (value < num3)
                                {
                                    size -= num3 - value;
                                    value = num3;
                                }

                                if (size < num5)
                                {
                                    value -= num5 - size;
                                    size = num5;
                                }

                                break;
                            case 2:
                                size = minMaxSliderState.dragStartSize + num15;
                                if (value + size > num4)
                                {
                                    size = num4 - value;
                                }

                                if (size < num5)
                                {
                                    size = num5;
                                }

                                break;
                        }

                        GUI.changed = true;
                        current.Use();
                        break;
                    }
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        current.Use();
                        GUIUtility.hotControl = 0;
                    }

                    break;
                case EventType.Repaint:
                    slider.Draw(position, GUIContent.none, id);
                    thumb.Draw(position2, GUIContent.none, id);
                    EditorGUIUtility.AddCursorRect(position3, horiz ? MouseCursor.ResizeHorizontal : MouseCursor.ResizeVertical, (minMaxSliderState != null && minMaxSliderState.whereWeDrag == 1) ? id : (-1));
                    EditorGUIUtility.AddCursorRect(position4, horiz ? MouseCursor.ResizeHorizontal : MouseCursor.ResizeVertical, (minMaxSliderState != null && minMaxSliderState.whereWeDrag == 2) ? id : (-1));
                    if (GUIUtility.hotControl != id || !position.Contains(current.mousePosition) || num - num2 == 0f)
                    {
                        break;
                    }

                    if (position2.Contains(current.mousePosition))
                    {
                        if (minMaxSliderState != null && (minMaxSliderState.whereWeDrag == 3 || minMaxSliderState.whereWeDrag == 4))
                        {
                            GUIUtility.hotControl = 0;
                        }
                    }
                    else
                    {
                        if (DateTime.Now < s_NextScrollStepTime)
                        {
                            break;
                        }

                        float num12 = (horiz ? current.mousePosition.x : current.mousePosition.y);
                        float num13 = (horiz ? position2.x : position2.y);
                        int num14 = ((num12 > num13) ? 4 : 3);
                        if (minMaxSliderState != null && num14 != minMaxSliderState.whereWeDrag)
                        {
                            break;
                        }

                        if (size != 0f && flag)
                        {
                            if (horiz)
                            {
                                if (num11 > position2.xMax - position.x)
                                {
                                    value += size * num8 * 0.9f;
                                }
                                else
                                {
                                    value -= size * num8 * 0.9f;
                                }
                            }
                            else if (num11 > position2.yMax - position.y)
                            {
                                value += size * num8 * 0.9f;
                            }
                            else
                            {
                                value -= size * num8 * 0.9f;
                            }

                            if (minMaxSliderState != null)
                            {
                                minMaxSliderState.whereWeDrag = -1;
                            }

                            GUI.changed = true;
                        }

                        value = Mathf.Clamp(value, num3, num4 - size);
                        s_NextScrollStepTime = DateTime.Now.AddMilliseconds(kScrollWait);
                    }

                    break;
            }
        }

        public static bool DragSelection(Rect[] positions, ref bool[] selections, GUIStyle style)
        {
            int controlID = GUIUtility.GetControlID(34553287, FocusType.Keyboard);
            Event current = Event.current;
            int num = -1;
            for (int num2 = positions.Length - 1; num2 >= 0; num2--)
            {
                if (positions[num2].Contains(current.mousePosition))
                {
                    num = num2;
                    break;
                }
            }

            switch (current.GetTypeForControl(controlID))
            {
                case EventType.Repaint:
                    {
                        for (int l = 0; l < positions.Length; l++)
                        {
                            style.Draw(positions[l], GUIContent.none, controlID, selections[l]);
                        }

                        break;
                    }
                case EventType.MouseDown:
                    {
                        if (current.button != 0 || num < 0)
                        {
                            break;
                        }

                        GUIUtility.keyboardControl = 0;
                        bool flag = false;
                        if (selections[num])
                        {
                            int num3 = 0;
                            bool[] array = selections;
                            for (int i = 0; i < array.Length; i++)
                            {
                                if (array[i])
                                {
                                    num3++;
                                    if (num3 > 1)
                                    {
                                        break;
                                    }
                                }
                            }

                            if (num3 == 1)
                            {
                                flag = true;
                            }
                        }

                        if (!current.shift && !EditorGUI.actionKey)
                        {
                            for (int j = 0; j < positions.Length; j++)
                            {
                                selections[j] = false;
                            }
                        }

                        initIndex = num;
                        initSelections = (bool[])selections.Clone();
                        adding = true;
                        if ((current.shift || EditorGUI.actionKey) && selections[num])
                        {
                            adding = false;
                        }

                        selections[num] = !flag && adding;
                        GUIUtility.hotControl = controlID;
                        current.Use();
                        return true;
                    }
                case EventType.MouseDrag:
                    {
                        if (GUIUtility.hotControl != controlID || current.button != 0)
                        {
                            break;
                        }

                        if (num < 0)
                        {
                            Rect rect = new Rect(positions[0].x, positions[0].y - 200f, positions[0].width, 200f);
                            if (rect.Contains(current.mousePosition))
                            {
                                num = 0;
                            }

                            rect.y = positions[^1].yMax;
                            if (rect.Contains(current.mousePosition))
                            {
                                num = selections.Length - 1;
                            }
                        }

                        if (num < 0)
                        {
                            return false;
                        }

                        int num4 = Mathf.Min(initIndex, num);
                        int num5 = Mathf.Max(initIndex, num);
                        for (int k = 0; k < selections.Length; k++)
                        {
                            if (k >= num4 && k <= num5)
                            {
                                selections[k] = adding;
                            }
                            else
                            {
                                selections[k] = initSelections[k];
                            }
                        }

                        current.Use();
                        return true;
                    }
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                    }

                    break;
            }

            return false;
        }

        private static bool Any(bool[] selections)
        {
            for (int i = 0; i < selections.Length; i++)
            {
                if (selections[i])
                {
                    return true;
                }
            }

            return false;
        }

        public static HighLevelEvent MultiSelection(Rect rect, Rect[] positions, GUIContent content, Rect[] hitPositions, ref bool[] selections, bool[] readOnly, out int clickedIndex, out Vector2 offset, out float startSelect, out float endSelect, GUIStyle style)
        {
            int controlID = GUIUtility.GetControlID(41623453, FocusType.Keyboard);
            Event current = Event.current;
            offset = Vector2.zero;
            clickedIndex = -1;
            startSelect = (endSelect = 0f);
            if (current.type == EventType.Used)
            {
                return HighLevelEvent.None;
            }

            bool flag = false;
            if (Event.current.type != EventType.Layout && GUIUtility.keyboardControl == controlID)
            {
                flag = true;
            }

            switch (current.GetTypeForControl(controlID))
            {
                case EventType.Repaint:
                    {
                        if (GUIUtility.hotControl == controlID && s_MultiSelectDragSelection == DragSelectionState.DragSelecting)
                        {
                            float num = Mathf.Min(s_StartSelectPos.x, current.mousePosition.x);
                            float num2 = Mathf.Max(s_StartSelectPos.x, current.mousePosition.x);
                            Rect position = new Rect(0f, 0f, rect.width, rect.height);
                            position.x = num;
                            position.width = num2 - num;
                            if (position.width > 1f)
                            {
                                GUI.Box(position, "", ms_Styles.selectionRect);
                            }
                        }

                        Color color = GUI.color;
                        for (int i = 0; i < positions.Length; i++)
                        {
                            if (readOnly != null && readOnly[i])
                            {
                                GUI.color = color * new Color(0.9f, 0.9f, 0.9f, 0.5f);
                            }
                            else if (selections[i])
                            {
                                GUI.color = color * new Color(0.3f, 0.55f, 0.95f, 1f);
                            }
                            else
                            {
                                GUI.color = color * new Color(0.9f, 0.9f, 0.9f, 1f);
                            }

                            style.Draw(positions[i], content, controlID, selections[i]);
                        }

                        GUI.color = color;
                        break;
                    }
                case EventType.MouseDown:
                    {
                        if (current.button != 0)
                        {
                            break;
                        }

                        GUIUtility.hotControl = controlID;
                        GUIUtility.keyboardControl = controlID;
                        s_StartSelectPos = current.mousePosition;
                        int indexUnderMouse = GetIndexUnderMouse(hitPositions, readOnly);
                        if (Event.current.clickCount == 2 && indexUnderMouse >= 0)
                        {
                            for (int l = 0; l < selections.Length; l++)
                            {
                                selections[l] = false;
                            }

                            selections[indexUnderMouse] = true;
                            current.Use();
                            clickedIndex = indexUnderMouse;
                            return HighLevelEvent.DoubleClick;
                        }

                        if (indexUnderMouse >= 0)
                        {
                            if (!current.shift && !EditorGUI.actionKey && !selections[indexUnderMouse])
                            {
                                for (int m = 0; m < hitPositions.Length; m++)
                                {
                                    selections[m] = false;
                                }
                            }

                            if (current.shift || EditorGUI.actionKey)
                            {
                                selections[indexUnderMouse] = !selections[indexUnderMouse];
                            }
                            else
                            {
                                selections[indexUnderMouse] = true;
                            }

                            s_MouseDownPos = current.mousePosition;
                            s_MultiSelectDragSelection = DragSelectionState.None;
                            current.Use();
                            clickedIndex = indexUnderMouse;
                            return HighLevelEvent.SelectionChanged;
                        }

                        bool flag4 = false;
                        if (!current.shift && !EditorGUI.actionKey)
                        {
                            for (int n = 0; n < hitPositions.Length; n++)
                            {
                                selections[n] = false;
                            }

                            flag4 = true;
                        }
                        else
                        {
                            flag4 = false;
                        }

                        s_SelectionBackup = new List<bool>(selections);
                        s_LastFrameSelections = new List<bool>(selections);
                        s_MultiSelectDragSelection = DragSelectionState.DragSelecting;
                        current.Use();
                        return flag4 ? HighLevelEvent.SelectionChanged : HighLevelEvent.None;
                    }
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl != controlID)
                    {
                        break;
                    }

                    if (s_MultiSelectDragSelection == DragSelectionState.DragSelecting)
                    {
                        float num3 = Mathf.Min(s_StartSelectPos.x, current.mousePosition.x);
                        float num4 = Mathf.Max(s_StartSelectPos.x, current.mousePosition.x);
                        s_SelectionBackup.CopyTo(selections);
                        for (int j = 0; j < hitPositions.Length; j++)
                        {
                            if (!selections[j])
                            {
                                float num5 = hitPositions[j].x + hitPositions[j].width * 0.5f;
                                if (num5 >= num3 && num5 <= num4)
                                {
                                    selections[j] = true;
                                }
                            }
                        }

                        current.Use();
                        startSelect = num3;
                        endSelect = num4;
                        bool flag3 = false;
                        for (int k = 0; k < selections.Length; k++)
                        {
                            if (selections[k] != s_LastFrameSelections[k])
                            {
                                flag3 = true;
                                s_LastFrameSelections[k] = selections[k];
                            }
                        }

                        return flag3 ? HighLevelEvent.SelectionChanged : HighLevelEvent.None;
                    }

                    offset = current.mousePosition - s_MouseDownPos;
                    current.Use();
                    if (s_MultiSelectDragSelection == DragSelectionState.None)
                    {
                        s_MultiSelectDragSelection = DragSelectionState.Dragging;
                        return HighLevelEvent.BeginDrag;
                    }

                    return HighLevelEvent.Drag;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                        if (s_StartSelectPos != current.mousePosition)
                        {
                            current.Use();
                        }

                        if (s_MultiSelectDragSelection != 0)
                        {
                            s_MultiSelectDragSelection = DragSelectionState.None;
                            s_SelectionBackup = null;
                            s_LastFrameSelections = null;
                            return HighLevelEvent.EndDrag;
                        }

                        clickedIndex = GetIndexUnderMouse(hitPositions, readOnly);
                        if (current.clickCount == 1)
                        {
                            return HighLevelEvent.Click;
                        }
                    }

                    break;
                case EventType.ValidateCommand:
                case EventType.ExecuteCommand:
                    {
                        if (!flag)
                        {
                            break;
                        }

                        bool flag2 = current.type == EventType.ExecuteCommand;
                        switch (current.commandName)
                        {
                            case "Delete":
                                current.Use();
                                if (flag2)
                                {
                                    return HighLevelEvent.Delete;
                                }

                                break;
                            case "Copy":
                                current.Use();
                                if (flag2)
                                {
                                    return HighLevelEvent.Copy;
                                }

                                break;
                            case "Paste":
                                current.Use();
                                if (flag2)
                                {
                                    return HighLevelEvent.Paste;
                                }

                                break;
                        }

                        break;
                    }
                case EventType.KeyDown:
                    if (flag && (current.keyCode == KeyCode.Backspace || current.keyCode == KeyCode.Delete))
                    {
                        current.Use();
                        return HighLevelEvent.Delete;
                    }

                    break;
                case EventType.ContextClick:
                    {
                        int indexUnderMouse = GetIndexUnderMouse(hitPositions, readOnly);
                        if (indexUnderMouse >= 0)
                        {
                            clickedIndex = indexUnderMouse;
                            GUIUtility.keyboardControl = controlID;
                            current.Use();
                            return HighLevelEvent.ContextClick;
                        }

                        break;
                    }
            }

            return HighLevelEvent.None;
        }

        private static int GetIndexUnderMouse(Rect[] hitPositions, bool[] readOnly)
        {
            Vector2 mousePosition = Event.current.mousePosition;
            for (int num = hitPositions.Length - 1; num >= 0; num--)
            {
                if ((readOnly == null || !readOnly[num]) && hitPositions[num].Contains(mousePosition))
                {
                    return num;
                }
            }

            return -1;
        }

        internal static Rect FromToRect(Vector2 start, Vector2 end)
        {
            Rect result = new Rect(start.x, start.y, end.x - start.x, end.y - start.y);
            if (result.width < 0f)
            {
                result.x += result.width;
                result.width = 0f - result.width;
            }

            if (result.height < 0f)
            {
                result.y += result.height;
                result.height = 0f - result.height;
            }

            return result;
        }
    }
}
#endif