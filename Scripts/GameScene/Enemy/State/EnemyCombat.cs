using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责敌人的攻击伤害逻辑——使用扫掠检测（Sweep Test）判定武器挥动路径上的碰撞
/// </summary>
public class EnemyCombat : MonoBehaviour
{
    // === 引用 ===

    private EnemyBase _brain;

    // === 攻击检测配置 ===

    [Header("攻击检测配置")]
    //扫掠检测的目标层级：勾选 Player 和 Tower 层，避免打到敌人自己或地面
    //LayerMask 是一个位掩码，用于过滤物理检测时只检测特定层级的碰撞体
    //在 Inspector 中配置，比如勾选 "Player" 和 "Tower"
    public LayerMask targetLayerMask;

    //攻击盒子的尺寸（宽、高、深），单位：米
    //这个盒子代表武器挥动时的碰撞范围
    //Vector3(1.5f, 1.5f, 1f) 表示：宽 1.5 米、高 1.5 米、深 1 米
    //为什么需要配置？不同敌人的武器大小不同（拳头 vs 巨剑）
    public Vector3 attackBoxSize = new Vector3(1.5f, 1.5f, 1f);

    //盒子起点相对敌人的偏移距离，单位：米
    //为什么要偏移？避免盒子包含敌人自己，否则会误伤自己
    //0.5f 表示盒子从敌人前方 0.5 米开始
    public float boxOffsetDistance = 0.5f;

    // === 攻击状态 ===

    //当前是否处于攻击动画的"挥动阶段"
    //为什么需要标记？因为只在挥动阶段才持续检测碰撞，起手和收招阶段不检测
    //由动画事件控制：AtkStart() 设为 true，AtkEnd() 设为 false
    private bool _isAttacking = false;

    //本次攻击已经命中的目标集合
    //为什么用 HashSet？因为需要快速判断"某个目标是否已经被打过"，HashSet.Contains 是 O(1)
    //作用：防止同一次攻击重复伤害同一个目标（比如攻击动画 0.3 秒，每帧检测 18 次，不能造成 18 次伤害）
    private HashSet<Damageable> _hitTargets = new HashSet<Damageable>();

    // ─────────────────────────────────────────────────────────

    private void Awake()
    {
        // GetComponent 在同一个 GameObject 上找兄弟组件，不是跨对象依赖
        _brain = GetComponent<EnemyBase>();

        //检查 LayerMask 是否配置
        //为什么要检查？如果忘记配置，扫掠检测会失效（检测不到任何东西）
        if (targetLayerMask == 0)
        {
            Debug.LogWarning($"[EnemyCombat] {gameObject.name} 的 targetLayerMask 未配置，攻击检测将失效！");
        }
    }

    private void Update()
    {
        //只在攻击状态下每帧检测
        //为什么在 Update 而不是 FixedUpdate？因为动画在 Update 里更新，保持同步
        if (_isAttacking)
        {
            CheckAttackHit();
        }
    }

    // ─── 攻击检测逻辑 ─────────────────────────────────────────

