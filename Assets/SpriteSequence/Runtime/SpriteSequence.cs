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
        [System.Serializable]
        public class SubSequence
        {
            public string label;
            public bool loop;
            public float fps = 12f;
#if UNITY_EDITOR
            public SpriteAtlas spriteAtlas;
            public bool bExpand;
#endif
            public Sprite[] frames;
            public bool IsValid()
            {
                return !string.IsNullOrEmpty(label);
            }
        }
        public byte currentSub = 0;

        public SpriteRenderer spriteRender;
        public SubSequence[] subSequences;
        int m_nIndex;
        int m_nSubSeqIndex = -1;
        bool m_bPlaying = false;
        //----------------------------------------------
        void Start()
        {
            if(spriteRender == null) spriteRender = GetComponent<SpriteRenderer>();
            if (subSequences == null || subSequences.Length == 0) { Debug.LogError("没找到帧图"); return; }
            m_nSubSeqIndex = currentSub;
            StartCoroutine(Play());
        }
        //----------------------------------------------
        public void PlaySequence(string label)
        {
            if (subSequences == null || subSequences.Length <= 0)
                return;
            int useIndex = -1;
            for(int i=0; i< subSequences.Length; ++i)
            {
                if (subSequences[i].label.CompareTo(label) ==0)
                {
                    useIndex = i;
                    break;
                }
            }
            if (useIndex == m_nSubSeqIndex)
                return;
            m_nSubSeqIndex = useIndex;
            m_nIndex = 0;
            if(!m_bPlaying) StartCoroutine(Play());
        }
        //----------------------------------------------
        IEnumerator Play()
        {
            if (subSequences == null || subSequences.Length <= 0)
                yield break;

            if (m_nSubSeqIndex < 0 || m_nSubSeqIndex >= subSequences.Length)
                yield break;

                m_bPlaying = true;
            var sequence = subSequences[m_nSubSeqIndex];
            var delay = new WaitForSeconds(1f / Mathf.Max(1,sequence.fps));
            while (true)
            {
                if (subSequences == null)
                    break;
                if (m_nSubSeqIndex < 0 || m_nSubSeqIndex >= subSequences.Length)
                    break;
                sequence = subSequences[m_nSubSeqIndex];

                spriteRender.sprite = sequence.frames[m_nIndex];
                m_nIndex = (m_nIndex + 1) % sequence.frames.Length;
                if (!sequence.loop && m_nIndex == 0) break;
                yield return delay;
            }
            m_bPlaying = false;
        }
    }
    //----------------------------------------------
    //! Editor
    //----------------------------------------------
