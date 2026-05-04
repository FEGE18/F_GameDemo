using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理场景中的所有已放置的防御塔，供召唤小兵寻找攻击目标
/// </summary>
public class TowerRegistry : MonoBehaviour
{
    private static List<Transform> _towers = new List<Transform>();

    public static void Register(Transform t) { _towers.Add(t); }
    public static void Unregister(Transform t) { _towers.Remove(t); }

    public static IReadOnlyList<Transform> GetAll() => _towers;
}
