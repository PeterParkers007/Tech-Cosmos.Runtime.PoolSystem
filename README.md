# TechCosmos PoolSystem 对象池系统

一个基于 Unity ScriptableObject 的轻量级、类型安全的对象池系统，支持编辑器配置、代码生成和运行时动态管理。

## ✨ 功能特性

- **🚀 零代码配置** - 通过 Inspector 可视化配置对象池
- **🔒 类型安全** - 泛型约束确保类型正确性
- **📊 性能监控** - 实时查看活跃/闲置对象数量
- **🎮 自动回收** - 内置生命周期管理和自动回收机制
- **🔧 高度可扩展** - 支持自定义创建、回收、销毁逻辑
- **👨‍💻 编辑器友好** - 完整的 Inspector 配置和验证
- **⚡ 自动代码生成** - 基于特性标记自动生成池类代码
- **🏷️ 智能扫描** - 自动发现项目中可池化的组件

## 🚀 快速开始

### 1. 标记可池化组件

在需要池化的 MonoBehaviour 类上添加 `[Poolable]` 特性：

```csharp
using ZJM_PoolSystem.Runtime;

[Poolable(DisplayName = "子弹池", MenuPath = "Pool/Combat/")]
public class Bullet : MonoBehaviour
{
    // 子弹逻辑...
}

[Poolable(DisplayName = "特效池", MenuPath = "Pool/Effects/")]
public class ParticleEffect : MonoBehaviour
{
    // 特效逻辑...
}
```

### 2. 自动生成池类代码

打开代码生成工具：
- **菜单路径**: Tools/对象池/生成池类代码
- **功能**: 扫描项目中所有标记了 `[Poolable]` 特性的组件
- **操作**: 选择要生成的类型，点击生成按钮

工具将自动在 `Assets/ZJM_PoolSystem/GeneratedPools/` 目录下生成对应的池类代码：

```csharp
// 自动生成的 BulletPool.cs
using UnityEngine;
using ZJM_PoolSystem.Runtime;

namespace ZJM_PoolSystem.Generated
{
    [CreateAssetMenu(fileName = "New 子弹池", menuName = "Pool/Combat/子弹池", order = 100)]
    public class BulletPool : Pool<Bullet>
    {
        // 可以在这里添加特定于Bullet池的逻辑
    }
}
```

### 3. 创建对象池配置

**方法一（推荐） - 使用生成的池类：**
- 在 Project 窗口右键 → Create → Pool/Combat → 子弹池
- 系统会自动创建 BulletPool 的 ScriptableObject 配置

**方法二 - 手动创建（如需自定义）：**
```csharp
[CreateAssetMenu(fileName = "New Bullet Pool", menuName = "Pool/Bullet Pool")]
public class BulletPool : Pool<Bullet> { }

[CreateAssetMenu(fileName = "New ParticleEffect Pool", menuName = "Pool/ParticleEffect Pool")]
public class ParticleEffectPool : Pool<ParticleEffect> { }
```

**配置池参数：**
- **Prefab**: 拖入要池化的预制体
- **Default Capacity**: 初始容量（推荐10）
- **Max Size**: 最大容量（推荐100）
- **Collection Check**: 启用重复回收检查（调试时建议开启）

### 4. 配置 PoolManager

1. 创建空 GameObject，添加 `PoolManager` 组件
2. 将创建好的池配置拖入 `Pools` 列表
3. （可选）设置 `poolRoot` 作为回收对象的统一父节点

### 5. 使用对象池

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

### PoolableAttribute 特性说明

`[Poolable]` 特性用于标记可以被对象池管理的组件：

```csharp
[Poolable]  // 使用默认设置
public class SimpleObject : MonoBehaviour { }

[Poolable(
    DisplayName = "自定义名称",  // 池的显示名称
    MenuPath = "Pool/Category/", // 创建菜单路径
    Icon = "CustomIcon"          // 图标名称（可选）
)]
public class CustomObject : MonoBehaviour { }
```

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

## 🎨 编辑器工具

### PoolGeneratorEditor 代码生成器

**位置：** Tools/对象池/生成池类代码

**功能：**
1. 自动扫描项目中所有标记了 `[Poolable]` 特性的组件
2. 可视化选择要生成的池类
3. 自动生成带正确命名空间的代码文件
4. 支持自定义显示名称和菜单路径

**快捷键操作：**
- 点击"全选"按钮选择所有类型
- 点击"全不选"取消选择所有类型
- 点击"刷新"重新扫描项目

### 自动命名空间处理

系统智能处理命名空间：
- 如果组件在 `ZJM_PoolSystem.Runtime` 中，直接使用
- 如果组件在其他命名空间，自动添加对应的 `using` 语句
- 生成的池类统一放在 `ZJM_PoolSystem.Generated` 命名空间

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

### 扩展特性配置

```csharp
// 支持自定义容量配置的特性
public class PoolableWithConfigAttribute : PoolableAttribute
{
    public int DefaultCapacity { get; set; } = 10;
    public int MaxSize { get; set; } = 100;
}

// 使用示例
[PoolableWithConfig(
    DisplayName = "重型子弹",
    DefaultCapacity = 5,
    MaxSize = 20
)]
public class HeavyBullet : MonoBehaviour { }
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

**Q: 代码生成失败**
- 确保组件继承自 MonoBehaviour
- 检查是否有编译错误
- 确认组件不是抽象类

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

// 在池类中添加调试日志
protected override void OnGet(T obj)
{
    base.OnGet(obj);
    Debug.Log($"获取对象: {obj.name} (池: {typeof(T).Name})");
}

protected override void OnRelease(T obj)
{
    Debug.Log($"回收对象: {obj.name} (池: {typeof(T).Name})");
    base.OnRelease(obj);
}
```

## 📋 版本要求

- **Unity**: 2020.3 或更高版本
- **.NET**: 4.x 运行时
- **依赖**: 无外部依赖

## 📁 项目结构

```
Assets/
├── ZJM_PoolSystem/
│   ├── Runtime/
│   │   ├── Pool.cs              # 泛型对象池基类
│   │   ├── PoolBase.cs          # 池抽象基类
│   │   ├── PoolManager.cs       # 池管理器
│   │   ├── PoolableAttribute.cs # 池化特性标记
│   │   └── Utility/
│   │       └── Singleton.cs     # 单例基类
│   ├── Editor/
│   │   └── PoolGeneratorEditor.cs  # 代码生成器  
└── YourProject/
    └── Scripts/
        ├── Bullet.cs           # 标记[Poolable]
        └── ParticleEffect.cs   # 标记[Poolable]
    └── GeneratedPools/          # 自动生成的池类
│       ├── BulletPool.cs
│       ├── ParticleEffectPool.cs
│       └── ...
```

## 📝 更新日志

### v2.0.0
- **新增**: 基于特性的自动代码生成系统
- **新增**: `[Poolable]` 特性标记系统
- **新增**: 可视化编辑器工具 PoolGeneratorEditor
- **优化**: 智能命名空间处理
- **优化**: 更完善的错误提示和验证

### v1.0.0
- 基于 ScriptableObject 的可视化配置
- 双泛型设计支持继承体系
- 完整的生命周期管理
- 性能监控和调试支持

## 📄 许可证

MIT License - 可自由用于商业项目

---

**提示**: 使用过程中遇到问题，请检查控制台错误信息，大多数问题都有详细的错误提示。如果代码生成有问题，可以手动创建池类配置。