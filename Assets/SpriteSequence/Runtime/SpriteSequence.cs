/********************************************************************
生成日期:	01:20:2026
类    名: 	SpriteSequence
作    者:	HappLI
描    述:	常规的基于Sprite的序列帧动画播放组件
*********************************************************************/
using System.Collections;
using UnityEngine;
using UnityEngine.U2D;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.U2D;
#endif

namespace Framework.SpriteSeq
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteSequence : MonoBehaviour
    {
        public string spriteLabel = "";
        public float fps = 12f;
        public bool loop = true;
        public SpriteRenderer spriteRender;
        public Sprite[] frames;
        int m_nIndex;
        //----------------------------------------------
        void Start()
        {
            if(spriteRender == null) spriteRender = GetComponent<SpriteRenderer>();
            if (frames == null || frames.Length == 0) { Debug.LogError("没找到帧图"); return; }
            StartCoroutine(Play());
        }
        //----------------------------------------------
        IEnumerator Play()
        {
            var delay = new WaitForSeconds(1f / fps);
            while (true)
            {
                spriteRender.sprite = frames[m_nIndex];
                m_nIndex = (m_nIndex + 1) % frames.Length;
                if (!loop && m_nIndex == 0) break;
                yield return delay;
            }
        }
    }
    //----------------------------------------------
    //! Editor
    //----------------------------------------------
#if UNITY_EDITOR
    [CustomEditor(typeof(SpriteSequence))]
    public class SpriteSequenceInspectorEditor : Editor
    {
        SpriteAtlas m_Atlas;
        Framework.ED.EditorTimer m_pTimer = new Framework.ED.EditorTimer();
        bool m_bPreview = false;
        float m_fPlayTime = 0;
        Editor m_SpriteRenderEditor = null;
        //----------------------------------------------
        private void OnEnable()
        {
            SpriteSequence seq = (SpriteSequence)target;
            seq.spriteRender = seq.GetComponent<SpriteRenderer>();
            EditorApplication.update += OnUpdate;
        }
        //----------------------------------------------
        private void OnDisable()
        {
            EditorApplication.update -= OnUpdate;
            if (Application.isPlaying) Object.Destroy(m_SpriteRenderEditor);
            else Object.DestroyImmediate(m_SpriteRenderEditor);
            m_SpriteRenderEditor = null;
        }
        //----------------------------------------------
        public override void OnInspectorGUI()
        {
            m_Atlas = EditorGUILayout.ObjectField("Sprite Atlas", m_Atlas, typeof(SpriteAtlas), false) as SpriteAtlas;
            SpriteSequence seq = (SpriteSequence)target;
            if (seq.spriteRender == null)
                seq.spriteRender = seq.GetComponent<SpriteRenderer>();
            if (seq.spriteRender)
                seq.spriteRender.hideFlags |= HideFlags.HideInInspector;

            DrawDefaultInspector();

            if (seq.spriteRender && !EditorUtility.IsPersistent(seq.spriteRender))
            {
                if (m_SpriteRenderEditor == null)
                {
                    m_SpriteRenderEditor = Editor.CreateEditor(seq.spriteRender);
                    m_SpriteRenderEditor.ResetTarget();
                }
                if (m_SpriteRenderEditor != null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("SpriteRenderer 属性", EditorStyles.boldLabel);
                    m_SpriteRenderEditor.OnInspectorGUI();
                }
            }
            if (m_Atlas!=null)
            {
                if(GUILayout.Button(("烘焙")))
                {
                    // 获取标签对应的Sprite列表
                    var spriteList = new System.Collections.Generic.List<Sprite>();
                    var spriteArray = new Sprite[ m_Atlas.spriteCount];
                    m_Atlas.GetSprites(spriteArray);
                    spriteList.AddRange(spriteArray);
                    var selectedSprites = spriteList.FindAll(s => s.name.Replace("(Clone)","").StartsWith(seq.spriteLabel));
                    if (selectedSprites.Count == 0)
                    {
                        Debug.LogError("没有找到对应标签的Sprite");
                        return;
                    }

                    BuildFrames(selectedSprites);
                    EditorUtility.SetDirty(seq);
                }
            }
            if(GUILayout.Button(m_bPreview?"停止播放":"预览播放"))
            {
                m_fPlayTime = 0;
                m_bPreview = !m_bPreview;
            }
        }
        //----------------------------------------------
        void BuildFrames(System.Collections.Generic.List<Sprite> sprites)
        {
            var packables = SpriteAtlasExtensions.GetPackables(m_Atlas);

            System.Collections.Generic.HashSet<string> vSpriteNames = new System.Collections.Generic.HashSet<string>();
            foreach (var db in sprites)
            {
                vSpriteNames.Add(db.name.Replace("(Clone)","").ToLower());
            }

            System.Collections.Generic.List<Sprite> vFrames = new System.Collections.Generic.List<Sprite>();
            foreach (var item in packables)
            {
                if (item is DefaultAsset) // 文件夹
                {
                    string folderPath = AssetDatabase.GetAssetPath(item);
                    string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
                    foreach (var guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                        if (sprite == null) continue;
                        if (!vSpriteNames.Contains(sprite.name.ToLower()))
                            continue;
                        if (sprite != null && !vFrames.Contains(sprite))
                        {
                            vFrames.Add(sprite);
                        }
                    }
                }
                else if (item is Sprite)
                {
                    Sprite sprite = item as Sprite;
                    if (sprite != null )
                    {
                        if (!vSpriteNames.Contains(sprite.name.ToLower()))
                            continue;
                        if (sprite != null && !vFrames.Contains(sprite))
                        {
                            vFrames.Add(sprite);
                        }
                    }
                }
                else if (item is Texture2D)
                {
                    string texPath = AssetDatabase.GetAssetPath(item);
                    var tempSprites = AssetDatabase.LoadAllAssetsAtPath(texPath);
                    foreach (var asset in tempSprites)
                    {
                        if (asset is Sprite sprite)
                        {
                            if (!vSpriteNames.Contains(sprite.name.ToLower()))
                                continue;
                            if (sprite != null && !vFrames.Contains(sprite))
                            {
                                vFrames.Add(sprite);
                            }
                        }
                    }
                }
            }
            SpriteSequence seq = (SpriteSequence)target;

            vFrames.Sort((s1, s2) =>
            {
                int i0 = 0;
                int.TryParse(s1.name.Replace("(Clone)", "").Substring(seq.spriteLabel.Length), out i0);
                int i1 = 0;
                int.TryParse(s2.name.Replace("(Clone)", "").Substring(seq.spriteLabel.Length), out i1);
                return i0 - i1;
            });
            seq.frames = vFrames.ToArray();
        }
        //----------------------------------------------
        void OnUpdate()
        {
            m_pTimer.Update();
            if(m_bPreview)
            {
                SpriteSequence seq = (SpriteSequence)target;
                if (seq.frames == null || seq.frames.Length == 0 || seq.spriteRender == null) return;
                m_fPlayTime += m_pTimer.deltaTime;
                int frameCount = Mathf.FloorToInt(m_fPlayTime * seq.fps);
                if (!seq.loop && frameCount >= seq.frames.Length)
                {
                    m_bPreview = false;
                    return;
                }
                frameCount = frameCount % seq.frames.Length;
                seq.spriteRender.sprite = seq.frames[frameCount];
            }
        }
    }
#endif
}
