using System;
using UnityEngine;
namespace ZJM_PoolSystem
{
    /// <summary>
    /// ����ػ��ࣨ���о���������̳д��ࣩ
    /// </summary>
    public abstract class PoolBase : ScriptableObject
    {
        /// <summary>
        /// ��ȡ��ǰ�ع���Ķ�������
        /// </summary>
        public abstract Type PoolType { get; }

        /// <summary>
        /// ��ն���أ������������ö���
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// ��Ծ��������������ʹ���еĶ���
        /// </summary>
        public abstract int CountActive { get; }

        /// <summary>
        /// ���ö������������еı��ö���
        /// </summary>
        public abstract int CountInactive { get; }
    }
}
