using System.Collections.Generic;
using UnityEngine;
using ZJM_PoolSystem.Runtime.Utility;
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

        /// <summary>
        /// 根据类型获取对应的对象池（泛型重载，更便捷）
        /// </summary>
        /// <typeparam name="T">池管理的组件类型</typeparam>
        /// <returns>对应的泛型对象池实例</returns>
        public Pool<T> GetPool<T, U>() where T : Component
        {
            foreach (var pool in pools)
            {
                if (pool is Pool<T> targetPool)
                {
                    if (targetPool.prefab.GetType() == typeof(U))
                    {
                        return targetPool;
                    }
                }
            }
            Debug.LogError($"未找到管理[{typeof(T).Name}]的对象池，请检查PoolManager配置");
            return null;
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
