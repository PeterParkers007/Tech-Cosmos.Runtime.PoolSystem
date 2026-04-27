using System.Collections.Generic;
using UnityEngine;
using ZJM_PoolSystem.Runtime.Utility;
using System;
namespace ZJM_PoolSystem.Runtime
{
    /// <summary>
    /// 池管理器（单例模式，统一管理所有对象池实例）
    /// </summary>
    public class PoolManager : Singleton<PoolManager>
    {
        /// <summary>
        /// 所有注册的对象池列表（编辑器中手动添加配置好的池实例）
        /// </summary>
        public List<PoolBase> pools;

        private Dictionary<string, PoolBase> poolByName = new();

        /// <summary>
        /// 可选：对象池根节点（用于统一管理回收对象的父节点）
        /// </summary>
        public Transform poolRoot;

        protected override void Awake()
        {
            base.Awake();

            // 自动创建根节点（若未指定）
            if (poolRoot == null)
            {
                GameObject rootObj = new GameObject("PoolRoot");
                poolRoot = rootObj.transform;
                rootObj.transform.parent = transform; // 作为管理器的子节点
            }
            BuildPoolMap();
        }

        /// <summary>
        /// 向管理器添加新的对象池
        /// </summary>
        /// <param name="pool">需要添加的对象池实例</param>
        public void AddNew(PoolBase pool)
        {
            if (pool != null && !pools.Contains(pool))
            {
                pools.Add(pool);
            }
        }

        private void BuildPoolMap()
        {
            poolByName.Clear();
            foreach (var pool in pools)
            {
                var prefabField = pool.GetType().GetField("prefab");
                if (prefabField == null) continue;

                var prefab = prefabField.GetValue(pool) as Component;
                if (prefab != null && !poolByName.ContainsKey(prefab.name))
                {
                    poolByName[prefab.name] = pool;
                }
            }
        }
        /// <summary>
        /// 根据名字 + 类型精确获取池（一个预制体 → 唯一池）
        /// </summary>
        public Pool<T> GetPool<T>(string prefabName) where T : Component
        {
            var pool = GetPoolByPrefabName(prefabName);
            if(pool == null)
            {
                Debug.LogError($"[Name]未找到预制体[{prefabName}]对应的{typeof(T).Name}池");
                return null;
            }
            if (pool is Pool<T> typedPool)
                return typedPool;

            Debug.LogError($"未找到预制体[{prefabName}]对应的{typeof(T).Name}池");
            return null;
        }
        /// <summary>
        /// 根据预制体名字获取池
        /// </summary>
        public PoolBase GetPoolByPrefabName(string prefabName)
        {
            poolByName.TryGetValue(prefabName, out var pool);
            return pool;
        }

        /// <summary>
        /// 回收对象到它对应的池
        /// </summary>
        public void ReleaseByPrefabName(Component instance)
        {
            var pool = GetPoolByPrefabName(instance.name);
            if (pool != null)
            {
                var releaseMethod = pool.GetType().GetMethod("Release");
                releaseMethod?.Invoke(pool, new object[] { instance });
            }
        }

        /// <summary>
        /// 根据组件类型获取对象池
        /// </summary>
        public Pool<T> GetPool<T>() where T : Component
        {
            foreach (var pool in pools)
            {
                if (pool is Pool<T> targetPool)
                    return targetPool;
            }
            Debug.LogError($"未找到管理[{typeof(T).Name}]的对象池");
            return null;
        }

        /// <summary>
        /// 根据预设子类型获取对象池（返回精确类型，无需强转）
        /// </summary>
        public Pool<U> GetPool<T, U>() where T : Component where U : T
        {
            foreach (var pool in pools)
            {
                if (pool is Pool<U> targetPool && targetPool.prefab is U)
                    return targetPool;
            }
            Debug.LogError($"未找到管理[{typeof(U).Name}]的对象池");
            return null;
        }

        /// <summary>
        /// 根据组件类型和预设类型动态获取对象池（运行时版本）
        /// </summary>
        /// <param name="componentType">组件类型（如 typeof(Projectile)）</param>
        /// <param name="prefabType">预设的具体类型（如 typeof(ArrowProjectile)）</param>
        /// <returns>匹配的对象池</returns>
        public PoolBase GetPool(Type componentType, Type prefabType)
        {
            foreach (var pool in pools)
            {
                // 检查池的类型是否匹配 componentType
                if (pool.PoolType == componentType)
                {
                    // 通过反射获取 prefab 字段的值
                    var prefabField = pool.GetType().GetField("prefab");
                    if (prefabField != null)
                    {
                        var prefab = prefabField.GetValue(pool) as Component;
                        if (prefab != null && prefab.GetType() == prefabType)
                        {
                            return pool;
                        }
                    }
                }
            }
            
            Debug.LogError($"未找到管理 [{componentType.Name}] 且预设类型为 [{prefabType.Name}] 的对象池");
            return null;
        }

        /// <summary>
        /// 泛型便捷包装（用于已知 componentType 的情况）
        /// </summary>
        public Pool<T> GetPoolByPrefabType<T>(Type prefabType) where T : Component
        {
            return GetPool(typeof(T), prefabType) as Pool<T>;
        }

        /// <summary>
        /// 清空所有对象池（场景切换时调用）
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in pools)
            {
                pool.Clear();
            }
        }
    }
}
