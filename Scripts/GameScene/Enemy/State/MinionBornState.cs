using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionBornState : IEnemyState
{
    // === 单例模式 ===
    //静态单例：所有敌人共用同一个 Born 状态对象，避免每次切换状态都 new 造成 GC
    //为什么可以共用？因为 Born 状态没有实例字段，只是行为逻辑（停止移动）
    //所有敌人的 Born 行为都一样，不需要每个敌人一个状态对象
    private static MinionBornState _instance;

    //获取单例实例（懒加载：第一次访问时才创建）
    public static MinionBornState Instance
    {
        get
        {
            //如果还没创建过，就创建一次（整个游戏生命周期只创建一次）
            if (_instance == null)
            {
                _instance = new MinionBornState();
            }
            return _instance;
        }
    }

    //私有构造函数：防止外部使用 new MinionBornState()
    //强制外部只能通过 Instance 属性获取单例
    private MinionBornState() { }

    // === 状态行为 ===

    public void Enter(EnemyBase enemy)
    {
        //出生不移动
        enemy.Agent.isStopped = true;
    }

    public void Update(EnemyBase enemy)
    {

    }

    public void Exit(EnemyBase enemy)
    {
        //出生结束，回复移动
        enemy.Agent.isStopped = false;
    }
}
