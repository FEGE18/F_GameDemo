using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 集中管理所有 Animator 参数的调用，避免字符串散落在各个状态类里
/// 主要用来统一管理Animator Controllor 
/// </summary>
[RequireComponent(typeof(Animator))]
public class EnemyAnimatorCtrl : MonoBehaviour
{
    private Animator _anim;

    // ── 提前把字符串转成 Hash，性能更好 ──
    private static readonly int HashRun   = Animator.StringToHash("Run");
    private static readonly int HashDead  = Animator.StringToHash("Dead");
    private static readonly int HashWound = Animator.StringToHash("Wound");
    private static readonly int HashAtk = Animator.StringToHash("Atk");

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    //给外部提供的改变动画状态机的方法
    //统一通过这些方法来改，不硬编码变量名，方便修改和管理。这样如果增加或修改变量名只需要在这里改，不用满世界找
    public void SetRun(bool value) => _anim.SetBool(HashRun, value);
    public void SetDead(bool value) => _anim.SetBool(HashDead, value);
    public void TriggerWound() => _anim.SetTrigger(HashWound);
    public void TriggerAtk() => _anim.SetTrigger(HashAtk);
}
