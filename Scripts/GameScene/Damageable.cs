using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 挂在任何可以受伤的物体上（敌人、靶子、可破坏物等）
/// </summary>
public class Damageable : MonoBehaviour
{
    public int maxHP = 10;
    private int currentHP;

    void Start()
    {
        currentHP = maxHP;
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log(gameObject.name + " 受到 " + damage + " 点伤害，剩余HP: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " 被击毁！");
        Destroy(gameObject);
    }
}
