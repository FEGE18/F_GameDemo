using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionChaseState : IEnemyState
{
    // === 单例模式 ===
    //静态单例：所有敌人共用同一个 Chase 状态对象，避免每次切换状态都 new 造成 GC
    //为什么可以共用？因为 Chase 状态没有实例字段，只是行为逻辑（寻路、检测距离）
    private static MinionChaseState _instance;

    //获取单例实例（懒加载：第一次访问时才创建）
    public static MinionChaseState Instance
    {
        get
        {
            //如果还没创建过，就创建一次（整个游戏生命周期只创建一次）
            if (_instance == null)
            {
                _instance = new MinionChaseState();
            }
            return _instance;
        }
    }

    //私有构造函数：防止外部使用 new MinionChaseState()
    //强制外部只能通过 Instance 属性获取单例
    private MinionChaseState() { }

    // === 状态行为 ===

    public void Enter(EnemyBase enemy)
    {
        // 设置 NavMesh 转向速度（对应配置表里的 roundSpeed）
        enemy.Agent.angularSpeed = enemy.Stats.roundSpeed;
        //开始寻路
        enemy.Agent.SetDestination(enemy.Target.position);
        //播放跑步动画
        enemy.AnimCtrl.SetRun(true);
    }

    public void Update(EnemyBase enemy)
    {
        //每帧更新目标玩家位置
        enemy.Agent.SetDestination(enemy.Target.position);
        float dist = Vector3.Distance(enemy.transform.position, enemy.Target.position);

        //情况1：进入攻击距离 → 切换到攻击状态（使用单例，不再 new）
        if (dist <= enemy.Stats.attackRange)
        {
            enemy.ChangeState(MinionAttackState.Instance);
            return;
        }

        //情况2：玩家跑太远了 → 放弃追击，回到待机（使用单例，不再 new）
        //为什么不直接用 chaseRange，而是乘以 1.5？
        //这帧：玩家 10.1 米 → 放弃 → 切回 Idle
        //下帧：Idle 的 Update 检测 dist <= chaseRange，10.1 还没超过太多，立刻又切 Chase
        //结果：怪物在两个状态之间每帧来回抖动，这叫状态震荡（State Flicker）
        //乘以 1.5 就是加了一个迟滞区间（Hysteresis）
        if(dist>enemy.Stats.chaseRange*1.5f)
        {
            enemy.ChangeState(MinionIdleState.Instance);
        }
    }

    public void Exit(EnemyBase enemy)
    {
        enemy.AnimCtrl.SetRun(false);
    }
}
