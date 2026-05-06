using System;
using UnityEngine;

/// <summary>
/// 挂在任何可以受伤的物体上（敌人、靶子、可破坏物等）
/// </summary>
public class Damageable : MonoBehaviour
{
    [Header("非敌人用(敌人血量从 EnemyStats 读)")]
    public int maxHP = 10;

    [Header("飘字预设体")]
    public GameObject damagePopupPrefab;
    //飘字高度偏移
    public float upOffset = 1.5f;

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

        if (damagePopupPrefab != null)
        {
            // 生成位置：怪物脚底 + 向上偏移1.5米（头顶附近）
            Vector3 spawnPos = transform.position + Vector3.up * upOffset +
                                new Vector3(UnityEngine.Random.Range(0.3f, 0.5f), 0, UnityEngine.Random.Range(0.3f, 0.5f));
            GameObject popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);
            popup.GetComponent<DamagePopup>().Init(damage);
        }

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
