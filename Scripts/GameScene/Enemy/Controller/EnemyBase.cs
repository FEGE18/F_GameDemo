using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 所有敌人的抽象基类——持有模块组件 + 驱动状态机
/// 小兵(MinionBrain)、Boss(BossBrain) 都继承这个类
/// </summary>
[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EnemyCombat))]
[RequireComponent(typeof(EnemyAnimatorCtrl))]
[RequireComponent(typeof(Damageable))]
public abstract class EnemyBase : MonoBehaviour
{
    // ─── 模块引用（只读，外部可访问） ───────────────────────
    //敌人的基本数据，血量，攻击之类的
    public EnemyStats Stats { get; private set; }
    //寻路组件
    public NavMeshAgent Agent { get; private set; }
    //敌人身上的动画状态机
    public Animator Anim { get; private set; }
    //敌人的攻击逻辑脚本
    public EnemyCombat Combat { get; private set; }
    //敌人的动画状态机控制脚本
    public EnemyAnimatorCtrl AnimCtrl { get; private set; }

    // ─── 状态机 ──────────────────────────────────────────────
    //敌人现在的状态，由于所有状态继承于接口IEnemyState，所以可以用父类装
    private IEnemyState _currentState;

    // ─── 感知目标 ─────────────────────────────────────────────
    //敌人的跟踪目标
    public Transform Target { get; set; }  // 由外部（GameManager / 刷怪器）赋值

    //可以被攻击的对象的脚本
    public Damageable Damageable { get; private set; }

    // ─── 状态数据 ─────────────────────────────────────────────
    //上次攻击的时间戳，用于攻击冷却判断
    //为什么放在这里而不是 MinionAttackState？因为状态对象会被多个敌人共用（单例模式）
    //如果放在状态类里，多个敌人会共享同一个 _lastAtkTime，导致攻击节奏错乱
    //初始化为 -999 是为了保证敌人第一次进入攻击状态时能立刻攻击（不用等冷却）
    public float LastAttackTime { get; set; } = -999f;


    // ─────────────────────────────────────────────────────────
    protected virtual void Awake()
    {
        Stats = GetComponent<EnemyStats>();
        Agent = GetComponent<NavMeshAgent>();
        Anim = GetComponent<Animator>();
        Combat   = GetComponent<EnemyCombat>();
        AnimCtrl = GetComponent<EnemyAnimatorCtrl>();

        // NavMeshAgent 速度从配置表里读，不手动填
        // 注意：EnemyStats 的 Awake 先执行，这里 Stats 已经有值了
        Agent.speed = Stats.moveSpeed;

        //配置寻路区域代价（Area Cost），让怪物优先走主干道
        //为什么要配置？NavMesh 默认所有区域代价都是 1.0，不会优先选路径
        //通过降低主干道的代价，A* 算法会优先选择主干道，即使绕路也比直线穿普通地面更"便宜"
        SetupNavMeshAreaCosts();

        Damageable = GetComponent<Damageable>();
        Damageable.OnDeath += OnDeath;
        Damageable.OnHurt += OnHurt;

        //把敌人注册进EnemyRegistry中，方便防御塔寻怪
        EnemyRegistry.Register(this.transform);
    }

    protected virtual void Start()
    {
        // 子类可以重写 Start，在这里切入初始状态
        // 比如 MinionBrain 的 Start 里调用 ChangeState(new IdleState())

        // 自动调用子类声明的初始状态，不需要子类手动 ChangeState
        ChangeState(GetInitialState());

     
    }

    protected virtual void Update()
    {
        // 把每帧的决策权委托给当前状态
        _currentState?.Update(this);
    }

    // ─────────────────────────────────────────────────────────

    /// <summary>
    /// 切换到新状态（会依次调用旧状态 Exit、新状态 Enter）
    /// </summary>
    public void ChangeState(IEnemyState newState)
    {
        _currentState?.Exit(this);
        _currentState = newState;
        _currentState?.Enter(this);
    }

    /// <summary>
    /// 子类必须实现的初始状态（比如小兵 = Idle，Boss = Phase1Idle）
    /// </summary>
    protected abstract IEnemyState GetInitialState();

    /// <summary>
    /// 死亡时由 Damageable 事件触发——切换到死亡状态
    /// 敌人死亡时的逻辑，给子类复写
    /// </summary>
    protected virtual void OnDeath()
    {
        // 奖励金币
        GameManager.Instance.AddMoney(Stats.reward);

        //切换到死亡状态（使用单例，不再 new，避免 GC）
        ChangeState(MinionDeadState.Instance);
    }

    protected virtual void OnHurt()
    {
        AnimCtrl.TriggerWound();
    }
    
    protected virtual void OnDestroy()
    {
        //退订事件
        //Unity 里销毁一个 GameObject 时，C# 的 GC 并不会立刻回收它的托管内存
        //因为 Damageable 的事件委托链里还持有着对 EnemyBase.OnDeath 的引用。
        //如果不退订，长此以往会造成内存泄露
        if (Damageable != null)
        {
            Damageable.OnDeath -= OnDeath;
            Damageable.OnHurt -= OnHurt;
        }

        //在敌人死亡后，要把该对象移出注册表
        EnemyRegistry.Unregister(this.transform);
    }

    // ─── 寻路优化 ─────────────────────────────────────────────

    /// <summary>
    /// 配置 NavMesh 区域代价，让敌人优先走主干道
    /// </summary>
    private void SetupNavMeshAreaCosts()
    {
        //NavMesh 的 Area 系统：
        //- Unity 默认有 3 个 Area：Walkable(0)、Not Walkable(1)、Jump(2)
        //- 可以在 Navigation 窗口的 Areas 标签添加自定义 Area
        //- 每个 Area 有一个 Cost（代价），默认都是 1.0
        //- A* 算法计算路径时：总代价 = 距离 × Area Cost
        //- Cost 越低，越优先选择这条路

        //Area ID 对应关系（需要在 Unity Navigation 窗口配置）：
        //0 = Walkable（普通地面，默认代价 1.0）
        //3 = MainRoad（主干道，我们设为 0.3，降低 70% 代价）
        //4 = Grass（草地，我们设为 2.0，提高 100% 代价）

        //设置普通地面的代价（保持默认）
        //为什么要显式设置？防止其他代码修改了默认值
        Agent.SetAreaCost(0, 1.0f);  //Area 0 = Walkable

        //设置主干道的代价（降低 70%）
        //为什么是 0.3？假设主干道绕路 2 倍距离，代价仍然比直线穿普通地面低：
        //  - 直线穿普通地面：100 米 × 1.0 = 100 代价
        //  - 绕路走主干道：200 米 × 0.3 = 60 代价 ← 更优
        //这样怪物会优先走主干道，即使绕路
        Agent.SetAreaCost(3, 0.3f);  //Area 3 = MainRoad

        //设置草地的代价（提高 100%）
        //为什么提高？模拟草地难走，让怪物避开草地
        //如果场景没有草地 Area，这行不会报错，只是不生效
        Agent.SetAreaCost(4, 2.0f);  //Area 4 = Grass

        //注意：Area ID 3 和 4 需要在 Unity 编辑器中手动创建
        //步骤：
        //1. 打开 Navigation 窗口（Window → AI → Navigation）
        //2. 切换到 Areas 标签
        //3. 添加自定义 Area：MainRoad（ID 3）、Grass（ID 4）
        //4. 在场景中选中主干道的地面模型
        //5. Inspector → Navigation → Navigation Area 选择 MainRoad
        //6. 点击 Bake 重新烘焙 NavMesh
    }
}