#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Framework.ED
{
    public class TickHandler
    {
        [SerializeField]
        private float[] m_TickModulos = new float[0];

        [SerializeField]
        private float[] m_TickStrengths = new float[0];

        [SerializeField]
        private int m_SmallestTick = 0;

        [SerializeField]
        private int m_BiggestTick = -1;

        [SerializeField]
        private float m_MinValue = 0f;

        [SerializeField]
        private float m_MaxValue = 1f;

        [SerializeField]
        private float m_PixelRange = 1f;

        private List<float> m_TickList = new List<float>(1000);

        public int tickLevels => m_BiggestTick - m_SmallestTick + 1;

        public void SetTickModulos(float[] tickModulos)
        {
            m_TickModulos = tickModulos;
        }

        public List<float> GetTickModulosForFrameRate(float frameRate)
        {
            if (frameRate > 1.07374182E+09f || frameRate != Mathf.Round(frameRate))
            {
                return new List<float>
                {
                    1f / frameRate,
                    5f / frameRate,
                    10f / frameRate,
                    50f / frameRate,
                    100f / frameRate,
                    500f / frameRate,
                    1000f / frameRate,
                    5000f / frameRate,
                    10000f / frameRate,
                    50000f / frameRate,
                    100000f / frameRate,
                    500000f / frameRate
                };
            }

            List<int> list = new List<int>();
            int num = 1;
            while ((float)num < frameRate && !((double)Math.Abs((float)num - frameRate) < 1E-05))
            {
                int num2 = Mathf.RoundToInt(frameRate / (float)num);
                if (num2 % 60 == 0)
                {
                    num *= 2;
                    list.Add(num);
                }
                else if (num2 % 30 == 0)
                {
                    num *= 3;
                    list.Add(num);
                }
                else if (num2 % 20 == 0)
                {
                    num *= 2;
                    list.Add(num);
                }
                else if (num2 % 10 == 0)
                {
                    num *= 2;
                    list.Add(num);
                }
                else if (num2 % 5 == 0)
                {
                    num *= 5;
                    list.Add(num);
                }
                else if (num2 % 2 == 0)
                {
                    num *= 2;
                    list.Add(num);
                }
                else if (num2 % 3 == 0)
                {
                    num *= 3;
                    list.Add(num);
                }
                else
                {
                    num = Mathf.RoundToInt(frameRate);
                }
            }

            List<float> list2 = new List<float>(13 + list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                list2.Add(1f / (float)list[list.Count - i - 1]);
            }

            list2.Add(1f);
            list2.Add(5f);
            list2.Add(10f);
            list2.Add(30f);
            list2.Add(60f);
            list2.Add(300f);
            list2.Add(600f);
            list2.Add(1800f);
            list2.Add(3600f);
            list2.Add(21600f);
            list2.Add(86400f);
            list2.Add(604800f);
            list2.Add(1209600f);
            return list2;
        }

        public void SetTickModulosForFrameRate(float frameRate)
        {
            List<float> tickModulosForFrameRate = GetTickModulosForFrameRate(frameRate);
            SetTickModulos(tickModulosForFrameRate.ToArray());
        }

        public void SetRanges(float minValue, float maxValue, float minPixel, float maxPixel)
        {
            m_MinValue = minValue;
            m_MaxValue = maxValue;
            m_PixelRange = maxPixel - minPixel;
        }

        public float[] GetTicksAtLevel(int level, bool excludeTicksFromHigherlevels)
        {
            if (level < 0)
            {
                return new float[0];
            }

            m_TickList.Clear();
            GetTicksAtLevel(level, excludeTicksFromHigherlevels, m_TickList);
            return m_TickList.ToArray();
        }

        public void GetTicksAtLevel(int level, bool excludeTicksFromHigherlevels, List<float> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            int num = Mathf.Clamp(m_SmallestTick + level, 0, m_TickModulos.Length - 1);
            int num2 = Mathf.FloorToInt(m_MinValue / m_TickModulos[num]);
            int num3 = Mathf.CeilToInt(m_MaxValue / m_TickModulos[num]);
            for (int i = num2; i <= num3; i++)
            {
                if (!excludeTicksFromHigherlevels || num >= m_BiggestTick || i % Mathf.RoundToInt(m_TickModulos[num + 1] / m_TickModulos[num]) != 0)
                {
                    list.Add((float)i * m_TickModulos[num]);
                }
            }
        }

        public float GetStrengthOfLevel(int level)
        {
            return m_TickStrengths[m_SmallestTick + level];
        }

        public float GetPeriodOfLevel(int level)
        {
            return m_TickModulos[Mathf.Clamp(m_SmallestTick + level, 0, m_TickModulos.Length - 1)];
        }

        public int GetLevelWithMinSeparation(float pixelSeparation)
        {
            for (int i = 0; i < m_TickModulos.Length; i++)
            {
                float num = m_TickModulos[i] * m_PixelRange / (m_MaxValue - m_MinValue);
                if (num >= pixelSeparation)
                {
                    return i - m_SmallestTick;
                }
            }

            return -1;
        }

        public void SetTickStrengths(float tickMinSpacing, float tickMaxSpacing, bool sqrt)
        {
            if (m_TickStrengths == null || m_TickStrengths.Length != m_TickModulos.Length)
            {
                m_TickStrengths = new float[m_TickModulos.Length];
            }

            m_SmallestTick = 0;
            m_BiggestTick = m_TickModulos.Length - 1;
            for (int num = m_TickModulos.Length - 1; num >= 0; num--)
            {
                float num2 = m_TickModulos[num] * m_PixelRange / (m_MaxValue - m_MinValue);
                m_TickStrengths[num] = (num2 - tickMinSpacing) / (tickMaxSpacing - tickMinSpacing);
                if (m_TickStrengths[num] >= 1f)
                {
                    m_BiggestTick = num;
                }

                if (num2 <= tickMinSpacing)
                {
                    m_SmallestTick = num;
                    break;
                }
            }

            for (int i = m_SmallestTick; i <= m_BiggestTick; i++)
            {
                m_TickStrengths[i] = Mathf.Clamp01(m_TickStrengths[i]);
                if (sqrt)
                {
                    m_TickStrengths[i] = Mathf.Sqrt(m_TickStrengths[i]);
                }
            }
        }
    }
}
#endif