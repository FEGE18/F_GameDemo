using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionIdleState : IEnemyState
{
    // === 单例模式 ===
    //静态单例：所有敌人共用同一个 Idle 状态对象，避免每次切换状态都 new 造成 GC
    //为什么可以共用？因为 Idle 状态没有实例字段，只是行为逻辑（停止寻路、检测玩家）
    private static MinionIdleState _instance;

    //获取单例实例（懒加载：第一次访问时才创建）
    public static MinionIdleState Instance
    {
        get
        {
            //如果还没创建过，就创建一次（整个游戏生命周期只创建一次）
            if (_instance == null)
            {
                _instance = new MinionIdleState();
            }
            return _instance;
        }
    }

    //私有构造函数：防止外部使用 new MinionIdleState()
    //强制外部只能通过 Instance 属性获取单例
    private MinionIdleState() { }

    // === 状态行为 ===

    public void Enter(EnemyBase enemy)
    {
        //待机：停止寻路，播放Idle动画
        enemy.Agent.ResetPath();
        enemy.AnimCtrl.SetRun(false);
    }

    public void Update(EnemyBase enemy)
    {
        //每帧检查：玩家是否进入攻击范围
        if (enemy.Target == null) return;
        float dist = Vector3.Distance(enemy.transform.position, enemy.Target.position);

        if(dist<=enemy.Stats.chaseRange)
        {
            //切换到追击状态（使用单例，不再 new）
            enemy.ChangeState(MinionChaseState.Instance);
        }
    }

    public void Exit(EnemyBase enemy)
    {
        //离开待机状态不需要做任何事情
    }
}
