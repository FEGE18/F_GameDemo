using UnityEngine;

public class MinionAttackState : IEnemyState
{
    // === 单例模式 ===
    //静态单例：所有敌人共用同一个 Attack 状态对象，避免每次切换状态都 new 造成 GC
    //为什么可以共用？因为攻击冷却数据（_lastAtkTime）已经移到 EnemyBase 里了
    //每个敌人有自己的 LastAttackTime，不会互相干扰
    private static MinionAttackState _instance;

    //获取单例实例（懒加载：第一次访问时才创建）
    public static MinionAttackState Instance
    {
        get
        {
            //如果还没创建过，就创建一次（整个游戏生命周期只创建一次）
            if (_instance == null)
            {
                _instance = new MinionAttackState();
            }
            return _instance;
        }
    }

    //私有构造函数：防止外部使用 new MinionAttackState()
    //强制外部只能通过 Instance 属性获取单例
    private MinionAttackState() { }

    // === 状态行为 ===

    public void Enter(EnemyBase enemy)
    {
        // 停下来，面朝玩家（停止寻路，NavMesh 不再推着走）
        enemy.Agent.isStopped = true;  // 完全停止 Agent
        enemy.Agent.ResetPath();       // 清除残余路径
        enemy.Agent.velocity = Vector3.zero;  // ← 强制清掉残余速度
    }

    public void Update(EnemyBase enemy)
    {
        // 先转向玩家（攻击时要朝着玩家）
        FaceTarget(enemy);

        float dist = Vector3.Distance(enemy.transform.position, enemy.Target.position);

        // 玩家跑出了攻击距离 → 切回追击（使用单例，不再 new）
        if (dist > enemy.Stats.attackRange * 1.3f)
        {
            enemy.ChangeState(MinionChaseState.Instance);
            return;
        }

        // 冷却结束 → 执行一次攻击
        //从 EnemyBase 读取上次攻击时间（不再用实例字段）
        //为什么要从 EnemyBase 读？因为状态对象是单例，多个敌人共用
        //如果用实例字段，敌人 A 攻击后，敌人 B 也会受到冷却影响
        if (Time.time - enemy.LastAttackTime >= enemy.Stats.atkInterval)
        {
            //更新 EnemyBase 的攻击时间戳
            enemy.LastAttackTime = Time.time;
            enemy.AnimCtrl.TriggerAtk();

            // 实际伤害由动画事件 AtkEvent() 处理
        }
    }

    public void Exit(EnemyBase enemy)
    {
        enemy.Agent.isStopped = false;  // 恢复 Agent，让 Chase 重新寻路
    }

    private void FaceTarget(EnemyBase enemy)
    {
        Vector3 dir = enemy.Target.position - enemy.transform.position;
        dir.y = 0;  // 只在水平面转向，不仰头俯身
        //magnitude要开根号（√(x²+y²+z²)），计算量大 ； dir.sqrMagnitude直接 x²+y²+z²，省掉开根号
        //"这个向量是不是快零了"这件事，用平方比较完全够，不需要开根号
        if (dir.sqrMagnitude < 0.001f) return;  // 距离极近时不转，防抖

        Quaternion targetRot = Quaternion.LookRotation(dir);
        // RotateTowards 这个 API 用来做平滑且有速度限制的转向 这个API与Slerp的区别是 匀速且准确到达
        enemy.transform.rotation = Quaternion.RotateTowards(
            enemy.transform.rotation,   //当前的旋转
            targetRot,                  //目标旋转
            enemy.Stats.roundSpeed * Time.deltaTime  //这一帧最多转多少度
        );
    }
}