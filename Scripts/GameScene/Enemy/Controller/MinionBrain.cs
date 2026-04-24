using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionBrain : EnemyBase
{
    protected override IEnemyState GetInitialState()
    {
        //出生时先进出生状态，动画结束后再切Idle
        return new MinionBornState();
    }
}
