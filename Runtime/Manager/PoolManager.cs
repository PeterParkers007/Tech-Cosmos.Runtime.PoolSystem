using System.Collections.Generic;
using UnityEngine;
using ZJM_PoolSystem.Runtime.Utility;
namespace ZJM_PoolSystem.Runtime
{
    /// <summary>
    /// �ع�����������ģʽ��ͳһ�������ж����ʵ����
    /// </summary>
    public class PoolManager : Singleton<PoolManager>
    {
        /// <summary>
        /// ����ע��Ķ�����б��༭�����ֶ�������úõĳ�ʵ����
        /// </summary>
        public List<PoolBase> pools;

        /// <summary>
        /// ��ѡ������ظ��ڵ㣨����ͳһ������ն���ĸ��ڵ㣩
        /// </summary>
        public Transform poolRoot;

        protected override void Awake()
        {
            base.Awake();

            // �Զ��������ڵ㣨��δָ����
            if (poolRoot == null)
            {
                GameObject rootObj = new GameObject("PoolRoot");
                poolRoot = rootObj.transform;
                rootObj.transform.parent = transform; // ��Ϊ���������ӽڵ�
            }
        }

        /// <summary>
        /// �����������µĶ����
        /// </summary>
        /// <param name="pool">��Ҫ��ӵĶ����ʵ��</param>
        public void AddNew(PoolBase pool)
        {
            if (pool != null && !pools.Contains(pool))
            {
                pools.Add(pool);
            }
        }

        /// <summary>
        /// �������ͻ�ȡ��Ӧ�Ķ���أ��������أ�����ݣ�
        /// </summary>
        /// <typeparam name="T">�ع�����������</typeparam>
        /// <returns>��Ӧ�ķ��Ͷ����ʵ��</returns>
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
            Debug.LogError($"δ�ҵ�����[{typeof(T).Name}]�Ķ���أ�����PoolManager����");
            return null;
        }

        /// <summary>
        /// ������ж���أ������л�ʱ���ã�
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
