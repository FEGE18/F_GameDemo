using System;
using UnityEngine;

/// <summary>
/// 挂在任何可以受伤的物体上（敌人、靶子、可破坏物等）
/// </summary>
public class Damageable : MonoBehaviour
{
    [Header("非敌人用(敌人血量从 EnemyStats 读)")]
    public int maxHP = 10;
    public int CurrentHP { get; private set; }

    //事件：外部订阅，发生时自动收到通知
    public event Action OnDeath;  //死亡时调用
    public event Action OnHurt;   //受伤时调用

    private void Awake()
    {
        //如果同一个 GameObject 上有 EnemyStats，用配置表的血量
        EnemyStats stats = GetComponent<EnemyStats>();
        CurrentHP = stats != null ? stats.maxHp : maxHP;
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (CurrentHP <= 0) return;// 已经死了，不会受伤

        CurrentHP -= damage;

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            //观察者模式：死亡和受伤的逻辑由外部传入，Damageable 本身并不关心自己挂在谁的身上
            OnDeath?.Invoke();
        }
        else
        {
            OnHurt?.Invoke();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " 被击毁！");
        Destroy(gameObject);
    }
}