#if UNITY_EDITOR
    [CustomEditor(typeof(SpriteSequence))]
    public class SpriteSequenceInspectorEditor : Editor
    {
        Framework.ED.EditorTimer m_pTimer = new Framework.ED.EditorTimer();
        bool m_bPreview = false;
        float m_fPlayTime = 0;
        int m_nPreviewSubIndex = 0;
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
            SpriteSequence seq = (SpriteSequence)target;
            if (seq.spriteRender == null)
                seq.spriteRender = seq.GetComponent<SpriteRenderer>();
            if (seq.spriteRender)
                seq.spriteRender.hideFlags |= HideFlags.HideInInspector;

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
            Color color = GUI.color;
            if(seq.subSequences!=null)
            {
                if(GUILayout.Button("新增子序列"))
                {
                    var newSubSeq = new SpriteSequence.SubSequence() { label = "NewSubSeq", loop = true, fps = 12f, frames = new Sprite[0] };
                    var list = new System.Collections.Generic.List<SpriteSequence.SubSequence>(seq.subSequences);
                    list.Add(newSubSeq);
                    seq.subSequences = list.ToArray();
                    EditorUtility.SetDirty(seq);
                }
                for (int i = 0; i < seq.subSequences.Length; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("-", GUILayout.Width(15)))
                    {
                        if(EditorUtility.DisplayDialog("提示", "确认删除该标签序列?","删除", "再想想"))
                        {
                            if (m_nPreviewSubIndex == i)
                            {
                                m_nPreviewSubIndex = -1;
                                m_bPreview = false;
                            }
                            if (seq.currentSub == i)
                            {
                                seq.currentSub = 0xff;
                            }
                            var list = new System.Collections.Generic.List<SpriteSequence.SubSequence>(seq.subSequences);
                            list.RemoveAt(i);
                            seq.subSequences = list.ToArray();
                            EditorUtility.SetDirty(seq);
                            EditorGUILayout.EndHorizontal();
                            break;
                        }
                    }
                    GUI.color = seq.currentSub == i ? Color.green : color;
                    GUILayout.Space(10);
                    seq.subSequences[i].bExpand = EditorGUILayout.Foldout(seq.subSequences[i].bExpand, seq.subSequences[i].label);
                    if (GUILayout.Button("设为默认"))
                    {
                        seq.currentSub = (byte)i;
                    }
                    if (GUILayout.Button(m_bPreview ? "停止播放" : "预览播放"))
                    {
                        m_nPreviewSubIndex = i;
                        m_fPlayTime = 0;
                        m_bPreview = !m_bPreview;
                    }
                    EditorGUILayout.EndHorizontal();
                    GUI.color = color;
                    if (seq.subSequences[i].bExpand)
                    {
                        EditorGUI.indentLevel++;
                        seq.subSequences[i] = DrawSpriteSequene(seq.subSequences[i]);
                        EditorGUI.indentLevel--;
                    }
                    if (string.IsNullOrEmpty(seq.subSequences[i].label))
                    {
                        EditorGUILayout.HelpBox($"标签名不能为空！", MessageType.Error);
                    }
                    else if(seq.subSequences[i].frames!=null)
                    {
                        for(int j= 0;j < seq.subSequences[i].frames.Length; ++j)
                        {
                            if (seq.subSequences[i].frames[j] == null)
                            {
                                EditorGUILayout.HelpBox($"帧序列中第 {j} 帧为空，请重新烘焙！", MessageType.Error);
                                break;
                            }
                        }
                    }
                }
            }
            if (GUILayout.Button("说明文档"))
            {
                Application.OpenURL("https://docs.qq.com/doc/DTGVoRnBxa0NrT2RQ");
            }
        }
        //----------------------------------------------
        SpriteSequence.SubSequence DrawSpriteSequene(SpriteSequence.SubSequence sequence)
        {
            SpriteSequence seq = (SpriteSequence)target;
            sequence.label = EditorGUILayout.TextField("标签", sequence.label);
            sequence.loop = EditorGUILayout.Toggle("循环播放", sequence.loop);
            sequence.fps = EditorGUILayout.FloatField("帧率", sequence.fps);
            EditorGUILayout.LabelField("帧数:", sequence.frames != null ? sequence.frames.Length.ToString() : "0");
            sequence.spriteAtlas = EditorGUILayout.ObjectField("Sprite 图集", sequence.spriteAtlas, typeof(SpriteAtlas), false) as SpriteAtlas;
            if (sequence.spriteAtlas != null)
            {
                if (GUILayout.Button(("烘焙")))
                {
                    // 获取标签对应的Sprite列表
                    var spriteList = new System.Collections.Generic.List<Sprite>();
                    var spriteArray = new Sprite[sequence.spriteAtlas.spriteCount];
                    sequence.spriteAtlas.GetSprites(spriteArray);
                    spriteList.AddRange(spriteArray);
                    var selectedSprites = spriteList.FindAll(s => s.name.Replace("(Clone)", "").StartsWith(sequence.label));
                    if (selectedSprites.Count == 0)
                    {
                        Debug.LogError("没有找到对应标签的Sprite");
                        return sequence;
                    }

                    sequence.frames = BuildFrames(sequence.spriteAtlas, sequence.label, selectedSprites);
                    EditorUtility.SetDirty(seq);
                }
            }
            return sequence;
        }
        //----------------------------------------------
        Sprite[] BuildFrames(SpriteAtlas atlas, string label, System.Collections.Generic.List<Sprite> sprites)
        {
            var packables = SpriteAtlasExtensions.GetPackables(atlas);

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
                int.TryParse(s1.name.Replace("(Clone)", "").Substring(label.Length), out i0);
                int i1 = 0;
                int.TryParse(s2.name.Replace("(Clone)", "").Substring(label.Length), out i1);
                return i0 - i1;
            });
           return vFrames.ToArray();
        }
        //----------------------------------------------
        void OnUpdate()
        {
            m_pTimer.Update();
            if(m_bPreview)
            {
                SpriteSequence seq = (SpriteSequence)target;
                if (seq.subSequences == null|| seq.spriteRender == null)
                    return;
                if (m_nPreviewSubIndex<0 || m_nPreviewSubIndex >= seq.subSequences.Length)
                    return;
                var subSeq = seq.subSequences[m_nPreviewSubIndex];
                if (subSeq.frames == null)
                    return;
                m_fPlayTime += m_pTimer.deltaTime;
                int frameCount = Mathf.FloorToInt(m_fPlayTime * subSeq.fps);
                if (!subSeq.loop && frameCount >= subSeq.frames.Length)
                {
                    m_bPreview = false;
                    return;
                }
                frameCount = frameCount % subSeq.frames.Length;
                seq.spriteRender.sprite = subSeq.frames[frameCount];
            }
        }
    }
#endif
}
