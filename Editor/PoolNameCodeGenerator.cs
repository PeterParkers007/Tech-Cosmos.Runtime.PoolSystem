#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using ZJM_PoolSystem.Runtime;
using System.IO;
using System.Text;
using System.Collections.Generic;
using ZJM_PoolSystem;
namespace ZJM_PoolSystem.Editor
{
    public class PoolNameCodeGenerator : EditorWindow
    {
        [MenuItem("Tech-Cosmos/对象池/生成池名称常量类")]
        public static void Generate()
        {
            // 1. 搜索所有 PoolBase 类型的资产
            var guids = AssetDatabase.FindAssets("t:ScriptableObject");
            SortedSet<string> prefabNames = new SortedSet<string>(); // 排序去重

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (asset is PoolBase pool)
                {
                    // 通过反射获取 prefab 字段
                    var prefabField = pool.GetType().GetField("prefab");
                    if (prefabField != null)
                    {
                        var prefab = prefabField.GetValue(pool) as Component;
                        if (prefab != null && !string.IsNullOrEmpty(prefab.name))
                        {
                            prefabNames.Add(prefab.name);
                        }
                    }
                }
            }

            // 2. 生成代码
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// ====================================================");
            sb.AppendLine("// 自动生成的池名称常量类 - " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("// 请勿手动修改此文件，重新生成会覆盖修改");
            sb.AppendLine("// ====================================================");
            sb.AppendLine();
            sb.AppendLine("namespace ZJM_PoolSystem.Generated");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 对象池预制体名称常量（防止拼写错误）");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static class PoolNames");
            sb.AppendLine("    {");

            foreach (var name in prefabNames)
            {
                // 转换非法字符为下划线
                string validName = MakeValidIdentifier(name);
                sb.AppendLine($"        public const string {validName} = \"{name}\";");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            // 3. 写入文件
            string outputPath = "Assets/ZJM_PoolSystem/Generated/PoolNames.cs";
            string directory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();

            Debug.Log($"已生成 PoolNames.cs，包含 {prefabNames.Count} 个池名称常量");
            EditorUtility.DisplayDialog("完成", $"成功生成 {prefabNames.Count} 个池名称常量", "确定");
        }

        // 处理预制体名字中的特殊字符，使其成为合法的 C# 标识符
        private static string MakeValidIdentifier(string name)
        {
            // 简单处理：替换空格、括号、连字符等为下划线（可根据项目规则自定义）
            StringBuilder sb = new StringBuilder();
            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
                else
                    sb.Append('_');
            }
            string result = sb.ToString().Trim('_');

            // 如果以数字开头，加前缀
            if (result.Length > 0 && char.IsDigit(result[0]))
                result = "_" + result;

            return string.IsNullOrEmpty(result) ? "Empty" : result;
        }
    }
}

#endif