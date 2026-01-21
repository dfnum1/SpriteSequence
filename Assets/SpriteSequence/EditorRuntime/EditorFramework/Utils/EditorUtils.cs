/********************************************************************
生成日期:		11:03:2020
类    名: 	EditorUtils
作    者:	HappLI
描    述:	编辑器工具类
*********************************************************************/

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Framework.ED
{
    public class EditorUtils
    {
        //-------------------------------------------------
        public static void GetAppAssemblyTypes(System.Action<System.Type> onTypes)
        {
            if (onTypes == null) return;
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types = null;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types; // 部分可用类型
                                      // 可选：输出警告
                    UnityEngine.Debug.LogWarning($"[GuideSystemEditor] 加载程序集 {assembly.FullName} 时部分类型无法加载: {ex}");
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"[GuideSystemEditor] 加载程序集 {assembly.FullName} 时发生异常: {ex}");
                    continue;
                }
                for (int i = 0; i < types.Length; ++i)
                {
                    System.Type tp = types[i];
                    if (tp == null) continue;
                    onTypes(tp);
                }
            }
        }
        //-------------------------------------------------
        static Dictionary<string, System.Type> ms_vBindTypes = null;
        public static System.Type GetTypeByName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return null;
            if (ms_vBindTypes == null)
            {
                ms_vBindTypes = new Dictionary<string, System.Type>();
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    System.Type[] types = assembly.GetTypes();
                    for (int i = 0; i < types.Length; ++i)
                    {
                        System.Type tp = types[i];
                        if (tp.IsDefined(typeof(DrawProps.BinderTypeAttribute), false))
                        {
                            DrawProps.BinderTypeAttribute attr = (DrawProps.BinderTypeAttribute)tp.GetCustomAttribute(typeof(DrawProps.BinderTypeAttribute));
                            ms_vBindTypes[attr.bindName] = tp;
                        }
                    }
                }
            }
            System.Type returnType;
            if (ms_vBindTypes.TryGetValue(typeName, out returnType))
                return returnType;
            returnType = Type.GetType(typeName);
            if (returnType != null) return returnType;
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                returnType = assembly.GetType(typeName, false, true);
                if (returnType != null) return returnType;
            }
            int index = typeName.LastIndexOf(".");
            if (index > 0 && index + 1 < typeName.Length)
            {
                string strTypeName = typeName.Substring(0, index) + "+" + typeName.Substring(index + 1, typeName.Length - index - 1);
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    returnType = assembly.GetType(strTypeName, false, true);
                    if (returnType != null) return returnType;
                }
            }

            return null;
        }
        //------------------------------------------------------
        public static string GetEnumDisplayName(System.Enum curVar)
        {
            FieldInfo fi = curVar.GetType().GetField(curVar.ToString());
            string strTemName = curVar.ToString();
            if (fi != null && fi.IsDefined(typeof(Framework.DrawProps.DisplayAttribute)))
            {
                strTemName = fi.GetCustomAttribute<Framework.DrawProps.DisplayAttribute>().displayName;
            }
            if (fi != null && fi.IsDefined(typeof(InspectorNameAttribute)))
            {
                strTemName = fi.GetCustomAttribute<InspectorNameAttribute>().displayName;
            }
            return strTemName;
        }
        //------------------------------------------------------
        public static void Destroy(UnityEngine.Object go)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(go);
            else UnityEngine.Object.DestroyImmediate(go);
        }
        //------------------------------------------------------
        public static Texture2D GenerateGridTexture(Color line, Color bg)
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] cols = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    Color col = bg;
                    if (y % 16 == 0 || x % 16 == 0) col = Color.Lerp(line, bg, 0.65f);
                    if (y == 63 || x == 63) col = Color.Lerp(line, bg, 0.35f);
                    cols[(y * 64) + x] = col;
                }
            }
            tex.SetPixels(cols);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "Grid";
            tex.Apply();
            return tex;
        }
        //------------------------------------------------------
        public static Texture2D GenerateCrossTexture(Color line)
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] cols = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    Color col = line;
                    if (y != 31 && x != 31) col.a = 0;
                    cols[(y * 64) + x] = col;
                }
            }
            tex.SetPixels(cols);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "Grid";
            tex.Apply();
            return tex;
        }
        //------------------------------------------------------
        public static bool IsMp4ByHeader(string filePath)
        {
            if (!File.Exists(filePath)) return false;
            byte[] buffer = new byte[32];
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fs.Read(buffer, 0, buffer.Length);
            }
            string header = System.Text.Encoding.ASCII.GetString(buffer);
            return header.Contains("ftyp");
        }
        //------------------------------------------------------
        public static float GetVideoDuration(string filePath)
        {
            if (!File.Exists(filePath)) return 0;
            Assembly TagLibSharp = null;
            foreach (var ass in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                if(ass.GetName().Name == "TagLibSharp")
                {
                    TagLibSharp = ass;
                    break;
                }
            }
            if(TagLibSharp == null)
            {
                string dllFile = "Assets/Plugins/TagLibSharp.dll";
                if (!File.Exists(dllFile))
                {
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("TagLibSharp t:Dll");
                    foreach (string guid in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                        if (path.EndsWith("TagLibSharp.dll", StringComparison.OrdinalIgnoreCase))
                        {
                            dllFile = path;
                            break;
                        }
                    }
                }

                if (!File.Exists(dllFile))
                    return 0;

                TagLibSharp = Assembly.LoadFrom(dllFile);
            }
            
            if (TagLibSharp == null) return 0;

            // 获取 TagLib.File 类型
            var fileType = TagLibSharp.GetType("TagLib.File");
            if (fileType == null) return 0;

            // 调用静态方法 File.Create
            var createMethod = fileType.GetMethod("Create", new[] { typeof(string) });
            if (createMethod == null) return 0;

            var tfile = createMethod.Invoke(null, new object[] { filePath });
            if (tfile == null) return 0;

            // 获取 Properties 属性
            var propertiesProp = fileType.GetProperty("Properties");
            var properties = propertiesProp.GetValue(tfile);

            // 获取 Duration 属性
            var durationProp = properties.GetType().GetProperty("Duration");
            var duration = (TimeSpan)durationProp.GetValue(properties);

            return (float)duration.TotalSeconds;
        }
        //------------------------------------------------------
        public static void CommitGit(string commitFile, bool bCommitListFile = false, bool bWait = true)
        {
            if(bCommitListFile)
            {
                if(!File.Exists(commitFile))
                {
                    EditorUtility.DisplayDialog("提交引导修改", "提交列表文件不存在，请检查路径是否正确。", "确定");
                    return;
                }
            }
            else
            {
                if (!File.Exists(commitFile) && !Directory.Exists(commitFile))
                {
                    return;
                }
            }

            string tortoiseGitExe = "TortoiseGitProc.exe";
            string[] possiblePaths = {
    @"C:\Program Files\TortoiseGit\bin\TortoiseGitProc.exe",
    @"C:\Program Files (x86)\TortoiseGit\bin\TortoiseGitProc.exe"
};
            foreach (var path in possiblePaths)
            {
                if (System.IO.File.Exists(path))
                {
                    tortoiseGitExe = path;
                    break;
                }
            }

            //   if(!System.IO.File.Exists(tortoiseGitExe))
            //   {
            //       UnityEngine.Debug.LogError("TortoiseGitProc.exe 未找到，请确保已安装 TortoiseGit 并正确配置路径。");
            //       return;
            //   }
            if (bCommitListFile)
            {
                string commandStr = $"/command:commit /pathfile:\"{commitFile}\"";
                try
                {
                    Process p = new Process();
                    p.StartInfo.FileName = tortoiseGitExe;
                    p.StartInfo.Arguments = commandStr;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = bWait;
                    p.StartInfo.WorkingDirectory = Application.dataPath;
                    p.Start();
                    if (bWait)
                    {
                        p.BeginOutputReadLine();
                        p.WaitForExit();
                        int exitCode = p.ExitCode;
                        p.Close();
                        p.Dispose();
                        if (exitCode == 0)
                        {
                            //    if(File.Exists(cacheFile))
                            //       File.Delete(cacheFile);
                            //    EditorUtility.DisplayDialog("提交引导修改", "提交成功。", "确定");
                        }
                        else if (exitCode != 0)
                        {
                            EditorUtility.DisplayDialog("提交引导修改", "提交失败。", "确定");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError($"无法启动 TortoiseGit 提交界面: {ex.Message}");
                }
                return;
            }
            try
            {
                commitFile = commitFile.Replace("\\", "/");
                if(commitFile.StartsWith("Assets/"))
                {
                    commitFile = Application.dataPath.Replace("\\", "/") + "/" + commitFile.Substring(7);
                }
                int commitCode = 0;
                string commandStr = $"/command:commit /path:\"{commitFile}\"";
                Process p = new Process();
                p.StartInfo.FileName = tortoiseGitExe;
                p.StartInfo.Arguments = commandStr;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = bWait;
                p.StartInfo.RedirectStandardError = bWait;
                p.StartInfo.WorkingDirectory = Application.dataPath;
                p.Start();
                if(bWait)
                {
                    string output = p.StandardOutput.ReadToEnd();
                    string error = p.StandardError.ReadToEnd();
                    p.WaitForExit();
                    int exitCode = p.ExitCode;
                    p.Close();
                    p.Dispose();
                    commitCode += exitCode;
                    if (commitCode == 0)
                    {
                        if (!string.IsNullOrEmpty(output))
                            EditorUtility.DisplayDialog("提交引导修改", output, "确定");
                    }
                    else if (commitCode != 0)
                    {
                        if (error == null) error = "";
                        EditorUtility.DisplayDialog("提交引导修改", "提交失败。" + "\r\n" + error, "确定");
                    }
                }
                
            }
            catch
            {
                EditorUtility.DisplayDialog("提交引导修改", "提交失败。", "确定");
            }
        }
    }
}
#endif
