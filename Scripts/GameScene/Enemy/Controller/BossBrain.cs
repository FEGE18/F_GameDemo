using UnityEngine;

public class BossBrain: EnemyBase
{
    private int _phase = 1;

    [Header("召唤设置")]
    //小兵预设体
    public GameObject minionPrefab;
    //每次召唤几只
    public int summonCount;


    protected override void Start()
    {
        //必须调用父类的Start方法，会初始化状态机
        base.Start();
        CutsceneManager.Instance.PlayBossIntro(transform);
    }
    protected override IEnemyState GetInitialState()
    {
        //Boss 出生直接进入Idle状态
        return new MinionIdleState();
    }

    protected override void OnHurt()
    {
        base.OnHurt();

        int hp = Damageable.CurrentHP;
        int maxHp = Stats.maxHp;

        //阶段切换
        if (_phase == 1 && hp <= maxHp * 0.5f)
        {
            EnterPhase2();
        }
    }

    private void EnterPhase2()
    {
        //改为2阶段
        _phase = 2;

        //加速：速度提升50%
        Stats.moveSpeed *= 1.5f;
        // NavMeshAgent 的速度要同步更新
        Agent.speed = Stats.moveSpeed;

        //攻击加快：间隔缩短
        Stats.atkInterval *= 0.7f;
        Debug.Log("Boss 进入狂暴阶段！");

        //召唤小兵
        SummonMinions();
    }

    /// <summary>
    /// 召唤小兵的方法
    /// </summary>
    private void SummonMinions()
    {
        for (int i = 0; i < summonCount; i++)
        {
            //在Boss周围随机半径3米内生成
            Vector3 offset = Random.insideUnitSphere * 3f;
            //只在Boss同一水平面生成
            offset.y = 0;
            Vector3 spawnPos = transform.position + offset;

            GameObject minionObj = Instantiate(minionPrefab, spawnPos, Quaternion.identity);

            //找到小兵的EnemyBase，设置攻击目标为最近的塔
            EnemyBase minion = minionObj.GetComponent<EnemyBase>();
            Transform nearestTower = FindNearestTower();
            if (minion != null && nearestTower != null)
            {
                minion.Target = nearestTower;
            }
        }
    }
    
    private Transform FindNearestTower()
    {
        var towers = TowerRegistry.GetAll();
        if (towers.Count == 0) return null;

        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (Transform t in towers)
        {
            float dist = Vector3.Distance(transform.position, t.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = t;
            }
        }
        return nearest;
    }
}
