using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 统一管理哪些玩家和塔可以被攻击的注册表，静态类
/// </summary>
//static class 说明这个类不能被实例化
public static class EnemyTargetRegistry
{
    //static 字段说明这个变量属于类本身而不是某个实例。因为类都是静态的了，成员也必须是 static。
    private static List<Transform> _target = new List<Transform>();

    //那为什么这里不判断重复？因为我们的设计保证了它不会重复注册——每个 TargetRegistrar 在 Awake() 里注册一次，OnDestroy() 里注销一次，
    //生命周期绑定 Unity 组件，天然不会重复。没有问题就不用防御性编程，避免过度设计。
    public static void Register(Transform t) { _target.Add(t); }
    public static void Unregister(Transform t) { _target.Remove(t); }

    //IReadOnlyList 是接口，只暴露只读操作（Count、[] 索引）
    public static IReadOnlyList<Transform> GetAll() => _target;
}