    /// <summary>
    /// 扫掠检测：检测武器挥动路径上的所有碰撞体
    /// 每帧调用（在攻击动画的挥动阶段）
    /// </summary>
    private void CheckAttackHit()
    {
        //如果没有目标，不检测（节省性能）
        if (_brain.Target == null) return;

        // === 计算扫掠起点 ===
        //盒子的起始位置：敌人位置 + 朝向 × 偏移距离
        //比如：敌人在 (0,0,0)，朝向东方(1,0,0)，偏移 0.5 → 起点 (0.5,0,0)
        Vector3 boxOrigin = transform.position + transform.forward * boxOffsetDistance;

        // === 扫掠方向 ===
        //盒子移动的方向：敌人的朝向
        //为什么用 transform.forward？敌人朝哪边，攻击就打哪边（有方向性）
        Vector3 sweepDirection = transform.forward;

        // === 扫掠距离 ===
        //盒子移动的距离：从配置表读取攻击范围
        //为什么要减去偏移距离？因为起点已经前移了 0.5 米，总距离要保持一致
        //比如：attackRange = 2 米，偏移 0.5 米 → 实际扫掠 1.5 米，总距离仍是 2 米
        float sweepDistance = _brain.Stats.attackRange - boxOffsetDistance;

        // === 发射扫掠检测 ===
        //Physics.BoxCastAll：沿着路径发射一个盒形射线，检测路径上的所有碰撞体
        //
        //API 说明：
        //  public static RaycastHit[] BoxCastAll(
        //      Vector3 center,          // 盒子的起始中心位置
        //      Vector3 halfExtents,     // 盒子的半尺寸（注意是"半"，实际大小 × 2）
        //      Vector3 direction,       // 盒子移动的方向（需要是单位向量）
        //      Quaternion orientation,  // 盒子的旋转（跟随敌人旋转）
        //      float maxDistance,       // 盒子移动的最大距离
        //      int layerMask            // 只检测哪些层级（位掩码）
        //  )
        //
        //原理：
        //  1. 在 center 位置创建一个虚拟盒子
        //  2. 盒子沿着 direction 方向移动 maxDistance 距离
        //  3. 检测盒子移动路径上碰到的所有 Collider
        //  4. 返回所有命中结果的数组
        //
        //注意事项：
        //  - halfExtents 是半尺寸，比如传 (1,1,1)，实际盒子是 (2,2,2)
        //  - direction 最好是单位向量（normalized），否则距离计算可能不准
        //  - layerMask 为 0 时会检测所有层级（容易误伤）
        RaycastHit[] hits = Physics.BoxCastAll(
            boxOrigin,                      //起点：敌人前方 0.5 米
            attackBoxSize * 0.5f,           //半尺寸：配置的尺寸除以 2
            sweepDirection,                 //方向：敌人朝向
            transform.rotation,             //旋转：跟随敌人旋转（让盒子和敌人朝向一致）
            sweepDistance,                  //距离：攻击范围 - 偏移距离
            targetLayerMask                 //只检测目标层（Player、Tower）
        );

        // === 处理命中结果 ===
        //遍历所有命中的碰撞体
        foreach (RaycastHit hit in hits)
        {
            //尝试获取碰撞体上的 Damageable 组件
            //为什么用 GetComponent？因为 RaycastHit 只给了 Collider，要找到可伤害的组件
            Damageable target = hit.collider.GetComponent<Damageable>();

            //如果没有 Damageable 组件，跳过（比如碰到了装饰物）
            if (target == null) continue;

            //检查是否已经打过这个目标
            //为什么要检查？因为每帧都检测，同一个目标可能被检测 10+ 次
            //HashSet.Contains 是 O(1) 操作，非常快
            if (_hitTargets.Contains(target)) continue;

            //造成伤害
            target.TakeDamage(_brain.Stats.atk);

            //记录已命中，避免重复伤害
            //HashSet.Add 也是 O(1) 操作
            _hitTargets.Add(target);

            //TODO: 可以在这里播放命中特效
            //PlayHitEffect(hit.point, hit.normal);
        }
    }

    // ─── 动画事件接口 ─────────────────────────────────────────

    /// <summary>
    /// 攻击开始：由攻击动画的起始帧调用（Animation Event）
    /// 在动画编辑器中设置：攻击动画的 0.0 秒添加事件 "AtkStart"
    /// </summary>
    public void AtkStart()
    {
        //进入攻击状态：开始持续检测
        _isAttacking = true;

        //清空已命中列表：新的攻击，所有目标都可以再次被打
        //为什么要清空？因为上一次攻击的命中记录不应该影响这次攻击
        _hitTargets.Clear();
    }

    /// <summary>
    /// 攻击结束：由攻击动画的结束帧调用（Animation Event）
    /// 在动画编辑器中设置：攻击动画的最后一帧（比如 0.6 秒）添加事件 "AtkEnd"
    /// </summary>
    public void AtkEnd()
    {
        //退出攻击状态：停止检测
        _isAttacking = false;

        //注意：不清空 _hitTargets，因为可能需要调试查看命中了哪些目标
        //下次攻击开始时（AtkStart）会清空
    }

    // ─── 其他动画事件 ─────────────────────────────────────────

    /// <summary>
    /// 死亡动画结束时的监听事件
    /// </summary>
    public void DeadEvent()
    {
        Destroy(this.gameObject);
    }

    /// <summary>
    /// 出生动画结束时的监听事件
    /// </summary>
    public void BornOver()
    {
        //出生动画结束后切换到 Idle 状态（使用单例，不再 new，避免 GC）
        _brain.ChangeState(MinionIdleState.Instance);
    }

    // ─── 调试可视化 ─────────────────────────────────────────────

    /// <summary>
    /// 在 Scene 视图中绘制攻击范围（调试用）
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        //只在编辑器中运行时绘制
        if (!Application.isPlaying) return;

        //只在攻击状态下绘制
        if (!_isAttacking) return;

        //计算盒子起点
        Vector3 boxOrigin = transform.position + transform.forward * boxOffsetDistance;

        //计算盒子终点
        float sweepDistance = (_brain != null && _brain.Stats != null)
            ? _brain.Stats.attackRange - boxOffsetDistance
            : 2f;
        Vector3 boxEnd = boxOrigin + transform.forward * sweepDistance;

        //绘制起点盒子（绿色）
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(boxOrigin, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, attackBoxSize);

        //绘制终点盒子（红色）
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(boxEnd, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, attackBoxSize);

        //绘制扫掠路径（黄色线）
        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.DrawLine(boxOrigin, boxEnd);
    }
}
