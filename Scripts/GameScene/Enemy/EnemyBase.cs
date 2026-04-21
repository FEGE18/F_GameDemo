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
public abstract class EnemyBase : MonoBehaviour
{
    // ─── 模块引用（只读，外部可访问） ───────────────────────
    public EnemyStats   Stats    { get; private set; }
    public NavMeshAgent Agent    { get; private set; }
    public Animator Anim { get; private set; }
    public EnemyCombat      Combat   { get; private set; }
    public EnemyAnimatorCtrl AnimCtrl { get; private set; }

    // ─── 状态机 ──────────────────────────────────────────────
    private IEnemyState _currentState;

    // ─── 感知目标 ─────────────────────────────────────────────
    public Transform Target { get; set; }  // 由外部（GameManager / 刷怪器）赋值

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
}