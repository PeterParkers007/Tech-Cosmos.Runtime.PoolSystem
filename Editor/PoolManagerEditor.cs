#if UNITY_EDITOR
namespace ZJM_PoolSystem.Editor
{
    using ZJM_PoolSystem;
    using UnityEngine;
    using UnityEditor;
    using ZJM_PoolSystem.Runtime;
    [CustomEditor(typeof(PoolManager))]
    public class PoolManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PoolManager manager = (PoolManager)target;

            EditorGUILayout.Space(10);

            if (GUILayout.Button("一键收集所有 SO 池", GUILayout.Height(30)))
            {
                CollectAllPools(manager);
            }
        }

        private void CollectAllPools(PoolManager manager)
        {
            // 搜索所有 ScriptableObject
            var guids = AssetDatabase.FindAssets("t:ScriptableObject");

            manager.pools.Clear();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                // 只保留 PoolBase 的子类
                if (asset is PoolBase pool)
                {
                    manager.pools.Add(pool);
                }
            }

            EditorUtility.SetDirty(manager);
            Debug.Log($"已收集 {manager.pools.Count} 个对象池");
        }
    }
}

#endif