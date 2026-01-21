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
    public class ZoomableArea
    {
        public enum YDirection
        {
            Positive,
            Negative
        }

        public class Styles
        {
            private class SliderTypeStyles
            {
                public class SliderAxisStyles
                {
                    public GUIStyle horizontal;

                    public GUIStyle vertical;
                }

                public SliderAxisStyles scrollbar;

                public SliderAxisStyles minMaxSliders;
            }

            public GUIStyle horizontalScrollbar;

            public GUIStyle horizontalScrollbarLeftButton;

            public GUIStyle horizontalScrollbarRightButton;

            public GUIStyle verticalScrollbar;

            public GUIStyle verticalScrollbarUpButton;

            public GUIStyle verticalScrollbarDownButton;

            public bool enableSliderZoomHorizontal;

            public bool enableSliderZoomVertical;

            public float sliderWidth;

            public float visualSliderWidth;

            private bool minimalGUI;

            private static SliderTypeStyles minimalSliderStyles;

            private static SliderTypeStyles normalSliderStyles;

            public GUIStyle horizontalMinMaxScrollbarThumb => GetSliderAxisStyle(enableSliderZoomHorizontal).horizontal;

            public GUIStyle verticalMinMaxScrollbarThumb => GetSliderAxisStyle(enableSliderZoomVertical).vertical;

            private SliderTypeStyles.SliderAxisStyles GetSliderAxisStyle(bool enableSliderZoom)
            {
                if (minimalGUI)
                {
                    return enableSliderZoom ? minimalSliderStyles.minMaxSliders : minimalSliderStyles.scrollbar;
                }

                return enableSliderZoom ? normalSliderStyles.minMaxSliders : normalSliderStyles.scrollbar;
            }

            public Styles(bool minimalGUI)
            {
                if (minimalGUI)
                {
                    visualSliderWidth = 0f;
                    sliderWidth = 13f;
                }
                else
                {
                    visualSliderWidth = 13f;
                    sliderWidth = 13f;
                }
            }

            public void InitGUIStyles(bool minimalGUI, bool enableSliderZoom)
            {
                InitGUIStyles(minimalGUI, enableSliderZoom, enableSliderZoom);
            }

            public void InitGUIStyles(bool minimalGUI, bool enableSliderZoomHorizontal, bool enableSliderZoomVertical)
            {
                this.minimalGUI = minimalGUI;
                this.enableSliderZoomHorizontal = enableSliderZoomHorizontal;
                this.enableSliderZoomVertical = enableSliderZoomVertical;
                if (minimalGUI)
                {
                    if (minimalSliderStyles == null)
                    {
                        minimalSliderStyles = new SliderTypeStyles
                        {
                            scrollbar = new SliderTypeStyles.SliderAxisStyles
                            {
                                horizontal = "MiniSliderhorizontal",
                                vertical = "MiniSliderVertical"
                            },
                            minMaxSliders = new SliderTypeStyles.SliderAxisStyles
                            {
                                horizontal = "MiniMinMaxSliderHorizontal",
                                vertical = "MiniMinMaxSlidervertical"
                            }
                        };
                    }

                    horizontalScrollbarLeftButton = GUIStyle.none;
                    horizontalScrollbarRightButton = GUIStyle.none;
                    horizontalScrollbar = GUIStyle.none;
                    verticalScrollbarUpButton = GUIStyle.none;
                    verticalScrollbarDownButton = GUIStyle.none;
                    verticalScrollbar = GUIStyle.none;
                }
                else
                {
                    if (normalSliderStyles == null)
                    {
                        normalSliderStyles = new SliderTypeStyles
                        {
                            scrollbar = new SliderTypeStyles.SliderAxisStyles
                            {
                                horizontal = "horizontalscrollbarthumb",
                                vertical = "verticalscrollbarthumb"
                            },
                            minMaxSliders = new SliderTypeStyles.SliderAxisStyles
                            {
                                horizontal = "horizontalMinMaxScrollbarThumb",
                                vertical = "verticalMinMaxScrollbarThumb"
                            }
                        };
                    }

                    horizontalScrollbarLeftButton = "horizontalScrollbarLeftbutton";
                    horizontalScrollbarRightButton = "horizontalScrollbarRightbutton";
                    horizontalScrollbar = GUI.skin.horizontalScrollbar;
                    verticalScrollbarUpButton = "verticalScrollbarUpbutton";
                    verticalScrollbarDownButton = "verticalScrollbarDownbutton";
                    verticalScrollbar = GUI.skin.verticalScrollbar;
                }
            }
        }

        private static Vector2 m_MouseDownPosition = new Vector2(-1000000f, -1000000f);

        private static int zoomableAreaHash = "ZoomableArea".GetHashCode();

        [SerializeField]
        private bool m_HRangeLocked;

        [SerializeField]
        private bool m_VRangeLocked;

        public bool hZoomLockedByDefault = false;

        public bool vZoomLockedByDefault = false;

        [SerializeField]
        private float m_HBaseRangeMin = 0f;

        [SerializeField]
        private float m_HBaseRangeMax = 1f;

        [SerializeField]
        private float m_VBaseRangeMin = 0f;

        [SerializeField]
        private float m_VBaseRangeMax = 1f;

        [SerializeField]
        private bool m_HAllowExceedBaseRangeMin = true;

        [SerializeField]
        private bool m_HAllowExceedBaseRangeMax = true;

        [SerializeField]
        private bool m_VAllowExceedBaseRangeMin = true;

        [SerializeField]
        private bool m_VAllowExceedBaseRangeMax = true;

        private const float kMinScale = 1E-05f;

        private const float kMaxScale = 100000f;

        private float m_HScaleMin = 1E-05f;

        private float m_HScaleMax = 100000f;

        private float m_VScaleMin = 1E-05f;

        private float m_VScaleMax = 100000f;

        private float m_MinWidth = 0.05f;

        private const float kMinHeight = 0.05f;

        private const float k_ScrollStepSize = 10f;

        [SerializeField]
        private bool m_ScaleWithWindow = false;

        [SerializeField]
        private bool m_HSlider = true;

        [SerializeField]
        private bool m_VSlider = true;

        [SerializeField]
        private bool m_IgnoreScrollWheelUntilClicked = false;

        [SerializeField]
        private bool m_EnableMouseInput = true;

        [SerializeField]
        private bool m_EnableSliderZoomHorizontal = true;

        [SerializeField]
        private bool m_EnableSliderZoomVertical = true;

        public bool m_UniformScale;

        [SerializeField]
        private YDirection m_UpDirection = YDirection.Positive;

        [SerializeField]
        private Rect m_DrawArea = new Rect(0f, 0f, 100f, 100f);

        [SerializeField]
        internal Vector2 m_Scale = new Vector2(1f, -1f);

        [SerializeField]
        internal Vector2 m_Translation = new Vector2(0f, 0f);

        [SerializeField]
        private float m_MarginLeft;

        [SerializeField]
        private float m_MarginRight;

        [SerializeField]
        private float m_MarginTop;

        [SerializeField]
        private float m_MarginBottom;

        [SerializeField]
        private Rect m_LastShownAreaInsideMargins = new Rect(0f, 0f, 100f, 100f);

        internal int areaControlID;

        private int verticalScrollbarID;

        private int horizontalScrollbarID;

        [SerializeField]
        private bool m_MinimalGUI;

        private Styles m_Styles;

        public bool hRangeLocked
        {
            get
            {
                return m_HRangeLocked;
            }
            set
            {
                m_HRangeLocked = value;
            }
        }

        public bool vRangeLocked
        {
            get
            {
                return m_VRangeLocked;
            }
            set
            {
                m_VRangeLocked = value;
            }
        }

        public float hBaseRangeMin
        {
            get
            {
                return m_HBaseRangeMin;
            }
            set
            {
                m_HBaseRangeMin = value;
            }
        }

        public float hBaseRangeMax
        {
            get
            {
                return m_HBaseRangeMax;
            }
            set
            {
                m_HBaseRangeMax = value;
            }
        }

        public float vBaseRangeMin
        {
            get
            {
                return m_VBaseRangeMin;
            }
            set
            {
                m_VBaseRangeMin = value;
            }
        }

        public float vBaseRangeMax
        {
            get
            {
                return m_VBaseRangeMax;
            }
            set
            {
                m_VBaseRangeMax = value;
            }
        }

        public bool hAllowExceedBaseRangeMin
        {
            get
            {
                return m_HAllowExceedBaseRangeMin;
            }
            set
            {
                m_HAllowExceedBaseRangeMin = value;
            }
        }

        public bool hAllowExceedBaseRangeMax
        {
            get
            {
                return m_HAllowExceedBaseRangeMax;
            }
            set
            {
                m_HAllowExceedBaseRangeMax = value;
            }
        }

        public bool vAllowExceedBaseRangeMin
        {
            get
            {
                return m_VAllowExceedBaseRangeMin;
            }
            set
            {
                m_VAllowExceedBaseRangeMin = value;
            }
        }

        public bool vAllowExceedBaseRangeMax
        {
            get
            {
                return m_VAllowExceedBaseRangeMax;
            }
            set
            {
                m_VAllowExceedBaseRangeMax = value;
            }
        }

        public float hRangeMin
        {
            get
            {
                return hAllowExceedBaseRangeMin ? float.NegativeInfinity : hBaseRangeMin;
            }
            set
            {
                SetAllowExceed(ref m_HBaseRangeMin, ref m_HAllowExceedBaseRangeMin, value);
            }
        }

        public float hRangeMax
        {
            get
            {
                return hAllowExceedBaseRangeMax ? float.PositiveInfinity : hBaseRangeMax;
            }
            set
            {
                SetAllowExceed(ref m_HBaseRangeMax, ref m_HAllowExceedBaseRangeMax, value);
            }
        }

        public float vRangeMin
        {
            get
            {
                return vAllowExceedBaseRangeMin ? float.NegativeInfinity : vBaseRangeMin;
            }
            set
            {
                SetAllowExceed(ref m_VBaseRangeMin, ref m_VAllowExceedBaseRangeMin, value);
            }
        }

        public float vRangeMax
        {
            get
            {
                return vAllowExceedBaseRangeMax ? float.PositiveInfinity : vBaseRangeMax;
            }
            set
            {
                SetAllowExceed(ref m_VBaseRangeMax, ref m_VAllowExceedBaseRangeMax, value);
            }
        }

        public float minWidth
        {
            get
            {
                return m_MinWidth;
            }
            set
            {
                if (value > 0f)
                {
                    m_MinWidth = value;
                    return;
                }

                Debug.LogWarning("Zoomable area width cannot have a value of or below 0. Reverting back to a default value of 0.05f");
                m_MinWidth = 0.05f;
            }
        }

        public float hScaleMin
        {
            get
            {
                return m_HScaleMin;
            }
            set
            {
                m_HScaleMin = Mathf.Clamp(value, 1E-05f, 100000f);
                styles.enableSliderZoomHorizontal = allowSliderZoomHorizontal;
            }
        }

        public float hScaleMax
        {
            get
            {
                return m_HScaleMax;
            }
            set
            {
                m_HScaleMax = Mathf.Clamp(value, 1E-05f, 100000f);
                styles.enableSliderZoomHorizontal = allowSliderZoomHorizontal;
            }
        }

        public float vScaleMin
        {
            get
            {
                return m_VScaleMin;
            }
            set
            {
                m_VScaleMin = Mathf.Clamp(value, 1E-05f, 100000f);
                styles.enableSliderZoomVertical = allowSliderZoomVertical;
            }
        }

        public float vScaleMax
        {
            get
            {
                return m_VScaleMax;
            }
            set
            {
                m_VScaleMax = Mathf.Clamp(value, 1E-05f, 100000f);
                styles.enableSliderZoomVertical = allowSliderZoomVertical;
            }
        }

        public bool scaleWithWindow
        {
            get
            {
                return m_ScaleWithWindow;
            }
            set
            {
                m_ScaleWithWindow = value;
            }
        }

        public bool hSlider
        {
            get
            {
                return m_HSlider;
            }
            set
            {
                Rect rect = this.rect;
                m_HSlider = value;
                this.rect = rect;
            }
        }

        public bool vSlider
        {
            get
            {
                return m_VSlider;
            }
            set
            {
                Rect rect = this.rect;
                m_VSlider = value;
                this.rect = rect;
            }
        }

        public bool ignoreScrollWheelUntilClicked
        {
            get
            {
                return m_IgnoreScrollWheelUntilClicked;
            }
            set
            {
                m_IgnoreScrollWheelUntilClicked = value;
            }
        }

        public bool enableMouseInput
        {
            get
            {
                return m_EnableMouseInput;
            }
            set
            {
                m_EnableMouseInput = value;
            }
        }

        protected bool allowSliderZoomHorizontal => m_EnableSliderZoomHorizontal && m_HScaleMin < m_HScaleMax;

        protected bool allowSliderZoomVertical => m_EnableSliderZoomVertical && m_VScaleMin < m_VScaleMax;

        public bool uniformScale
        {
            get
            {
                return m_UniformScale;
            }
            set
            {
                m_UniformScale = value;
            }
        }

        public YDirection upDirection
        {
            get
            {
                return m_UpDirection;
            }
            set
            {
                if (m_UpDirection != value)
                {
                    m_UpDirection = value;
                    m_Scale.y = 0f - m_Scale.y;
                }
            }
        }

        public Vector2 scale => m_Scale;

        public Vector2 translation => m_Translation;

        public float margin
        {
            set
            {
                m_MarginLeft = (m_MarginRight = (m_MarginTop = (m_MarginBottom = value)));
            }
        }

        public float leftmargin
        {
            get
            {
                return m_MarginLeft;
            }
            set
            {
                m_MarginLeft = value;
            }
        }

        public float rightmargin
        {
            get
            {
                return m_MarginRight;
            }
            set
            {
                m_MarginRight = value;
            }
        }

        public float topmargin
        {
            get
            {
                return m_MarginTop;
            }
            set
            {
                m_MarginTop = value;
            }
        }

        public float bottommargin
        {
            get
            {
                return m_MarginBottom;
            }
            set
            {
                m_MarginBottom = value;
            }
        }

        public float vSliderWidth => vSlider ? styles.sliderWidth : 0f;

        public float hSliderHeight => hSlider ? styles.sliderWidth : 0f;

        protected Styles styles
        {
            get
            {
                if (m_Styles == null)
                {
                    m_Styles = new Styles(m_MinimalGUI);
                }

                return m_Styles;
            }
        }

        public Rect rect
        {
            get
            {
                return new Rect(drawRect.x, drawRect.y, drawRect.width + (m_VSlider ? styles.visualSliderWidth : 0f), drawRect.height + (m_HSlider ? styles.visualSliderWidth : 0f));
            }
            set
            {
                Rect rect = new Rect(value.x, value.y, value.width - (m_VSlider ? styles.visualSliderWidth : 0f), value.height - (m_HSlider ? styles.visualSliderWidth : 0f));
                if (rect != m_DrawArea)
                {
                    if (m_ScaleWithWindow)
                    {
                        m_DrawArea = rect;
                        shownAreaInsideMargins = m_LastShownAreaInsideMargins;
                    }
                    else
                    {
                        m_Translation += new Vector2((rect.width - m_DrawArea.width) / 2f, (rect.height - m_DrawArea.height) / 2f);
                        m_DrawArea = rect;
                    }
                }

                EnforceScaleAndRange();
            }
        }

        public Rect drawRect => m_DrawArea;

        public Rect shownArea
        {
            get
            {
                if (m_UpDirection == YDirection.Positive)
                {
                    return new Rect((0f - m_Translation.x) / m_Scale.x, (0f - (m_Translation.y - drawRect.height)) / m_Scale.y, drawRect.width / m_Scale.x, drawRect.height / (0f - m_Scale.y));
                }

                return new Rect((0f - m_Translation.x) / m_Scale.x, (0f - m_Translation.y) / m_Scale.y, drawRect.width / m_Scale.x, drawRect.height / m_Scale.y);
            }
            set
            {
                float num = ((value.width < m_MinWidth) ? m_MinWidth : value.width);
                float num2 = ((value.height < 0.05f) ? 0.05f : value.height);
                if (m_UpDirection == YDirection.Positive)
                {
                    m_Scale.x = drawRect.width / num;
                    m_Scale.y = (0f - drawRect.height) / num2;
                    m_Translation.x = (0f - value.x) * m_Scale.x;
                    m_Translation.y = drawRect.height - value.y * m_Scale.y;
                }
                else
                {
                    m_Scale.x = drawRect.width / num;
                    m_Scale.y = drawRect.height / num2;
                    m_Translation.x = (0f - value.x) * m_Scale.x;
                    m_Translation.y = (0f - value.y) * m_Scale.y;
                }

                EnforceScaleAndRange();
            }
        }

        public Rect shownAreaInsideMargins
        {
            get
            {
                return shownAreaInsideMarginsInternal;
            }
            set
            {
                shownAreaInsideMarginsInternal = value;
                EnforceScaleAndRange();
            }
        }

        private Rect shownAreaInsideMarginsInternal
        {
            get
            {
                float num = leftmargin / m_Scale.x;
                float num2 = rightmargin / m_Scale.x;
                float num3 = topmargin / m_Scale.y;
                float num4 = bottommargin / m_Scale.y;
                Rect result = shownArea;
                result.x += num;
                result.y -= num3;
                result.width -= num + num2;
                result.height += num3 + num4;
                return result;
            }
            set
            {
                float num = ((value.width < m_MinWidth) ? m_MinWidth : value.width);
                float num2 = ((value.height < 0.05f) ? 0.05f : value.height);
                float num3 = drawRect.width - leftmargin - rightmargin;
                if (num3 < m_MinWidth)
                {
                    num3 = m_MinWidth;
                }

                float num4 = drawRect.height - topmargin - bottommargin;
                if (num4 < 0.05f)
                {
                    num4 = 0.05f;
                }

                if (m_UpDirection == YDirection.Positive)
                {
                    m_Scale.x = num3 / num;
                    m_Scale.y = (0f - num4) / num2;
                    m_Translation.x = (0f - value.x) * m_Scale.x + leftmargin;
                    m_Translation.y = drawRect.height - value.y * m_Scale.y - topmargin;
                }
                else
                {
                    m_Scale.x = num3 / num;
                    m_Scale.y = num4 / num2;
                    m_Translation.x = (0f - value.x) * m_Scale.x + leftmargin;
                    m_Translation.y = (0f - value.y) * m_Scale.y + topmargin;
                }
            }
        }

        public virtual Bounds drawingBounds => new Bounds(new Vector3((hBaseRangeMin + hBaseRangeMax) * 0.5f, (vBaseRangeMin + vBaseRangeMax) * 0.5f, 0f), new Vector3(hBaseRangeMax - hBaseRangeMin, vBaseRangeMax - vBaseRangeMin, 1f));

        public Matrix4x4 drawingToViewMatrix => Matrix4x4.TRS(m_Translation, Quaternion.identity, new Vector3(m_Scale.x, m_Scale.y, 1f));

        public Vector2 mousePositionInDrawing => ViewToDrawingTransformPoint(Event.current.mousePosition);

        private void SetAllowExceed(ref float rangeEnd, ref bool allowExceed, float value)
        {
            if (value == float.NegativeInfinity || value == float.PositiveInfinity)
            {
                rangeEnd = ((value != float.NegativeInfinity) ? 1 : 0);
                allowExceed = true;
            }
            else
            {
                rangeEnd = value;
                allowExceed = false;
            }
        }

        internal void SetDrawRectHack(Rect r)
        {
            m_DrawArea = r;
        }

        public void SetShownHRangeInsideMargins(float min, float max)
        {
            float num = drawRect.width - leftmargin - rightmargin;
            if (num < m_MinWidth)
            {
                num = m_MinWidth;
            }

            float num2 = max - min;
            if (num2 < m_MinWidth)
            {
                num2 = m_MinWidth;
            }

            m_Scale.x = num / num2;
            m_Translation.x = (0f - min) * m_Scale.x + leftmargin;
            EnforceScaleAndRange();
        }

        public void SetShownHRange(float min, float max)
        {
            float num = max - min;
            if (num < m_MinWidth)
            {
                num = m_MinWidth;
            }

            m_Scale.x = drawRect.width / num;
            m_Translation.x = (0f - min) * m_Scale.x;
            EnforceScaleAndRange();
        }

        public void SetShownVRangeInsideMargins(float min, float max)
        {
            float num = drawRect.height - topmargin - bottommargin;
            if (num < 0.05f)
            {
                num = 0.05f;
            }

            float num2 = max - min;
            if (num2 < 0.05f)
            {
                num2 = 0.05f;
            }

            if (m_UpDirection == YDirection.Positive)
            {
                m_Scale.y = (0f - num) / num2;
                m_Translation.y = drawRect.height - min * m_Scale.y - topmargin;
            }
            else
            {
                m_Scale.y = num / num2;
                m_Translation.y = (0f - min) * m_Scale.y - bottommargin;
            }

            EnforceScaleAndRange();
        }

        public void SetShownVRange(float min, float max)
        {
            float num = max - min;
            if (num < 0.05f)
            {
                num = 0.05f;
            }

            if (m_UpDirection == YDirection.Positive)
            {
                m_Scale.y = (0f - drawRect.height) / num;
                m_Translation.y = drawRect.height - min * m_Scale.y;
            }
            else
            {
                m_Scale.y = drawRect.height / num;
                m_Translation.y = (0f - min) * m_Scale.y;
            }

            EnforceScaleAndRange();
        }

        private float GetWidthInsideMargins(float widthWithMargins, bool substractSliderWidth = false)
        {
            float num = ((widthWithMargins < m_MinWidth) ? m_MinWidth : widthWithMargins);
            float a = num - leftmargin - rightmargin - ((!substractSliderWidth) ? 0f : (m_VSlider ? styles.visualSliderWidth : 0f));
            return Mathf.Max(a, m_MinWidth);
        }

        private float GetHeightInsideMargins(float heightWithMargins, bool substractSliderHeight = false)
        {
            float num = ((heightWithMargins < 0.05f) ? 0.05f : heightWithMargins);
            float a = num - topmargin - bottommargin - ((!substractSliderHeight) ? 0f : (m_HSlider ? styles.visualSliderWidth : 0f));
            return Mathf.Max(a, 0.05f);
        }

        public Vector2 DrawingToViewTransformPoint(Vector2 lhs)
        {
            return new Vector2(lhs.x * m_Scale.x + m_Translation.x, lhs.y * m_Scale.y + m_Translation.y);
        }

        public Vector3 DrawingToViewTransformPoint(Vector3 lhs)
        {
            return new Vector3(lhs.x * m_Scale.x + m_Translation.x, lhs.y * m_Scale.y + m_Translation.y, 0f);
        }

        public Vector2 ViewToDrawingTransformPoint(Vector2 lhs)
        {
            return new Vector2((lhs.x - m_Translation.x) / m_Scale.x, (lhs.y - m_Translation.y) / m_Scale.y);
        }

        public Vector3 ViewToDrawingTransformPoint(Vector3 lhs)
        {
            return new Vector3((lhs.x - m_Translation.x) / m_Scale.x, (lhs.y - m_Translation.y) / m_Scale.y, 0f);
        }

        public Vector2 DrawingToViewTransformVector(Vector2 lhs)
        {
            return new Vector2(lhs.x * m_Scale.x, lhs.y * m_Scale.y);
        }

        public Vector3 DrawingToViewTransformVector(Vector3 lhs)
        {
            return new Vector3(lhs.x * m_Scale.x, lhs.y * m_Scale.y, 0f);
        }

        public Vector2 ViewToDrawingTransformVector(Vector2 lhs)
        {
            return new Vector2(lhs.x / m_Scale.x, lhs.y / m_Scale.y);
        }

        public Vector3 ViewToDrawingTransformVector(Vector3 lhs)
        {
            return new Vector3(lhs.x / m_Scale.x, lhs.y / m_Scale.y, 0f);
        }

        public Vector2 NormalizeInViewSpace(Vector2 vec)
        {
            vec = Vector2.Scale(vec, m_Scale);
            vec /= vec.magnitude;
            return Vector2.Scale(vec, new Vector2(1f / m_Scale.x, 1f / m_Scale.y));
        }

        private bool IsZoomEvent()
        {
            return Event.current.button == 1 && Event.current.alt;
        }

        private bool IsPanEvent()
        {
            return (Event.current.button == 0 && Event.current.alt) || (Event.current.button == 2 && !Event.current.command);
        }

        public ZoomableArea()
        {
            m_MinimalGUI = false;
        }

        public ZoomableArea(bool minimalGUI)
        {
            m_MinimalGUI = minimalGUI;
        }

        public ZoomableArea(bool minimalGUI, bool enableSliderZoom)
            : this(minimalGUI, enableSliderZoom, enableSliderZoom)
        {
        }

        public ZoomableArea(bool minimalGUI, bool enableSliderZoomHorizontal, bool enableSliderZoomVertical)
        {
            m_MinimalGUI = minimalGUI;
            m_EnableSliderZoomHorizontal = enableSliderZoomHorizontal;
            m_EnableSliderZoomVertical = enableSliderZoomVertical;
        }

        public void BeginViewGUI()
        {
            if (styles.horizontalScrollbar == null)
            {
                styles.InitGUIStyles(m_MinimalGUI, allowSliderZoomHorizontal, allowSliderZoomVertical);
            }

            if (enableMouseInput)
            {
                HandleZoomAndPanEvents(m_DrawArea);
            }

            horizontalScrollbarID = GUIUtility.GetControlID(EditorGUIExt.s_MinMaxSliderHash, FocusType.Passive);
            verticalScrollbarID = GUIUtility.GetControlID(EditorGUIExt.s_MinMaxSliderHash, FocusType.Passive);
            if (!m_MinimalGUI || Event.current.type != EventType.Repaint)
            {
                SliderGUI();
            }
        }

        public void HandleZoomAndPanEvents(Rect area)
        {
            GUILayout.BeginArea(area);
            area.x = 0f;
            area.y = 0f;
            int num = (areaControlID = GUIUtility.GetControlID(zoomableAreaHash, FocusType.Passive, area));
            switch (Event.current.GetTypeForControl(num))
            {
                case EventType.MouseDown:
                    if (area.Contains(Event.current.mousePosition))
                    {
                        GUIUtility.keyboardControl = num;
                        if (IsZoomEvent() || IsPanEvent())
                        {
                            GUIUtility.hotControl = num;
                            m_MouseDownPosition = mousePositionInDrawing;
                            Event.current.Use();
                        }
                    }

                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == num)
                    {
                        GUIUtility.hotControl = 0;
                        m_MouseDownPosition = new Vector2(-1000000f, -1000000f);
                    }

                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == num)
                    {
                        if (IsZoomEvent())
                        {
                            HandleZoomEvent(m_MouseDownPosition, scrollwhell: false);
                            Event.current.Use();
                        }
                        else if (IsPanEvent())
                        {
                            Pan();
                            Event.current.Use();
                        }
                    }

                    break;
                case EventType.ScrollWheel:
                    if (!area.Contains(Event.current.mousePosition))
                    {
                        HandleScrolling(area);
                    }
                    else if (!m_IgnoreScrollWheelUntilClicked || GUIUtility.keyboardControl == num)
                    {
                        HandleZoomEvent(mousePositionInDrawing, scrollwhell: true);
                        Event.current.Use();
                    }

                    break;
            }

            GUILayout.EndArea();
        }

        private void HandleScrolling(Rect area)
        {
            if (!m_MinimalGUI)
            {
                if (m_VSlider && new Rect(area.x + area.width, area.y + GUI.skin.verticalScrollbarUpButton.fixedHeight, vSliderWidth, area.height - (GUI.skin.verticalScrollbarDownButton.fixedHeight + hSliderHeight)).Contains(Event.current.mousePosition))
                {
                    SetTransform(new Vector2(m_Translation.x, m_Translation.y - Event.current.delta.y * 10f), m_Scale);
                    Event.current.Use();
                }
                else if (m_HSlider && new Rect(area.x + GUI.skin.horizontalScrollbarLeftButton.fixedWidth, area.y + area.height, area.width - (GUI.skin.horizontalScrollbarRightButton.fixedWidth + vSliderWidth), hSliderHeight).Contains(Event.current.mousePosition))
                {
                    SetTransform(new Vector2(m_Translation.x + Event.current.delta.y * 10f, m_Translation.y), m_Scale);
                    Event.current.Use();
                }
            }
        }

        public void EndViewGUI()
        {
            if (m_MinimalGUI && Event.current.type == EventType.Repaint)
            {
                SliderGUI();
            }
        }

        private void SliderGUI()
        {
            if (!m_HSlider && !m_VSlider)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(!enableMouseInput))
            {
                Bounds bounds = drawingBounds;
                Rect rect = shownAreaInsideMargins;
                float num = styles.sliderWidth - styles.visualSliderWidth;
                float num2 = ((vSlider && hSlider) ? num : 0f);
                Vector2 vector = m_Scale;
                if (m_HSlider)
                {
                    Rect position = new Rect(drawRect.x + 1f, drawRect.yMax - num, drawRect.width - num2, styles.sliderWidth);
                    float size = rect.width;
                    float value = rect.xMin;
                    if (allowSliderZoomHorizontal)
                    {
                        EditorGUIExt.MinMaxScroller(position, horizontalScrollbarID, ref value, ref size, bounds.min.x, bounds.max.x, float.NegativeInfinity, float.PositiveInfinity, styles.horizontalScrollbar, styles.horizontalMinMaxScrollbarThumb, styles.horizontalScrollbarLeftButton, styles.horizontalScrollbarRightButton, horiz: true);
                    }
                    else
                    {
                        value = GUI.HorizontalScrollbar(position, value, size, bounds.min.x, bounds.max.x, styles.horizontalScrollbar);
                    }

                    float num3 = value;
                    float num4 = value + size;
                    float widthInsideMargins = GetWidthInsideMargins(this.rect.width, substractSliderWidth: true);
                    if (num3 > rect.xMin)
                    {
                        num3 = Mathf.Min(num3, num4 - widthInsideMargins / m_HScaleMax);
                    }

                    if (num4 < rect.xMax)
                    {
                        num4 = Mathf.Max(num4, num3 + widthInsideMargins / m_HScaleMax);
                    }

                    SetShownHRangeInsideMargins(num3, num4);
                }

                if (m_VSlider)
                {
                    if (m_UpDirection == YDirection.Positive)
                    {
                        Rect position2 = new Rect(drawRect.xMax - num, drawRect.y, styles.sliderWidth, drawRect.height - num2);
                        float size2 = rect.height;
                        float value2 = 0f - rect.yMax;
                        if (allowSliderZoomVertical)
                        {
                            EditorGUIExt.MinMaxScroller(position2, verticalScrollbarID, ref value2, ref size2, 0f - bounds.max.y, 0f - bounds.min.y, float.NegativeInfinity, float.PositiveInfinity, styles.verticalScrollbar, styles.verticalMinMaxScrollbarThumb, styles.verticalScrollbarUpButton, styles.verticalScrollbarDownButton, horiz: false);
                        }
                        else
                        {
                            value2 = GUI.VerticalScrollbar(position2, value2, size2, 0f - bounds.max.y, 0f - bounds.min.y, styles.verticalScrollbar);
                        }

                        float num3 = 0f - (value2 + size2);
                        float num4 = 0f - value2;
                        float heightInsideMargins = GetHeightInsideMargins(this.rect.height, substractSliderHeight: true);
                        if (num3 > rect.yMin)
                        {
                            num3 = Mathf.Min(num3, num4 - heightInsideMargins / m_VScaleMax);
                        }

                        if (num4 < rect.yMax)
                        {
                            num4 = Mathf.Max(num4, num3 + heightInsideMargins / m_VScaleMax);
                        }

                        SetShownVRangeInsideMargins(num3, num4);
                    }
                    else
                    {
                        Rect position3 = new Rect(drawRect.xMax - num, drawRect.y, styles.sliderWidth, drawRect.height - num2);
                        float size3 = rect.height;
                        float value3 = rect.yMin;
                        if (allowSliderZoomVertical)
                        {
                            EditorGUIExt.MinMaxScroller(position3, verticalScrollbarID, ref value3, ref size3, bounds.min.y, bounds.max.y, float.NegativeInfinity, float.PositiveInfinity, styles.verticalScrollbar, styles.verticalMinMaxScrollbarThumb, styles.verticalScrollbarUpButton, styles.verticalScrollbarDownButton, horiz: false);
                        }
                        else
                        {
                            value3 = GUI.HorizontalScrollbar(position3, value3, size3, bounds.min.y, bounds.max.y, styles.verticalScrollbar);
                        }

                        float num3 = value3;
                        float num4 = value3 + size3;
                        float heightInsideMargins2 = GetHeightInsideMargins(this.rect.height, substractSliderHeight: true);
                        if (num3 > rect.yMin)
                        {
                            num3 = Mathf.Min(num3, num4 - heightInsideMargins2 / m_VScaleMax);
                        }

                        if (num4 < rect.yMax)
                        {
                            num4 = Mathf.Max(num4, num3 + heightInsideMargins2 / m_VScaleMax);
                        }

                        SetShownVRangeInsideMargins(num3, num4);
                    }
                }

                if (uniformScale)
                {
                    float num5 = drawRect.width / drawRect.height;
                    vector -= m_Scale;
                    Vector2 vector2 = new Vector2((0f - vector.y) * num5, (0f - vector.x) / num5);
                    m_Scale -= vector2;
                    m_Translation.x -= vector.y / 2f;
                    m_Translation.y -= vector.x / 2f;
                    EnforceScaleAndRange();
                }
            }
        }

        private void Pan()
        {
            if (!m_HRangeLocked)
            {
                m_Translation.x += Event.current.delta.x;
            }

            if (!m_VRangeLocked)
            {
                m_Translation.y += Event.current.delta.y;
            }

            EnforceScaleAndRange();
        }

        private void HandleZoomEvent(Vector2 zoomAround, bool scrollwhell)
        {
            float num = Event.current.delta.x + Event.current.delta.y;
            if (scrollwhell)
            {
                num = 0f - num;
            }

            float num2 = Mathf.Max(0.01f, 1f + num * 0.01f);
            float width = shownAreaInsideMargins.width;
            if (!(width / num2 <= m_MinWidth))
            {
                SetScaleFocused(zoomAround, num2 * m_Scale, Event.current.shift, EditorGUI.actionKey);
            }
        }

        public void SetScaleFocused(Vector2 focalPoint, Vector2 newScale)
        {
            SetScaleFocused(focalPoint, newScale, lockHorizontal: false, lockVertical: false);
        }

        public void SetScaleFocused(Vector2 focalPoint, Vector2 newScale, bool lockHorizontal, bool lockVertical)
        {
            if (uniformScale)
            {
                lockHorizontal = (lockVertical = false);
            }
            else
            {
                if (hZoomLockedByDefault)
                {
                    lockHorizontal = !lockHorizontal;
                }

                if (hZoomLockedByDefault)
                {
                    lockVertical = !lockVertical;
                }
            }

            if (!m_HRangeLocked && !lockHorizontal)
            {
                m_Translation.x -= focalPoint.x * (newScale.x - m_Scale.x);
                m_Scale.x = newScale.x;
            }

            if (!m_VRangeLocked && !lockVertical)
            {
                m_Translation.y -= focalPoint.y * (newScale.y - m_Scale.y);
                m_Scale.y = newScale.y;
            }

            EnforceScaleAndRange();
        }

        public void SetTransform(Vector2 newTranslation, Vector2 newScale)
        {
            m_Scale = newScale;
            m_Translation = newTranslation;
            EnforceScaleAndRange();
        }

        public void EnforceScaleAndRange()
        {
            Rect lastShownAreaInsideMargins = m_LastShownAreaInsideMargins;
            Rect rect = shownAreaInsideMargins;
            if (rect == lastShownAreaInsideMargins)
            {
                return;
            }

            float num = 0.01f;
            if (!Mathf.Approximately(rect.width, lastShownAreaInsideMargins.width))
            {
                float width = rect.width;
                if (rect.width < lastShownAreaInsideMargins.width)
                {
                    width = GetWidthInsideMargins(drawRect.width / m_HScaleMax);
                }
                else
                {
                    width = GetWidthInsideMargins(drawRect.width / m_HScaleMin);
                    if (hRangeMax != float.PositiveInfinity && hRangeMin != float.NegativeInfinity)
                    {
                        float num2 = hRangeMax - hRangeMin;
                        if (num2 < m_MinWidth)
                        {
                            num2 = m_MinWidth;
                        }

                        width = Mathf.Min(width, num2);
                    }
                }

                float t = Mathf.InverseLerp(lastShownAreaInsideMargins.width, rect.width, width);
                float num3 = Mathf.Lerp(lastShownAreaInsideMargins.width, rect.width, t);
                float num4 = Mathf.Abs(num3 - rect.width);
                rect = new Rect((num4 > num) ? Mathf.Lerp(lastShownAreaInsideMargins.x, rect.x, t) : rect.x, rect.y, num3, rect.height);
            }

            if (!Mathf.Approximately(rect.height, lastShownAreaInsideMargins.height))
            {
                float height = rect.height;
                if (rect.height < lastShownAreaInsideMargins.height)
                {
                    height = GetHeightInsideMargins(drawRect.height / m_VScaleMax);
                }
                else
                {
                    height = GetHeightInsideMargins(drawRect.height / m_VScaleMin);
                    if (vRangeMax != float.PositiveInfinity && vRangeMin != float.NegativeInfinity)
                    {
                        float num5 = vRangeMax - vRangeMin;
                        if (num5 < 0.05f)
                        {
                            num5 = 0.05f;
                        }

                        height = Mathf.Min(height, num5);
                    }
                }

                float t2 = Mathf.InverseLerp(lastShownAreaInsideMargins.height, rect.height, height);
                float num6 = Mathf.Lerp(lastShownAreaInsideMargins.height, rect.height, t2);
                float num7 = Mathf.Abs(num6 - rect.height);
                rect = new Rect(rect.x, (num7 > num) ? Mathf.Lerp(lastShownAreaInsideMargins.y, rect.y, t2) : rect.y, rect.width, num6);
            }

            if (rect.xMin < hRangeMin)
            {
                rect.x = hRangeMin;
            }

            if (rect.xMax > hRangeMax)
            {
                rect.x = hRangeMax - rect.width;
            }

            if (rect.yMin < vRangeMin)
            {
                rect.y = vRangeMin;
            }

            if (rect.yMax > vRangeMax)
            {
                rect.y = vRangeMax - rect.height;
            }

            shownAreaInsideMarginsInternal = rect;
            m_LastShownAreaInsideMargins = shownAreaInsideMargins;
        }

        public float PixelToTime(float pixelX, Rect rect)
        {
            Rect rect2 = shownArea;
            return (pixelX - rect.x) * rect2.width / rect.width + rect2.x;
        }

        public float TimeToPixel(float time, Rect rect)
        {
            Rect rect2 = shownArea;
            return (time - rect2.x) / rect2.width * rect.width + rect.x;
        }

        public float PixelDeltaToTime(Rect rect)
        {
            return shownArea.width / rect.width;
        }

        public void UpdateZoomScale(float fMaxScaleValue, float fMinScaleValue)
        {
            if (m_Scale.y > fMaxScaleValue || m_Scale.y < fMinScaleValue)
            {
                m_Scale.y = ((m_Scale.y > fMaxScaleValue) ? fMaxScaleValue : fMinScaleValue);
            }

            if (m_Scale.x > fMaxScaleValue || m_Scale.x < fMinScaleValue)
            {
                m_Scale.x = ((m_Scale.x > fMaxScaleValue) ? fMaxScaleValue : fMinScaleValue);
            }
        }
    }
}
#endif