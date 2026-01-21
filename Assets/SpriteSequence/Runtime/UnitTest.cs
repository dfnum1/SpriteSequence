/********************************************************************
生成日期:	01:20:2026
类    名: 	UnitTest
作    者:	HappLI
描    述:	单元测试
*********************************************************************/
using UnityEngine;

namespace Framework.SpriteSeq
{
    public class UnitTest : MonoBehaviour
    {
        public SpriteSequenceData data;
        public int testCount = 100;

        SpriteSequenceManager m_Mgr = new SpriteSequenceManager();

        private void Awake()
        {
            Application.targetFrameRate = 120;
            //帮我随机生成一些测试数据，并设置位置
            int guid = 0;
            for (int i=0; i< testCount; ++i)
            {
                int guidMain = guid++;
                int fengche = guid++;
                m_Mgr.AddSequence(guidMain, data,"main");
                m_Mgr.AddSequence(fengche, data,"200");
                Vector3 pos = new Vector3(Random.Range(-100.0f, 100.0f), Random.Range(-50.0f, 50.0f), Random.Range(-50.0f, 50.0f));
                m_Mgr.SetPosition(guidMain, pos);
                m_Mgr.SetPosition(fengche, pos);
                m_Mgr.SetScale(guidMain, Vector3.one);
                m_Mgr.SetScale(fengche, Vector3.one);
                var color = UnityEngine.Random.ColorHSV();
                m_Mgr.SetColor(guidMain, color);
                m_Mgr.SetColor(fengche, color);
            }
        }
        //--------------------------------------------------------
        private void OnDestroy()
        {
            m_Mgr.Destroy();
        }
        //--------------------------------------------------------
        private void Update()
        {
            m_Mgr.Render(Camera.main);
        }
        //--------------------------------------------------------
        void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 36; // Increased font size by 1.5 times
            style.normal.textColor = Color.white;

            // Draw outline/shadow effect
            GUIStyle outlineStyle = new GUIStyle(style);
            outlineStyle.normal.textColor = Color.black;

            string fps = $"FPS: {1.0f / Time.smoothDeltaTime:F1}";
            string count = $"Instance Count: {testCount*2:N0}";

            // Draw completely black background
            Color originalColor = GUI.color;
            GUI.color = Color.black;
            GUI.Box(new Rect(12, 12, 465, 105), GUIContent.none); // Increased size by 1.5 times
            GUI.color = originalColor;

            // Draw outline/shadow
            GUI.Label(new Rect(18, 18, 450, 45), fps, outlineStyle); // Increased size by 1.5 times
            GUI.Label(new Rect(18, 63, 450, 45), count, outlineStyle); // Increased size by 1.5 times

            // Draw main text
            GUI.Label(new Rect(15, 15, 450, 45), fps, style); // Increased size by 1.5 times
            GUI.Label(new Rect(15, 60, 450, 45), count, style); // Increased size by 1.5 times
        }
    }
}
