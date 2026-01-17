// PoolGeneratorEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using ZJM_PoolSystem.Runtime;

namespace ZJM_PoolSystem.Editor
{
    public class PoolGeneratorEditor : EditorWindow
    {
        private Vector2 scrollPos;
        private List<Type> poolableTypes = new List<Type>();
        private Dictionary<Type, bool> selectedTypes = new Dictionary<Type, bool>();

        [MenuItem("Tools/对象池/生成池类代码")]
        public static void ShowWindow()
        {
            var window = GetWindow<PoolGeneratorEditor>("池代码生成器");
            window.minSize = new Vector2(400, 500);
            window.RefreshPoolableTypes();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("池代码生成器", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("选择要生成池类的组件类型，将自动生成对应的Pool类", MessageType.Info);

            if (GUILayout.Button("刷新可池化类型", GUILayout.Height(30)))
            {
                RefreshPoolableTypes();
            }

            if (GUILayout.Button("全选", GUILayout.Width(60)))
            {
                SelectAll(true);
            }

            if (GUILayout.Button("全不选", GUILayout.Width(60)))
            {
                SelectAll(false);
            }

            EditorGUILayout.Space(10);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach (var type in poolableTypes)
            {
                var attr = GetPoolableAttribute(type);
                string displayName = attr?.DisplayName ?? type.Name;

                if (!selectedTypes.ContainsKey(type))
                    selectedTypes[type] = true;

                selectedTypes[type] = EditorGUILayout.ToggleLeft(
                    $"{displayName} ({type.Name})",
                    selectedTypes[type]
                );

                if (attr != null && !string.IsNullOrEmpty(attr.MenuPath))
                {
                    EditorGUILayout.LabelField($"菜单路径: {attr.MenuPath}", EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(20);

            if (GUILayout.Button("生成选定池类", GUILayout.Height(40)))
            {
                GenerateSelectedPools();
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("生成所有池类", GUILayout.Height(30)))
            {
                SelectAll(true);
                GenerateSelectedPools();
            }
        }

        private void RefreshPoolableTypes()
        {
            poolableTypes.Clear();
            selectedTypes.Clear();

            // 扫描所有程序集中标记了PoolableAttribute的MonoBehaviour子类
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (IsPoolableType(type))
                        {
                            poolableTypes.Add(type);
                        }
                    }
                }
                catch (ReflectionTypeLoadException) { }
            }

            poolableTypes.Sort((a, b) => a.Name.CompareTo(b.Name));
            Debug.Log($"找到 {poolableTypes.Count} 个可池化类型");
        }

        private bool IsPoolableType(Type type)
        {
            // 必须是MonoBehaviour的子类
            if (!typeof(MonoBehaviour).IsAssignableFrom(type))
                return false;

            // 不能是抽象类
            if (type.IsAbstract)
                return false;

            // 必须有PoolableAttribute特性
            return Attribute.IsDefined(type, typeof(PoolableAttribute));
        }

        private PoolableAttribute GetPoolableAttribute(Type type)
        {
            return Attribute.GetCustomAttribute(type, typeof(PoolableAttribute)) as PoolableAttribute;
        }

        private void SelectAll(bool select)
        {
            foreach (var type in poolableTypes)
            {
                selectedTypes[type] = select;
            }
        }

        private void GenerateSelectedPools()
        {
            int count = 0;
            string outputPath = "Assets/PoolSystem/GeneratedPools/";

            // 确保目录存在
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            foreach (var kvp in selectedTypes)
            {
                if (!kvp.Value) continue;

                var type = kvp.Key;
                var attr = GetPoolableAttribute(type);

                if (GeneratePoolClass(type, attr, outputPath))
                {
                    count++;
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("完成", $"成功生成 {count} 个池类文件", "确定");
        }

        private bool GeneratePoolClass(Type componentType, PoolableAttribute attr, string outputPath)
        {
            try
            {
                string className = $"{componentType.Name}Pool";
                string fileName = $"{className}.cs";
                string filePath = Path.Combine(outputPath, fileName);

                // 获取显示名称和菜单路径
                string displayName = attr?.DisplayName ?? $"{componentType.Name}池";
                string menuPath = attr?.MenuPath ?? "Pool/";
                if (!menuPath.EndsWith("/")) menuPath += "/";

                // 生成代码
                string code = GeneratePoolCode(componentType, className, displayName, menuPath);

                // 写入文件
                File.WriteAllText(filePath, code, Encoding.UTF8);

                Debug.Log($"已生成: {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"生成{componentType.Name}的池类失败: {e.Message}");
                return false;
            }
        }

        private string GeneratePoolCode(Type componentType, string className, string displayName, string menuPath)
        {
            string menuName = menuPath + displayName;
            string componentNamespace = componentType.Namespace ?? "ZJM_PoolSystem.Runtime";

            return $@"// ====================================================
// 自动生成的池类 - {DateTime.Now:yyyy-MM-dd HH:mm:ss}
// 请勿手动修改此文件，重新生成会覆盖修改
// ====================================================

using UnityEngine;
using ZJM_PoolSystem.Runtime;

namespace ZJM_PoolSystem.Generated
{{
    [CreateAssetMenu(fileName = ""New {displayName}"", menuName = ""{menuName}"", order = 100)]
    public class {className} : Pool<{componentType.Name}>
    {{
        // 可以在这里添加特定于{componentType.Name}池的逻辑
        // 例如：特定的初始化、获取、回收处理
        
        /* 示例：
        protected override void OnGet({componentType.Name} obj)
        {{
            base.OnGet(obj);
            // {displayName}特有的获取逻辑
        }}
        
        protected override void OnRelease({componentType.Name} obj)
        {{
            // {displayName}特有的回收逻辑
            base.OnRelease(obj);
        }}
        */
        
        // 注意：如果需要自定义逻辑，建议复制此文件到其他位置并修改
        // 否则下次重新生成时会覆盖修改
    }}
}}";
        }
    }
}
#endif