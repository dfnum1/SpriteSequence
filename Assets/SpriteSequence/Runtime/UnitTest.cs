/********************************************************************
生成日期:	01:20:2026
类    名: 	UnitTest
作    者:	HappLI
描    述:	单元测试
*********************************************************************/
using System.Collections.Generic;
using UnityEngine;

namespace Framework.SpriteSeq
{
    public class UnitTest : MonoBehaviour
    {
        public bool useGPUInstancing = true;
        public SpriteSequenceData data;
        public int testCount = 100;

        public GameObject chengbaoPrefab;

        SpriteSequenceManager m_Mgr = new SpriteSequenceManager();
        private System.Collections.Generic.Dictionary<Vector3, KeyValuePair<int,int>> m_vPosition = null;

        private void Awake()
        {
            Application.targetFrameRate = 120;
            if(useGPUInstancing)
            {
                m_vPosition = new System.Collections.Generic.Dictionary<Vector3, KeyValuePair<int, int>>(testCount);
                int guid = 0;
                for (int i = 0; i < testCount; ++i)
                {
                    int guidMain = guid++;
                    int fengche = guid++;
                    m_Mgr.AddSequence(guidMain, data, "main");
                    m_Mgr.AddSequence(fengche, data, "200");
                    Vector3 pos = new Vector3(Random.Range(-100.0f, 100.0f), Random.Range(-50.0f, 50.0f), Random.Range(-50.0f, 50.0f));
                    m_Mgr.SetPosition(guidMain, pos);
                    m_Mgr.SetPosition(fengche, pos);
                    m_Mgr.SetScale(guidMain, Vector3.one);
                    m_Mgr.SetScale(fengche, Vector3.one);
                    var color = UnityEngine.Random.ColorHSV();
                    m_Mgr.SetColor(guidMain, color);
                    m_Mgr.SetColor(fengche, color);

                    m_vPosition[pos] = new KeyValuePair<int, int>(guidMain, fengche);
                }
            }
            else
            {
                for (int i = 0; i < testCount; ++i)
                {
                    var chebao = GameObject.Instantiate(chengbaoPrefab);
                    chebao.transform.position = new Vector3(Random.Range(-100.0f, 100.0f), Random.Range(-50.0f, 50.0f), Random.Range(-50.0f, 50.0f));
                    var color = UnityEngine.Random.ColorHSV();
                    chebao.GetComponent<SpriteRenderer>().color = color;
                    chebao.GetComponentInChildren<SpriteRenderer>().color = color;
                }
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
            if (useGPUInstancing)
            {
                Camera cam = Camera.main;
                m_Mgr.Render(cam);
                if(Input.GetMouseButtonUp(0))
                {
                    Vector3 mousePos = Input.mousePosition;

                    // 设定一个点击判定半径
                    float pickRadius = 20.0f;

                    bool bHit = false;
                    Vector3 hitPos = Vector3.zero;
                    KeyValuePair<int,int> removeGuid = default;
                    foreach (var db in m_vPosition)
                    {
                        Vector3 screen = cam.WorldToScreenPoint(db.Key);
                        float dist = Vector2.Distance(new Vector2(screen.x, screen.y), new Vector2(mousePos.x, mousePos.y));
                        if (dist < pickRadius)
                        {
                            removeGuid = db.Value;
                            bHit = true;
                            hitPos = db.Key;
                            break;
                        }
                    }
                    if (bHit)
                    {
                        m_Mgr.RemoveSequence(removeGuid.Key);
                        m_Mgr.RemoveSequence(removeGuid.Value);
                        m_vPosition.Remove(hitPos);
                    }
                }
            }
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
