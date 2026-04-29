using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 怪物状态机脚本 从 MonsterInfo 配置表里加载数值，挂在怪物 Prefab 上
/// </summary>
public class EnemyStats : MonoBehaviour
{
    [Header("配置ID")]
    public int monsterId = 1;

    // 运行时数值（从表里读出来填进去）
    //把MonsterInfo的每个字段单独记录下来，是为了方便后续动态修改敌人数据，而不会污染原数据
    [HideInInspector] public int   atk;
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float roundSpeed;
    [HideInInspector] public int   maxHp;
    [HideInInspector] public float atkInterval;
    [HideInInspector] public float chaseRange;
    [HideInInspector] public float attackRange;
    //杀死怪物得到的金钱奖励
    [HideInInspector] public int reward;


    //在Awake里初始化是为了方便其他类在start里调用EnemyStats类
    void Awake()
    {
        //
        MonsterInfo info = GameDataMgr.Instance.monsterInfoList.Find(m => m.id == monsterId);

        if (info == null)
        {
            Debug.LogError($"EnemyStats：找不到 id={monsterId} 的怪物配置！");
            return;
        }

        atk = info.atk;
        moveSpeed   = info.moveSpeed;
        roundSpeed  = info.roundSpeed;
        maxHp       = info.hp;
        atkInterval = info.atkInterval;
        chaseRange  = info.chaseRange;
        attackRange = info.attackRange;
        reward      = info.reward;

    }

}
