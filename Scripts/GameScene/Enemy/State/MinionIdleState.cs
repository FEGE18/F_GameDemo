using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionIdleState : IEnemyState
{
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
            enemy.ChangeState(new MinionChaseState());
        }
    }

    public void Exit(EnemyBase enemy)
    {
        //离开待机状态不需要做任何事情
    }
}
