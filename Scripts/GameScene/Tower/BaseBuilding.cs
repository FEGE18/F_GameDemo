using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBuilding : MonoBehaviour
{
    private Damageable _damageable;

    private void Awake()
    {
        //得到Damageable脚本
        _damageable = GetComponent<Damageable>();

        //订阅事件
        _damageable.OnDeath += OnBaseDeath;
    }

    private void OnDestroy()
    {
        _damageable.OnDeath -= OnBaseDeath;
    }
    
    private void OnBaseDeath()
    {
        // 广播游戏失败
        GameManager.Instance.GameOver();
    }
}
