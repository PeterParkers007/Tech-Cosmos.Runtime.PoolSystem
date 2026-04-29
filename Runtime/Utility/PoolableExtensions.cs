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
        public static string CleanName<T>(this T component ,string name) where T : Component
        {
            string cleanName = name;
            if (cleanName.Contains("(Clone)"))
            {
                cleanName = cleanName.Replace("(Clone)", "").Trim();
                // 同时修正对象名
                name = cleanName;
            }
            return name;
        }
    }
}