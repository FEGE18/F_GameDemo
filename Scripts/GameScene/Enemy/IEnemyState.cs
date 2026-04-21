using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//状态只是一个"行为契约"，它不需要数据字段，不需要共用实现。
//状态的数据都在 EnemyBase 里，状态只是操作它
public interface IEnemyState
{
    /// <summary>
    /// 进入该状态时调用一次（初始化）
    /// </summary>
    /// <param name="enemy">状态类操作的敌人</param>
    public void Enter(EnemyBase enemy);

    /// <summary>
    /// 每帧调用（做逻辑判断和状态切换）
    /// </summary>
    /// <param name="enemy"></param>
    public void Update(EnemyBase enemy);

    /// <summary>
    /// 离开该状态时调用一次（做清理）
    /// </summary>
    /// <param name="enemy"></param>
    public void Exit(EnemyBase enemy);
}
