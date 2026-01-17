// PoolableAttribute.cs
using UnityEngine;

namespace ZJM_PoolSystem.Runtime
{
    /// <summary>
    /// 标记一个组件可以被对象池管理
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class PoolableAttribute : System.Attribute
    {
        /// <summary>
        /// 池的显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 菜单路径
        /// </summary>
        public string MenuPath { get; set; } = "Pool/";

        /// <summary>
        /// 图标（可选）
        /// </summary>
        public string Icon { get; set; }

        public PoolableAttribute() { }

        public PoolableAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}