using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责敌人的攻击伤害逻辑——找到目标身上的 Damageable 并造成伤害
/// </summary>
public class EnemyCombat : MonoBehaviour
{
    private EnemyBase _brain;
    private void Awake()
    {
        // GetComponent 在同一个 GameObject 上找兄弟组件，不是跨对象依赖
        _brain = GetComponent<EnemyBase>();
    }

    /// <summary>
    /// 由攻击动画的 Animation Event 在打击帧调用
    /// 方法名不能改，Animation Event 里填的字符串要和这里一致
    /// </summary>
    public void AtkEvent()
    {
        if (_brain.Target == null)
            return;

        // 打击帧再做一次距离校验，防止动画起手后玩家已经跑开
        float dist = Vector3.Distance(transform.position, _brain.Target.position);
        if (dist > _brain.Stats.attackRange * 1.2f) return;

        // 找目标身上的 Damageable 组件
        Damageable target = _brain.Target.GetComponent<Damageable>();
        if (target == null) return;

        target.TakeDamage(_brain.Stats.atk);
    }

    public void DeadEvent()
    {
        Destroy(this.gameObject);
    }
}
