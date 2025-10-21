ZJM PoolSystem 对象池系统
概述
一个基于 Unity ScriptableObject 的轻量级、类型安全的对象池系统，支持编辑器配置和运行时动态管理。

功能特性
🚀 零代码配置 - 通过 Inspector 可视化配置对象池

🔒 类型安全 - 泛型约束确保类型正确性

📊 性能监控 - 实时查看活跃/闲置对象数量

🎮 自动回收 - 内置生命周期管理和自动回收机制

🔧 高度可扩展 - 支持自定义创建、回收、销毁逻辑
快速开始
1. 创建对象池配置
创建自己的池 例如：
[CreateAssetMenu(fileName = "New ParticleEffect Pool", menuName = "Pool/ParticleEffect Pool")]
    
    -----------------------------------------------------------------
    ParticleEffectPool : Pool<ParticleEffect>{}
    -----------------------------------------------------------------
    ParticleEffectPool --> 你的池SO资产


    -----------------------------------------------------------------
    Pool<ParticleEffect>
    -----------------------------------------------------------------
    Pool<T> --> T --> 你需要池生成回收的预制体类
创建完成后

在 Project 窗口右键 → Create → Pool → 选择对应的池类型

例如：
    BulletPool - 子弹对象池

    ParticleEffectPool - 粒子特效池

创建自定义池类型

配置池参数：

Prefab: 拖入要池化的预制体

Default Capacity: 初始容量（推荐10）

Max Size: 最大容量（推荐100）

Collection Check: 启用重复回收检查（调试时建议开启）

2. 配置 PoolManager
创建空 GameObject，添加 PoolManager 组件

将创建好的池配置拖入 Pools 列表

3. 使用对象池
获取对象：
csharp
// 获取粒子特效池
var effectPool = PoolManager.Instance.GetPool<ParticleEffect, MoveCommandEffect>();
MoveCommandEffect effect = effectPool.Get();
effect.transform.position = targetPosition;
！注意！

如果不用var 而是明确池类型的话
Pool<Bullet> bulletPool;  对


Pool<Bullet> fireBullet;  对

Pool<FireBullet> fireBullet; 不对！！！！

Pool<T> 这个T一定只能是父类 一定！一定！

回收对象：
csharp
// 在需要回收的组件中
public class ParticleEffect : MonoBehaviour
{
    protected Pool<ParticleEffect> pool;
    
    protected virtual void Initialize()
    {
        pool = PoolManager.Instance.GetPool<ParticleEffect, ParticleEffect>();
    }
    
    private IEnumerator ReleaseByLifeTime(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        pool.Release(this); // 自动回收
    }
}
核心组件说明
Pool<T>
泛型参数: 必须继承 Component

prefab: 池化的预制体引用

defaultCapacity: 初始预创建对象数量

maxSize: 池的最大容量，超出的对象会被销毁

collectionCheck: 防止重复回收的安全检查

PoolManager
单例模式: 全局访问点

pools: 所有注册的对象池列表(直接在编辑器界面将编辑好的SO池资产拖进去)

poolRoot: 回收对象的统一父节点（可选）

GetPool<T,U>(): 根据类型获取对象池

!关键!
因为GetPool的双泛型设计,
所以支持子类预制体使用父类SO池资产
例如：
    FireParticleEffect : ParticleEffect
    则FireParticleEffect如果需要自己的池 可以直接 在
    Project 窗口右键 → Create → Pool --> ParticleEffectPool
    创建池资产 将FireParticleEffect的预制体拖入Prefab字段即可.
其它同理.

怎么获取目标池?
例如要实现火焰子弹的实例化：
    
    var bulletPool = PoolManager.Instance.GetPool<Bullet,FireBullet>();
    FireBullet fireBullet = bulletPool.Get() as FireBullet;// 安全转换，因为池专门管理FireBullet
    -----------------------------------------------------------------
    GetPool<T,U>();
    -----------------------------------------------------------------
    T --> 父类  
    U --> 子类(若没有子类就跟T一样就行)

最佳实践
1. 对象初始化
csharp
public class Bullet : MonoBehaviour
{
    protected Pool<Bullet> pool;
    
    protected virtual void Start()
    {
        Initialize();
    }
    
    protected virtual void Initialize()
    {
        pool = PoolManager.Instance.GetPool<Bullet, Bullet>();
    }
}
2. 自动回收模式
csharp
public class TimedEffect : MonoBehaviour
{
    public float lifeTime = 2f;
    
    protected virtual void OnEnable()
    {
        StartCoroutine(ReleaseByLifeTime(lifeTime));
    }
    
    private IEnumerator ReleaseByLifeTime(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (pool != null)
            pool.Release(this);
    }
}
3. 手动回收模式
csharp
public class Projectile : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // 碰撞后回收
        pool?.Release(this);
    }
}
高级用法
自定义池逻辑
csharp
public class CustomPool<T> : Pool<T> where T : Component
{
    protected override void OnGet(T obj)
    {
        base.OnGet(obj);
        // 自定义获取逻辑
        obj.GetComponent<Renderer>().material.color = Color.white;
    }
    
    protected override void OnRelease(T obj)
    {
        // 自定义回收逻辑
        obj.GetComponent<Rigidbody>().velocity = Vector3.zero;
        base.OnRelease(obj);
    }
}
池事件监听
csharp
// 在 Pool<T> 子类中添加事件
public event System.Action<T> OnObjectCreated;
public event System.Action<T> OnObjectDestroyed;

protected override T CreateObject()
{
    T obj = base.CreateObject();
    OnObjectCreated?.Invoke(obj);
    return obj;
}

protected override void On_Destroy(T obj)
{
    OnObjectDestroyed?.Invoke(obj);
    base.On_Destroy(obj);
}
故障排除
常见问题
对象池初始化失败

检查 prefab 是否设置

确认 PoolManager 的 pools 列表包含对应配置

对象未正确回收

确认 pool 字段不为 null

检查回收逻辑是否被执行

性能问题

调整 defaultCapacity 减少运行时创建

设置合理的 maxSize 防止内存泄漏

版本要求
Unity 2020.3 或更高版本

.NET 4.x 运行时

许可证
MIT License - 可自由用于商业项目