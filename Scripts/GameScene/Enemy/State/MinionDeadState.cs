using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionDeadState : IEnemyState
{
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
