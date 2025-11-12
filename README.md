# TechCosmos PoolSystem 对象池系统

一个基于 Unity ScriptableObject 的轻量级、类型安全的对象池系统，支持编辑器配置和运行时动态管理。

## ✨ 功能特性

- **🚀 零代码配置** - 通过 Inspector 可视化配置对象池
- **🔒 类型安全** - 泛型约束确保类型正确性
- **📊 性能监控** - 实时查看活跃/闲置对象数量
- **🎮 自动回收** - 内置生命周期管理和自动回收机制
- **🔧 高度可扩展** - 支持自定义创建、回收、销毁逻辑
- **👨‍💻 编辑器友好** - 完整的 Inspector 配置和验证

## 🚀 快速开始

### 1. 创建对象池配置

```csharp
[CreateAssetMenu(fileName = "New Bullet Pool", menuName = "Pool/Bullet Pool")]
public class BulletPool : Pool<Bullet> { }

[CreateAssetMenu(fileName = "New ParticleEffect Pool", menuName = "Pool/ParticleEffect Pool")]
public class ParticleEffectPool : Pool<ParticleEffect> { }
```

**创建步骤：**
- 在 Project 窗口右键 → Create → Pool → 选择对应的池类型
- 配置池参数：
  - **Prefab**: 拖入要池化的预制体
  - **Default Capacity**: 初始容量（推荐10）
  - **Max Size**: 最大容量（推荐100）
  - **Collection Check**: 启用重复回收检查（调试时建议开启）

### 2. 配置 PoolManager

1. 创建空 GameObject，添加 `PoolManager` 组件
2. 将创建好的池配置拖入 `Pools` 列表
3. （可选）设置 `poolRoot` 作为回收对象的统一父节点

### 3. 使用对象池

#### 获取对象：
```csharp
// 获取子弹池
var bulletPool = PoolManager.Instance.GetPool<Bullet, FireBullet>();
FireBullet bullet = bulletPool.Get() as FireBullet;
bullet.transform.position = spawnPosition;
```

#### 回收对象：
```csharp
public class Bullet : MonoBehaviour
{
    protected Pool<Bullet> pool;
    
    protected virtual void Start()
    {
        pool = PoolManager.Instance.GetPool<Bullet, Bullet>();
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // 碰撞后回收
        pool?.Release(this);
    }
}
```

## 📖 核心概念

### Pool<T> 泛型参数说明

**重要规则：** `Pool<T>` 中的 `T` 必须是**父类类型**！

```csharp
// ✅ 正确用法
Pool<Bullet> bulletPool;        // 使用父类
Pool<ParticleEffect> effectPool; // 使用父类

// ❌ 错误用法
Pool<FireBullet> fireBullet;    // 使用了子类！
```

### GetPool<T,U>() 双泛型设计

系统采用双泛型设计，支持子类预制体使用父类SO池资产：

```csharp
// 示例：火焰子弹使用子弹池
var bulletPool = PoolManager.Instance.GetPool<Bullet, FireBullet>();
FireBullet fireBullet = bulletPool.Get() as FireBullet;
```

**参数说明：**
- `T` → 父类类型（池管理的基类）
- `U` → 子类类型（实际要获取的具体类型）

## 🛠️ 最佳实践

### 1. 自动回收模式（推荐用于特效等）

```csharp
public class TimedEffect : MonoBehaviour
{
    public float lifeTime = 2f;
    protected Pool<TimedEffect> pool;
    
    protected virtual void OnEnable()
    {
        StartCoroutine(ReleaseByLifeTime(lifeTime));
    }
    
    protected virtual void Start()
    {
        pool = PoolManager.Instance.GetPool<TimedEffect, TimedEffect>();
    }
    
    private IEnumerator ReleaseByLifeTime(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        pool?.Release(this);
    }
}
```

### 2. 手动回收模式（用于子弹、道具等）

```csharp
public class Projectile : MonoBehaviour
{
    protected Pool<Projectile> pool;
    
    protected virtual void Start()
    {
        pool = PoolManager.Instance.GetPool<Projectile, Projectile>();
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // 碰撞后回收
        pool?.Release(this);
    }
    
    private void OnBecameInvisible()
    {
        // 离开屏幕时回收
        pool?.Release(this);
    }
}
```

### 3. 继承体系使用

```csharp
// 基类
public class Bullet : MonoBehaviour
{
    protected Pool<Bullet> pool;
    
    protected virtual void Initialize()
    {
        pool = PoolManager.Instance.GetPool<Bullet, Bullet>();
    }
}

// 子类 - 火焰子弹
public class FireBullet : Bullet
{
    protected override void Initialize()
    {
        // 使用相同的 Bullet 池，但获取 FireBullet 类型
        pool = PoolManager.Instance.GetPool<Bullet, FireBullet>();
    }
}
```

## 🔧 高级用法

### 自定义池逻辑

```csharp
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
```

### 池事件监听

```csharp
public class EventDrivenPool<T> : Pool<T> where T : Component
{
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
}
```

## 🐛 故障排除

### 常见问题

**Q: 对象池初始化失败**
- 检查 prefab 是否设置
- 确认 PoolManager 的 pools 列表包含对应配置
- 验证预制体是否包含正确的组件

**Q: 对象未正确回收**
- 确认 pool 字段在 Start() 或 Awake() 中正确初始化
- 检查回收逻辑是否被执行（使用 Debug.Log 验证）
- 确保对象没有被意外销毁

**Q: 性能问题**
- 调整 defaultCapacity 减少运行时创建
- 设置合理的 maxSize 防止内存泄漏
- 关闭 collectionCheck 以提升性能（发布版本）

**Q: 类型转换错误**
- 确保 `GetPool<T,U>` 的 T 是父类，U 是子类
- 检查预制体类型与池配置是否匹配

### 调试技巧

```csharp
// 在 PoolManager 中添加调试信息
public void DebugPoolStatus()
{
    foreach (var pool in pools)
    {
        Debug.Log($"{pool.PoolType.Name}: Active={pool.CountActive}, Inactive={pool.CountInactive}");
    }
}
```

## 📋 版本要求

- **Unity**: 2020.3 或更高版本
- **.NET**: 4.x 运行时
- **依赖**: 无外部依赖

## 📝 更新日志

### v1.0.0
- 基于 ScriptableObject 的可视化配置
- 双泛型设计支持继承体系
- 完整的生命周期管理
- 性能监控和调试支持

## 📄 许可证

MIT License - 可自由用于商业项目

---

**提示**: 使用过程中遇到问题，请检查控制台错误信息，大多数问题都有详细的错误提示。