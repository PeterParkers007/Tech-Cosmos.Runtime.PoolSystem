// PoolableExtensions.cs
using System.Reflection;
using UnityEngine;
namespace ZJM_PoolSystem.Runtime
{
    public static class PoolableExtensions
    {
        /// <summary>
        /// 获取对象的池名称（通过反射读取PoolableAttribute）
        /// </summary>
        public static string GetPoolName<T>(this T component) where T : Component
        {
            var attr = typeof(T).GetCustomAttribute<PoolableAttribute>(false);
            return attr?.PoolName;
        }
        /// <summary>
        /// 清理对象名称中的 (Clone) 后缀，并返回清理后的名称
        /// </summary>
        public static string CleanName<T>(this T component) where T : Component
        {
            if (component.name.Contains("(Clone)"))
            {
                component.name = component.name.Replace("(Clone)", "").Trim();
            }
            return component.name;
        }
    }
}