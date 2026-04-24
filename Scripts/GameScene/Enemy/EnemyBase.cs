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

        Damageable = GetComponent<Damageable>();
        Damageable.OnDeath += OnDeath;
        Damageable.OnHurt += OnHurt;
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
        ChangeState(new MinionDeadState());
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
    }
}