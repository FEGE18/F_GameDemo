using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 同EnemyTargetRegistry
/// 这个是存放可以被攻击的敌人对象，是给防御塔使用的
/// </summary>
public static class EnemyRegistry
{
    private static List<Transform> _target = new List<Transform>();

    public static void Register(Transform t)
    {
        _target.Add(t);
    }

    public static void Unregister(Transform t)
    {
        _target.Remove(t);
    }

    public static IReadOnlyList<Transform> GetAll() => _target;
}
