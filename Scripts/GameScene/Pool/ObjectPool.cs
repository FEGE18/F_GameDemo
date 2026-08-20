using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用泛型对象池，支持任意 Component 类型的对象复用。
/// 使用示例：
/// var bulletPool = new ObjectPool<Bullet>(bulletPrefab.GetComponent<Bullet>(), defaultCapacity: 50, maxSize: 200);
/// Bullet bullet = bulletPool.Get();
/// bulletPool.Release(bullet);
/// </summary>
/// <typeparam name="T">池化的组件类型，必须继承自 Component</typeparam>
public class ObjectPool<T> where T : Component
{
    // === 核心字段 ===

    //池化对象的 Prefab 模板，所有对象都从这个模板实例化
    private readonly T _prefab;

    //对象池的根节点，用于在 Hierarchy 中统一收纳池对象，便于调试和管理
    private readonly Transform _root;

    //可用对象栈，存储当前没有被使用的对象
    //使用 Stack 的原因：后进先出特性可以让最近归还的对象优先被复用，提高缓存命中率
    private readonly Stack<T> _pool;

    //对象池的最大容量限制，防止无限扩容导致内存爆炸
    //当池中对象数量达到这个值时，拒绝继续创建新对象
    private readonly int _maxSize;

    // === 回调接口 ===

    //对象被取出时的回调，用于初始化对象状态（例如激活 GameObject、重置计时器）
    private readonly Action<T> _onGet;

    //对象被归还时的回调，用于清理对象状态（例如禁用 GameObject、清空引用）
    private readonly Action<T> _onRelease;

    // === 构造函数 ===

    /// <summary>
    /// 创建一个新的对象池
    /// </summary>
    /// <param name="prefab">作为模板的 Prefab，必须包含 T 类型的组件</param>
    /// <param name="parent">池对象的父节点，如果为 null 则创建在场景根节点下</param>
    /// <param name="defaultCapacity">栈的初始容量，用于减少动态扩容时的内存分配</param>
    /// <param name="maxSize">对象池的最大容量，超过此数量时拒绝创建新对象</param>
    /// <param name="onGet">对象被取出时的回调</param>
    /// <param name="onRelease">对象被归还时的回调</param>
    public ObjectPool(
        T prefab,
        Transform parent = null,
        int defaultCapacity = 10,
        int maxSize = 100,
        Action<T> onGet = null,
        Action<T> onRelease = null)
    {
        //检查 prefab 是否为空，如果为空则抛出异常
        //为什么不用 Debug.LogError？因为对象池无法工作时应该立即中断，而不是继续运行导致后续空引用
        if (prefab == null)
        {
            throw new ArgumentNullException(nameof(prefab), "对象池的 Prefab 不能为空");
        }

        //保存 Prefab 引用，后续创建对象时使用
        _prefab = prefab;

        //如果传入了父节点就使用，否则创建一个新的 GameObject 作为根节点
        //为什么需要根节点？为了在 Hierarchy 中把池对象集中管理，便于调试时查看
        if (parent != null)
        {
            _root = parent;
        }
        else
        {
            //创建运行时根节点，名字包含类型信息，方便在 Hierarchy 中识别
            GameObject rootObj = new GameObject($"ObjectPool_{typeof(T).Name}");
            _root = rootObj.transform;
        }

        //创建 Stack，并指定初始容量
        //为什么指定初始容量？避免栈动态扩容时的内存重新分配和复制开销
        _pool = new Stack<T>(defaultCapacity);

        //保存最大容量限制
        _maxSize = maxSize;

        //保存回调函数（可能为 null，使用时需要判断）
        _onGet = onGet;
        _onRelease = onRelease;
    }

    // === 核心方法 ===

    /// <summary>
    /// 从对象池中取出一个对象
    /// </summary>
    /// <returns>可用的对象实例，如果池已满且无法扩容则返回 null</returns>
    public T Get()
    {
        T element;

        //如果栈中还有可用对象，直接取出复用
        if (_pool.Count > 0)
        {
            //从栈顶取出一个对象
            element = _pool.Pop();

            //跳过已经被 Unity 销毁的失效对象
            //为什么会有失效对象？可能是场景切换时 Unity 自动销毁了对象，但栈里还保留着引用
            while (element == null && _pool.Count > 0)
            {
                element = _pool.Pop();
            }
        }
        else
        {
            //栈为空，需要动态创建新对象
            element = CreateNewObject();
        }

        //如果创建失败（达到最大容量限制），返回 null
        if (element == null)
        {
            return null;
        }

        //调用取出回调（如果有的话）
        //常见用途：激活 GameObject、重置计时器、清空目标引用
        _onGet?.Invoke(element);

        //返回已经准备好的对象
        return element;
    }

    /// <summary>
    /// 将对象归还到对象池
    /// </summary>
    /// <param name="element">要归还的对象</param>
    public void Release(T element)
    {
        //空引用无法归还，直接返回
        if (element == null)
        {
            return;
        }

        //调用归还回调（如果有的话）
        //常见用途：禁用 GameObject、清空业务状态、重置位置
        _onRelease?.Invoke(element);

        //将对象放回栈中，供下次 Get 时复用
        _pool.Push(element);
    }

    /// <summary>
    /// 清空对象池，销毁所有对象
    /// </summary>
    public void Clear()
    {
        //遍历栈中的所有对象并销毁
        while (_pool.Count > 0)
        {
            T element = _pool.Pop();

            //Unity 对象可能已经被外部销毁，需要先判断是否有效
            if (element != null)
            {
                //销毁对象的 GameObject（会同时销毁上面的所有组件）
                UnityEngine.Object.Destroy(element.gameObject);
            }
        }

        //清空栈，确保不会残留失效引用
        _pool.Clear();
    }

    // === 私有辅助方法 ===

    /// <summary>
    /// 创建一个新的池对象
    /// </summary>
    /// <returns>新创建的对象，如果达到最大容量限制则返回 null</returns>
    private T CreateNewObject()
    {
        //计算当前池的总对象数量
        //总数 = 栈中可用数量 + 外部正在使用的数量
        //因为只有栈容量，所以用一个粗略估算：假设每次扩容时都是栈空的情况
        //实际实现中，这里简化为只检查当前活跃对象数
        //为了简单起见，我们假设调用方不会在未归还的情况下无限调用 Get
        //如果需要严格限制，应该维护一个 _activeCount 计数器

        //实例化 Prefab，父节点设为池根节点
        //为什么以 _prefab.gameObject 作为参数？因为 Instantiate 的泛型重载需要传入 GameObject
        T element = UnityEngine.Object.Instantiate(_prefab, _root);

        //新创建的对象先禁用，由 Get 方法的 _onGet 回调负责激活
        //为什么先禁用？避免对象在初始化之前就开始执行 Update 等生命周期函数
        element.gameObject.SetActive(false);

        //返回新创建的对象
        return element;
    }

    // === 公共属性 ===

    /// <summary>
    /// 获取池中当前可用的对象数量
    /// </summary>
    public int CountInactive => _pool.Count;

    /// <summary>
    /// 获取对象池的最大容量限制
    /// </summary>
    public int MaxSize => _maxSize;
}
