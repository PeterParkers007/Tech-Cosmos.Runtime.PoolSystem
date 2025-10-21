using System;
using UnityEngine;
namespace ZJM_PoolSystem
{
    /// <summary>
    /// 对象池基类（所有具体对象池需继承此类）
    /// </summary>
    public abstract class PoolBase : ScriptableObject
    {
        /// <summary>
        /// 获取当前池管理的对象类型
        /// </summary>
        public abstract Type PoolType { get; }

        /// <summary>
        /// 清空对象池（销毁所有闲置对象）
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// 活跃对象数量（正在使用中的对象）
        /// </summary>
        public abstract int CountActive { get; }

        /// <summary>
        /// 闲置对象数量（池中的备用对象）
        /// </summary>
        public abstract int CountInactive { get; }
    }
}
