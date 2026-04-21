using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionChaseState : IEnemyState
{
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

        //情况1：进入攻击距离 → 切换到攻击状态
        if (dist <= enemy.Stats.attackRange)
        {
            enemy.ChangeState(new MinionAttackState());
            return;
        }
        
        //情况2：玩家跑太远了 → 放弃追击，回到待机
        //为什么不直接用 chaseRange，而是乘以 1.5？
        //这帧：玩家 10.1 米 → 放弃 → 切回 Idle
        //下帧：Idle 的 Update 检测 dist <= chaseRange，10.1 还没超过太多，立刻又切 Chase
        //结果：怪物在两个状态之间每帧来回抖动，这叫状态震荡（State Flicker）
        //乘以 1.5 就是加了一个迟滞区间（Hysteresis）
        if(dist>enemy.Stats.chaseRange*1.5f)
        {
            enemy.ChangeState(new MinionIdleState());
        }
    }

    public void Exit(EnemyBase enemy)
    {
        enemy.AnimCtrl.SetRun(false);
    }
}
