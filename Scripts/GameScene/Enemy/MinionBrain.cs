using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionBrain : EnemyBase
{
    protected override IEnemyState GetInitialState()
    {
        return new MinionIdleState();
    }
}
