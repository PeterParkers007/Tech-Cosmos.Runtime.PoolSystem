using System;
using UnityEngine;
using UnityEngine.Pool;
namespace ZJM_PoolSystem.Runtime
{
    /// <summary>
    /// 泛型对象池（基于Unity内置ObjectPool，支持编辑器配置）
    /// </summary>
    /// <typeparam name="T">池管理的组件类型（必须继承Component）</typeparam>
    public class Pool<T> : PoolBase where T : Component
    {
        // 实现基类的类型标识
        public override Type PoolType => typeof(T);

        // 实现基类的对象计数属性
        public override int CountActive => _pool?.CountActive ?? 0;
        public override int CountInactive => _pool?.CountInactive ?? 0;

        [Header("对象池配置")]
        [Tooltip("需要池化的对象预设（必须包含T类型组件）")]
        public T prefab;

        [Tooltip("池的初始容量（启动时创建的备用对象数量）")]
        public int defaultCapacity = 10;

        [Tooltip("池的最大容量（超过后创建的对象会直接销毁，不回收）")]
        public int maxSize = 100;

        [Tooltip("是否检查对象是否已在池中（防止重复回收，调试用）")]
        public bool collectionCheck = true;

        // Unity内置对象池（核心管理逻辑）
        private ObjectPool<T> _pool;

        private bool _isInitialized = false;

        private void OnEnable()
        {
            _isInitialized = false;
            _pool = null;
            Debug.Log($"[{typeof(T).Name}对象池] 状态已重置");
        }
        /// <summary>
        /// 初始化对象池（首次获取对象时自动调用，无需手动调用）
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized || prefab == null) return;

            // 初始化内置对象池，配置创建/获取/回收/销毁的回调逻辑
            _pool = new ObjectPool<T>(
                createFunc: CreateObject,       // 创建新对象的逻辑
                actionOnGet: OnGet,             // 从池获取对象时的逻辑
                actionOnRelease: OnRelease,     // 回收对象到池时的逻辑
                actionOnDestroy: On_Destroy,     // 对象超过最大容量时的销毁逻辑
                collectionCheck: collectionCheck,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
            _isInitialized = true;
        }

        /// <summary>
        /// 从池中获取一个对象（自动初始化池）
        /// </summary>
        /// <returns>池化的T类型组件实例</returns>
        public T Get()
        {
            Initialize();
            if (_pool == null)
            {
                Debug.LogError($"[{typeof(T).Name}对象池] 初始化失败！请检查预设是否正确配置");
                return null;
            }
            return _pool.Get();
        }

        /// <summary>
        /// 将对象回收至池中（自动隐藏对象，等待下次获取）
        /// </summary>
        /// <param name="obj">需要回收的T类型组件实例</param>
        public void Release(T obj)
        {
            if (_pool == null || obj == null) return;
            _pool.Release(obj);
        }

        /// <summary>
        /// 清空对象池（销毁所有闲置对象，活跃对象不受影响）
        /// </summary>
        public override void Clear()
        {
            _pool?.Clear();
        }

        /// <summary>
        /// 创建新对象（池为空时调用）
        /// </summary>
        protected virtual T CreateObject()
        {
            if (prefab == null)
            {
                Debug.LogError($"[{typeof(T).Name}对象池] 预设未设置！无法创建新对象");
                return null;
            }

            // 实例化预设，保持名称一致（便于调试）
            T newObj = Instantiate(prefab);
            newObj.name = prefab.name;
            return newObj;
        }

        /// <summary>
        /// 对象被获取时的处理（激活对象）
        /// </summary>
        protected virtual void OnGet(T obj)
        {
            obj.gameObject.SetActive(true);
        }

        /// <summary>
        /// 对象被回收时的处理（隐藏对象）
        /// </summary>
        protected virtual void OnRelease(T obj)
        {
            obj.gameObject.SetActive(false);
            // 可选：将回收对象移动到统一父节点（避免场景层级混乱）
            if (PoolManager.Instance.poolRoot != null)
                obj.transform.SetParent(PoolManager.Instance.poolRoot);
        }

        /// <summary>
        /// 对象被销毁时的处理（超过最大容量时）
        /// </summary>
        protected virtual void On_Destroy(T obj)
        {
            Destroy(obj.gameObject);
        }

        /// <summary>
        /// 编辑器模式下验证配置（防止无效参数）
        /// </summary>
        private void OnValidate()
        {
            // 最大容量不能小于初始容量（且最大容量需大于0）
            if (maxSize < defaultCapacity && maxSize > 0)
            {
                maxSize = defaultCapacity;
                Debug.LogWarning($"[{typeof(T).Name}对象池] 最大容量不能小于初始容量，已自动修正");
            }
        }
    }
}
