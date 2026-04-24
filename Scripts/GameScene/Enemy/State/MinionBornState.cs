using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionBornState : IEnemyState
{
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
