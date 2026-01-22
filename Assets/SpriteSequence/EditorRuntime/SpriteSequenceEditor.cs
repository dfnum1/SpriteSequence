#if UNITY_EDITOR
/********************************************************************
生成日期:	01:20:2026
类    名: 	SpriteSequenceEditor
作    者:	HappLI
描    述:	基于Sprite的序列帧动画编辑器
*********************************************************************/
using Framework.ED;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Framework.SpriteSeq
{
    public class SpriteSequenceEditor : EditorWindow
    {
        [MenuItem("GamePlay/精灵序列帧动画编辑器", false, 201)]
        public static void OpenWindow()
        {
            SpriteSequenceEditor window = GetWindow<SpriteSequenceEditor>("精灵序列帧动画编辑器");
            window.minSize = new UnityEngine.Vector2(400, 300);
            window.Show();
        }
        //--------------------------------------------------------
        [UnityEditor.Callbacks.OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj != null && obj is SpriteSequenceData)
            {
                SpriteSequenceEditor window = GetWindow<SpriteSequenceEditor>("精灵序列帧动画编辑器");
                window.minSize = new UnityEngine.Vector2(400, 300);
                window.Show();
                window.m_pData = obj as SpriteSequenceData;
                window.RefreshRender();
                return true;
            }
            return false;
        }
        //------------------------------------------------
        Framework.ED.EditorTimer m_pTimer = new EditorTimer();
        Vector2 m_Scrool = Vector2.zero;
        GUIStyle m_PreviewStyle;
        bool m_bShowCullingSize = true;
        Framework.ED.TargetPreview m_pPreview = null;
        FrameSequenceRenderer m_pRenderer = new FrameSequenceRenderer();
        private SpriteSequenceData m_pData;
        //------------------------------------------------
        private void OnEnable()
        {
            if (m_pPreview == null)
            {
                m_pPreview = new ED.TargetPreview(this);
                GameObject[] roots = new GameObject[1];
                roots[0] = new GameObject("EditorRoot");
                m_pPreview.AddPreview(roots[0]);
                m_pPreview.showFloor = 0.8f;

                m_pPreview.Initialize(roots);
                m_pPreview.SetPreviewInstance(roots[0] as GameObject);
                m_pPreview.OnDrawAfterCB = this.OnPreviewDraw;

                m_pPreview.SetCamera(0.01f, 10000f, 60f);
                m_pPreview.SetCameraPositionAndEulerAngle(new Vector3(-1.85f, 12.44f, -33.77f), new Vector3(19.61f, 2.25f, 0.00f), 9.0f);
            }
            m_pRenderer.Init();
            m_pRenderer.SetData(m_pData);
            RefreshRender();
        }
        //------------------------------------------------
        private void OnDisable()
        {
            m_pRenderer.Destroy();
            m_pPreview.Destroy();
            m_pPreview = null;
        }
        //------------------------------------------------
        void RefreshRender()
        {
            m_pRenderer.ClearSequence();
            if (m_pData == null)
                return;
            int guid = 0;
            m_pRenderer.SetData(m_pData);
            foreach (var db in m_pData.subSequeneces)
            {
                m_pRenderer.AddSequence(Animator.StringToHash(db.label), db.label);
            }
        }
        //------------------------------------------------
        private void Update()
        {
            m_pTimer.Update();
            this.Repaint();
        }
        //------------------------------------------------
        private void OnGUI()
        {
            if (m_pData == null)
            {
                //! 创建新数据
                if (GUILayout.Button("创建"))
                {
                    GUI.FocusControl(null);
                    string savePath = EditorUtility.SaveFilePanelInProject("保存精灵序列帧动画数据", "NewSpriteSequence", "asset", "请为新的精灵序列帧动画数据命名");
                    if (string.IsNullOrEmpty(savePath))
                    {
                        EditorUtility.DisplayDialog("提示", "创建失败！", "好的");
                        return;
                    }
                    m_pData = ScriptableObject.CreateInstance<SpriteSequenceData>();
                    m_pData.name = Path.GetFileNameWithoutExtension(savePath);
                    AssetDatabase.CreateAsset(m_pData, savePath);
                    AssetDatabase.SaveAssetIfDirty(m_pData);

                    RefreshRender();
                }
                return;
            }
            m_pData.altas = EditorGUILayout.ObjectField("精灵图集", m_pData.altas, typeof(SpriteAtlas), false) as SpriteAtlas;
            m_pData.frameRate = EditorGUILayout.IntField("帧率", m_pData.frameRate);
            int seqCnt = m_pData.subSequeneces != null ? m_pData.subSequeneces.Count : 0;
            m_Scrool = GUILayout.BeginScrollView(m_Scrool, GUILayout.Height( Mathf.Max(90, Mathf.Min((seqCnt+2)*25, 200))));
            DrawSubSequences();
            GUILayout.EndScrollView();

            GUILayout.Space(5);
            if (GUILayout.Button("烘焙") && m_pData.altas)
            {
                GUI.FocusControl(null);
                if (!Application.isPlaying)
                {
                    this.ShowNotification(new GUIContent("请先进入Play模式，再进行烘焙操作！"), 2);
                    return;
                }
                SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { m_pData.altas }, EditorUserBuildSettings.activeBuildTarget);

                var sprites = new Sprite[m_pData.altas.spriteCount];
                m_pData.altas.GetSprites(sprites);

                ClearSubAsset(m_pData);
                GeneAtlas(m_pData.altas);
                GeneUvPackTextureSubAsset(sprites);
                // 为索引纹理创建浮点数组
                Color[] indexData = m_pData.vtIndex.GetPixels();

                Color[] pixels = m_pData.vtPack.GetPixels();
                for (int i = 0; i < pixels.Length; ++i)
                {
                    pixels[i] = Color.clear;
                }
                List<Sprite> vSprites = SortSprites(sprites);
                BuildSubSequence(vSprites);
                for (int f = 0; f < vSprites.Count; ++f)
                {
                    var sp = vSprites[f];
                    int n = sp.GetVertexCount();
                    var v = sp.vertices;
                    var indecs = sp.triangles;
                    var u = sp.uv;
                    int drawCnt = m_pData.vertexPerFrame;
                    for (int i = 0; i < drawCnt; ++i)
                    {
                        if (i < n)
                        {
                            Color c = Color.clear;

                            float normalizedX = v[i].x;// (v[i].x + maxX) / (2 * maxX);
                            float normalizedY = v[i].y;// (v[i].y + maxY) / (2 * maxY);
                            c.b = normalizedX;        // 对象空间 x
                            c.a = normalizedY;        // 对象空间 y

                            c.r = u[i].x;        // Atlas-UV.x
                            c.g = u[i].y;        // Atlas-UV.y

                            int index = f * drawCnt + i;
                            pixels[index] = c;
                        }
                    }
                    // 存储索引数据到数组
                    for (int i = 0; i < m_pData.indexPerFrame; ++i)
                    {
                        int index = f * m_pData.indexPerFrame + i;
                        indexData[index] = Color.white * (i < indecs.Length ? indecs[i] : 0);
                    }
                }

                // 应用顶点数据
                m_pData.vtPack.SetPixels(pixels);
                m_pData.vtPack.Apply(false, false);

                // 直接设置索引纹理的浮点数据
                m_pData.vtIndex.SetPixels(indexData);
                m_pData.vtIndex.Apply(false, false);

                Selection.activeObject = m_pData;
                EditorUtility.SetDirty(m_pData);
                AssetDatabase.SaveAssetIfDirty(m_pData);
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("定位"))
            {
                GUI.FocusControl(null);
                EditorUtility.SetDirty(m_pData);
                Selection.activeObject = m_pData;
            }
            if (GUILayout.Button("说明文档"))
            {
                GUI.FocusControl(null);
                Application.OpenURL("https://docs.qq.com/doc/DTGVoRnBxa0NrT2RQ");
            }
            if (GUILayout.Button("刷新视图"))
            {
                GUI.FocusControl(null);
                EditorUtility.SetDirty(m_pData);
                RefreshRender();
            }
            GUILayout.EndHorizontal();

            var lastRect = GUILayoutUtility.GetLastRect();
            if (m_PreviewStyle == null) m_PreviewStyle = new GUIStyle(EditorStyles.textField);
            m_pPreview.OnPreviewGUI(new Rect(0, lastRect.yMax + 10, this.position.width, this.position.height - lastRect.yMax - 10), m_PreviewStyle);
        }
        //--------------------------------------------------------
        void DrawSubSequences()
        {
            if (m_pData == null) return;
            if (m_pData.subSequeneces == null)
                m_pData.subSequeneces = new List<SpriteSequenceData.SubSequence>();
            Color guiColor = GUI.color;
            float centerOffsetWidth = 200;
            float argvHead = (this.position.width - 140- centerOffsetWidth) / 5;
            GUILayout.BeginHorizontal();
            GUILayout.Label("子序列标签", GUILayout.Width(argvHead));
            GUILayout.Label("开始帧", GUILayout.Width(argvHead));
            GUILayout.Label("结束帧", GUILayout.Width(argvHead));
            GUILayout.Label("中心点", GUILayout.Width(centerOffsetWidth));
            GUI.color = m_bShowCullingSize ? Color.green : Color.white;
            if (GUILayout.Button("裁剪大小", GUILayout.Width(argvHead)))
            {
                m_bShowCullingSize = !m_bShowCullingSize;
            }
            GUI.color = guiColor;
            GUILayout.Label("排序层级", GUILayout.Width(argvHead));
            if (GUILayout.Button("+", GUILayout.Width(100)))
            {
                m_pData.subSequeneces.Add(new SpriteSequenceData.SubSequence() { label = "", beginFrame = 0, endFrame = 0 });
            }
            GUILayout.EndHorizontal();
            for (int s = 0; s < m_pData.subSequeneces.Count; ++s)
            {
                var sequence = m_pData.subSequeneces[s];
                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                sequence.label = EditorGUILayout.DelayedTextField(sequence.label, GUILayout.Width(argvHead));
                sequence.beginFrame = EditorGUILayout.DelayedIntField(sequence.beginFrame, GUILayout.Width(argvHead));
                sequence.endFrame = EditorGUILayout.DelayedIntField(sequence.endFrame, GUILayout.Width(argvHead));
                sequence.centerOffset = EditorGUILayout.Vector2Field("", sequence.centerOffset, GUILayout.Width(centerOffsetWidth));
                sequence.cullingSize = EditorGUILayout.FloatField(sequence.cullingSize, GUILayout.Width(argvHead));
                sequence.sortingOrder = EditorGUILayout.IntField(sequence.sortingOrder, GUILayout.Width(argvHead));
                if (EditorGUI.EndChangeCheck())
                    RefreshRender();
                if (GUILayout.Button("-", GUILayout.Width(50)))
                {
                    if (EditorUtility.DisplayDialog("提示", "移除子序列？", "移除", "再想想"))
                    {
                        m_pData.subSequeneces.RemoveAt(s);
                        RefreshRender();
                        GUILayout.EndHorizontal();
                        break;
                    }
                }
                bool isVisible = m_pRenderer.IsVisible(Animator.StringToHash(sequence.label));
                if (GUILayout.Button(isVisible ? "隐藏" : "显示", GUILayout.Width(50)))
                {
                    isVisible = !isVisible;
                    m_pRenderer.SetVisible(Animator.StringToHash(sequence.label), isVisible);
                }
                m_pData.subSequeneces[s] = sequence;
                GUILayout.EndHorizontal();
            }
        }
        //--------------------------------------------------------
        void OnPreviewDraw(int controllerId, Camera camera, Event evt)
        {
            if (m_pRenderer != null)
            {
                m_pRenderer.SetFps(m_pData.frameRate);
                m_pRenderer.EditorRender(m_pTimer.deltaTime, camera, TargetPreview.PreviewCullingLayer);

                if (m_pData != null && m_pData.subSequeneces!=null)
                {
                    for (int i = 0; i < m_pData.subSequeneces.Count; ++i)
                    {
                        var draw = m_pData.subSequeneces[i];
                        if (string.IsNullOrEmpty(draw.label))
                            continue;
                        Vector3 curPos = m_pRenderer.GetWorldMatrix(Animator.StringToHash(draw.label)).GetColumn(3);
                        Vector3 worldPos = curPos + new Vector3(draw.centerOffset.x, draw.centerOffset.y,0);
                        Handles.SphereHandleCap(controllerId, worldPos, Quaternion.identity, 0.5f, EventType.Repaint);
                        float size = draw.cullingSize;
                        if (size > 0 && m_bShowCullingSize)
                        {
                            // 绘制正方形裁剪区域
                            Vector3[] corners = new Vector3[5];
                            corners[0] = worldPos + new Vector3(-size, -size, 0);
                            corners[1] = worldPos + new Vector3(size, -size, 0);
                            corners[2] = worldPos + new Vector3(size, size, 0);
                            corners[3] = worldPos + new Vector3(-size, size, 0);
                            corners[4] = corners[0];
                            Handles.DrawPolyLine(corners);
                        }
                    }
                }
            }
        }
        //--------------------------------------------------------
        List<Sprite> SortSprites(Sprite[] sprites)
        {
            var groupDict = new Dictionary<string, List<(int index, Sprite sprite)>>();

            for (int i = 0; i < sprites.Length; ++i)
            {
                var sp = sprites[i];
                string name = sp.name.Replace("(Clone)", "");
                if (int.TryParse(name, out var namIndex))
                {
                    if (!groupDict.TryGetValue("index", out var list))
                    {
                        list = new List<(int, Sprite)>();
                        groupDict["index"] = list;
                    }
                    list.Add((namIndex, sp));
                }
                else
                {
                    int splitIdx = name.LastIndexOf('_');
                    if (splitIdx < 0)
                    {
                        if (!groupDict.TryGetValue(name, out var list))
                        {
                            list = new List<(int, Sprite)>();
                            groupDict[name] = list;
                        }
                        list.Add((-2, sp));
                    }
                    else
                    {

                        string label = name.Substring(0, splitIdx);
                        string numStr = name.Substring(splitIdx + 1);
                        if (!int.TryParse(numStr, out int num))
                        {
                            num = -1;
                        }
                        if (!groupDict.TryGetValue(label, out var list))
                        {
                            list = new List<(int, Sprite)>();
                            groupDict[label] = list;
                        }
                        list.Add((num, sp));
                    }
                }

            }

            // 2. 每组排序
            foreach (var list in groupDict.Values)
            {
                list.Sort((a, b) => a.index.CompareTo(b.index));
            }
            List<Sprite> vSprites = new List<Sprite>();
            foreach (var db in groupDict)
            {
                foreach (var sub in db.Value)
                    vSprites.Add(sub.sprite);
            }
            return vSprites;
        }
        //--------------------------------------------------------
        void BuildSubSequence(List<Sprite> vSprites)
        {
            for (int s = 0; s < m_pData.subSequeneces.Count; ++s)
            {
                var sequence = m_pData.subSequeneces[s];
                sequence.beginFrame = -1;
                sequence.endFrame = -1;
                for (int i = 0; i < vSprites.Count; ++i)
                {
                    string n1 = vSprites[i].name.Replace("(Clone)", "");
                    if (sequence.beginFrame == -1)
                    {
                        if (n1.StartsWith(sequence.label))
                            sequence.beginFrame = i;
                    }
                    else
                    {
                        if (!n1.StartsWith(sequence.label) && sequence.endFrame == -1)
                            sequence.endFrame = i - 1;
                    }
                    if (sequence.endFrame >= 0 && sequence.beginFrame >= 0)
                        break;
                }
                if (sequence.beginFrame < 0) sequence.beginFrame = 0;
                else
                {
                    if (sequence.endFrame < 0) sequence.endFrame = vSprites.Count - 1;
                }
                if (sequence.endFrame < 0) sequence.endFrame = 0;
                m_pData.subSequeneces[s] = sequence;
            }
        }
        //--------------------------------------------------------
        void GeneAtlas(SpriteAtlas atlas)
        {
            SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { atlas }, EditorUserBuildSettings.activeBuildTarget);

            var packField = m_pData.GetType().GetField("m_atlasTexture", BindingFlags.NonPublic | BindingFlags.Instance);
            if (packField == null)
                return;

            var atlasTex = GetPakAtlasTexture(atlas);
            if (atlasTex == null)
                return;

            int width = atlasTex.width;
            int height = atlasTex.height;

            // 新建一张同尺寸同格式的贴图
            Texture2D atlasClone = new Texture2D(width, height, atlasTex.format, atlasTex.mipmapCount > 1);
            atlasClone.name = "AtlasTexture";
            atlasClone.hideFlags = HideFlags.None;

            Graphics.CopyTexture(atlasTex, atlasClone);

            packField.SetValue(m_pData, atlasClone);

            string path = AssetDatabase.GetAssetPath(m_pData);
            AssetDatabase.AddObjectToAsset(atlasClone, path);
        }
        //--------------------------------------------------------
        void GeneUvPackTextureSubAsset(Sprite[] sprites)
        {
            int maxVtx = 0, maxIdx = 0;
            foreach (var sp in sprites)
            {
                maxVtx = Mathf.Max(maxVtx, sp.GetVertexCount());
                maxIdx = Mathf.Max(maxIdx, sp.triangles.Length);
            }
            m_pData.vertexPerFrame = maxVtx;
            m_pData.indexPerFrame = maxIdx;

            var packField = m_pData.GetType().GetField("m_vtPack", BindingFlags.NonPublic | BindingFlags.Instance);
            if (packField == null)
                return;


            var vt = new Texture2D(GetTexSize(maxVtx), GetTexSize(sprites.Length), TextureFormat.RGBAHalf, false, false);
            vt.name = "vtPack";
            vt.wrapMode = TextureWrapMode.Clamp;
            vt.filterMode = FilterMode.Point;
            vt.SetPixels(new Color[vt.width * vt.height]);
            vt.Apply();
            packField.SetValue(m_pData, vt);
            string path = AssetDatabase.GetAssetPath(m_pData);
            AssetDatabase.AddObjectToAsset(vt, path);


            var vtIndexField = m_pData.GetType().GetField("m_vtIndex", BindingFlags.NonPublic | BindingFlags.Instance);
            if (vtIndexField == null)
                return;

            var vtIdx = new Texture2D(GetTexSize(maxIdx), GetTexSize(sprites.Length), TextureFormat.RFloat, false, false);
            vtIdx.name = "vtIndex";
            vtIdx.wrapMode = TextureWrapMode.Clamp;
            vtIdx.filterMode = FilterMode.Point;
            vtIdx.SetPixels(new Color[vtIdx.width * vtIdx.height]);
            vtIdx.Apply();
            vtIndexField.SetValue(m_pData, vtIdx);
            AssetDatabase.AddObjectToAsset(vtIdx, path);
        }
        //--------------------------------------------------------
        float ToOneFloat(float v1, float v2)
        {
            uint uv1 = math.f32tof16(v1);
            uint uv2 = math.f32tof16(v2);
            uint v = uv1 << 16 | uv2;
            return math.asfloat(v);
        }
        //--------------------------------------------------------
        uint Pack3Index(uint i0, uint i1, uint i2)
        {
            return (i0 & 0xFFFF) | ((i1 & 0xFFFF) << 16) | ((i2 & 0xFFFF) << 24);
        }
        //--------------------------------------------------------
        private int GetTexSize(int spriteCount)
        {
            return spriteCount;
            int size = 1;
            while (true)
            {
                if (size >= spriteCount) break;
                size *= 2;
            }
            return size;
        }
        //--------------------------------------------------------
        void ClearSubAsset(UnityEngine.Object parentAsset)
        {
            if (parentAsset == null) return;

            string assetPath = AssetDatabase.GetAssetPath(parentAsset);
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            foreach (var subAsset in allAssets)
            {
                // 跳过主资源本身
                if (subAsset == parentAsset) continue;

                AssetDatabase.RemoveObjectFromAsset(subAsset);
                UnityEngine.Object.DestroyImmediate(subAsset, true);
            }

            EditorUtility.SetDirty(parentAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        //--------------------------------------------------------
        Texture2D GetPakAtlasTexture(SpriteAtlas spriteAtlas)
        {
            if (!Application.isPlaying)
            {
                if (spriteAtlas != null)
                {
                    var getPreviewTexturesMethod = typeof(UnityEditor.U2D.SpriteAtlasExtensions).GetMethod("GetPreviewTextures", BindingFlags.NonPublic | BindingFlags.Static);

                    if (getPreviewTexturesMethod != null)
                    {
                        Texture2D[] textures = getPreviewTexturesMethod.Invoke(null, new object[] { spriteAtlas }) as Texture2D[];
                        if (textures != null && textures.Length > 0)
                        {
                            return textures[0];
                        }
                    }
                }

            }
            else
            {
                Sprite[] sprits = new Sprite[spriteAtlas.spriteCount];
                spriteAtlas.GetSprites(sprits);
                return sprits[0].texture;
            }
            return null;
        }
    }
}
#endif