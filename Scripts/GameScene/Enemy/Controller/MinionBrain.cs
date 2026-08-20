using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionBrain : EnemyBase
{
    protected override IEnemyState GetInitialState()
    {
        //出生时先进出生状态，动画结束后再切Idle
        //使用单例，不再 new，避免 GC
        return MinionBornState.Instance;
    }
}
