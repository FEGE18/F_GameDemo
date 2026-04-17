using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationExitState : StateMachineBehaviour
{
    private static readonly int HashIsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int HashFireExitTime = Animator.StringToHash("FireExitTime");
    /// <summary>
    /// StateMachineBehaviour提供的接口，表示进入该动画时的逻辑，只调用一次，参数由引擎传入
    /// </summary>
    /// <param name="animator">角色身上的那个 Animator 组件</param>
    /// <param name="stateInfo">当前状态的运行时信息：名字 Hash、播放进度、时长、是否循环等</param>
    /// <param name="layerIndex">当前在第几层（0=Base Layer, 1=Fire Layer...）</param>
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(HashIsAttacking, true);
    }

    /// <summary>
    /// StateMachineBehaviour提供的接口，表示该动画每一帧播放时的逻辑，每一帧调用
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="stateInfo"></param>
    /// <param name="layerIndex"></param>
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 参数 FireExitTime —— 这个值是外部（武器系统）设的，枪械设 0.45，刀设 0.9。
        float exitTime = animator.GetFloat(HashFireExitTime);

        //stateInfo.normalizedTime 就是"当前动画播到了百分之几"：
        /*normalizedTime 值	含义
        0.0	刚进入，第一帧
        0.5	播了一半
        1.0	刚好播完一遍
        >1.0	非循环动画已播完，卡在最后一帧*/
        //normalizedTime不会在循环后重置为0，而是会一直累加
        if (stateInfo.normalizedTime >= exitTime)
        {
            animator.SetBool(HashIsAttacking, false);
        }
        
    }
}
