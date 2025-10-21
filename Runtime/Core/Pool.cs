using System;
using UnityEngine;
using UnityEngine.Pool;
namespace ZJM_PoolSystem.Runtime
{
    /// <summary>
    /// ���Ͷ���أ�����Unity����ObjectPool��֧�ֱ༭�����ã�
    /// </summary>
    /// <typeparam name="T">�ع����������ͣ�����̳�Component��</typeparam>
    public class Pool<T> : PoolBase where T : Component
    {
        // ʵ�ֻ�������ͱ�ʶ
        public override Type PoolType => typeof(T);

        // ʵ�ֻ���Ķ����������
        public override int CountActive => _pool?.CountActive ?? 0;
        public override int CountInactive => _pool?.CountInactive ?? 0;

        [Header("���������")]
        [Tooltip("��Ҫ�ػ��Ķ���Ԥ�裨�������T���������")]
        public T prefab;

        [Tooltip("�صĳ�ʼ����������ʱ�����ı��ö���������")]
        public int defaultCapacity = 10;

        [Tooltip("�ص���������������󴴽��Ķ����ֱ�����٣������գ�")]
        public int maxSize = 100;

        [Tooltip("�Ƿ�������Ƿ����ڳ��У���ֹ�ظ����գ������ã�")]
        public bool collectionCheck = true;

        // Unity���ö���أ����Ĺ����߼���
        private ObjectPool<T> _pool;

        private bool _isInitialized = false;

        private void OnEnable()
        {
            _isInitialized = false;
            _pool = null;
            Debug.Log($"[{typeof(T).Name}�����] ״̬������");
        }
        /// <summary>
        /// ��ʼ������أ��״λ�ȡ����ʱ�Զ����ã������ֶ����ã�
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized || prefab == null) return;

            // ��ʼ�����ö���أ����ô���/��ȡ/����/���ٵĻص��߼�
            _pool = new ObjectPool<T>(
                createFunc: CreateObject,       // �����¶�����߼�
                actionOnGet: OnGet,             // �ӳػ�ȡ����ʱ���߼�
                actionOnRelease: OnRelease,     // ���ն��󵽳�ʱ���߼�
                actionOnDestroy: On_Destroy,     // ���󳬹��������ʱ�������߼�
                collectionCheck: collectionCheck,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
            _isInitialized = true;
        }

        /// <summary>
        /// �ӳ��л�ȡһ�������Զ���ʼ���أ�
        /// </summary>
        /// <returns>�ػ���T�������ʵ��</returns>
        public T Get()
        {
            Initialize();
            if (_pool == null)
            {
                Debug.LogError($"[{typeof(T).Name}�����] ��ʼ��ʧ�ܣ�����Ԥ���Ƿ���ȷ����");
                return null;
            }
            return _pool.Get();
        }

        /// <summary>
        /// ��������������У��Զ����ض��󣬵ȴ��´λ�ȡ��
        /// </summary>
        /// <param name="obj">��Ҫ���յ�T�������ʵ��</param>
        public void Release(T obj)
        {
            if (_pool == null || obj == null) return;
            _pool.Release(obj);
        }

        /// <summary>
        /// ��ն���أ������������ö��󣬻�Ծ������Ӱ�죩
        /// </summary>
        public override void Clear()
        {
            _pool?.Clear();
        }

        /// <summary>
        /// �����¶��󣨳�Ϊ��ʱ���ã�
        /// </summary>
        protected virtual T CreateObject()
        {
            if (prefab == null)
            {
                Debug.LogError($"[{typeof(T).Name}�����] Ԥ��δ���ã��޷������¶���");
                return null;
            }

            // ʵ����Ԥ�裬��������һ�£����ڵ��ԣ�
            T newObj = Instantiate(prefab);
            newObj.name = prefab.name;
            return newObj;
        }

        /// <summary>
        /// ���󱻻�ȡʱ�Ĵ����������
        /// </summary>
        protected virtual void OnGet(T obj)
        {
            obj.gameObject.SetActive(true);
        }

        /// <summary>
        /// ���󱻻���ʱ�Ĵ������ض���
        /// </summary>
        protected virtual void OnRelease(T obj)
        {
            obj.gameObject.SetActive(false);
            // ��ѡ�������ն����ƶ���ͳһ���ڵ㣨���ⳡ���㼶���ң�
            if (PoolManager.Instance.poolRoot != null)
                obj.transform.SetParent(PoolManager.Instance.poolRoot);
        }

        /// <summary>
        /// ��������ʱ�Ĵ��������������ʱ��
        /// </summary>
        protected virtual void On_Destroy(T obj)
        {
            Destroy(obj.gameObject);
        }

        /// <summary>
        /// �༭��ģʽ����֤���ã���ֹ��Ч������
        /// </summary>
        private void OnValidate()
        {
            // �����������С�ڳ�ʼ��������������������0��
            if (maxSize < defaultCapacity && maxSize > 0)
            {
                maxSize = defaultCapacity;
                Debug.LogWarning($"[{typeof(T).Name}�����] �����������С�ڳ�ʼ���������Զ�����");
            }
        }
    }
}
