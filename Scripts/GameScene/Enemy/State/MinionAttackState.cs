using UnityEngine;

public class MinionAttackState : IEnemyState
{
    private float _lastAtkTime = -999f;  // 初始化为很小的值，保证进入状态就能立刻攻击

    public void Enter(EnemyBase enemy)
    {
        // 停下来，面朝玩家（停止寻路，NavMesh 不再推着走）
        enemy.Agent.ResetPath();
    }

    public void Update(EnemyBase enemy)
    {
        // 先转向玩家（攻击时要朝着玩家）
        FaceTarget(enemy);

        float dist = Vector3.Distance(enemy.transform.position, enemy.Target.position);

        // 玩家跑出了攻击距离 → 切回追击
        if (dist > enemy.Stats.attackRange)
        {
            enemy.ChangeState(new MinionChaseState());
            return;
        }

        // 冷却结束 → 执行一次攻击
        if (Time.time - _lastAtkTime >= enemy.Stats.atkInterval)
        {
            _lastAtkTime = Time.time;
            enemy.AnimCtrl.TriggerAtk();
            enemy.Combat.DealDamage();   // 实际伤害由 EnemyCombat 处理
        }
    }

    public void Exit(EnemyBase enemy) { }

    private void FaceTarget(EnemyBase enemy)
    {
        Vector3 dir = (enemy.Target.position - enemy.transform.position);
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