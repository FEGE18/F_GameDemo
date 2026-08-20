using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionDeadState : IEnemyState
{
    // === 单例模式 ===
    //静态单例：所有敌人共用同一个 Dead 状态对象，避免每次切换状态都 new 造成 GC
    //为什么可以共用？因为 Dead 状态没有实例字段，只是行为逻辑（停止移动、播放死亡动画）
    private static MinionDeadState _instance;

    //获取单例实例（懒加载：第一次访问时才创建）
    public static MinionDeadState Instance
    {
        get
        {
            //如果还没创建过，就创建一次（整个游戏生命周期只创建一次）
            if (_instance == null)
            {
                _instance = new MinionDeadState();
            }
            return _instance;
        }
    }

    //私有构造函数：防止外部使用 new MinionDeadState()
    //强制外部只能通过 Instance 属性获取单例
    private MinionDeadState() { }

    // === 状态行为 ===

    public void Enter(EnemyBase enemy)
    {
        enemy.Agent.isStopped = true;
        enemy.AnimCtrl.SetDead(true);
    }

    public void Update(EnemyBase enemy)
    {

    }

    public void Exit(EnemyBase enemy)
    {

    }
}
