using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponIK : MonoBehaviour
{
    [Header("左手IK目标")]
    public Transform leftHandIKTarget;

    [Header("IK权重(0=纯动画),(1=完全IK)")]
    [Range(0f, 1f)]
    public float ikWeight = 1f;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();

            Animator anim = GetComponent<Animator>();
    var clipInfo = anim.GetCurrentAnimatorClipInfo(0);
    foreach (var clip in clipInfo)
    {
        Debug.Log("当前播放的动画片段: " + clip.clip.name);
    }
    }

    //Animator 在每帧计算完常规动画后、应用到骨骼前的那个间隙调用OnAnimatorIK
    //layerIndex 是当前在处理哪个动画层，你只有 Base Layer 所以不用管它
    //这个回调由 Animator 在每帧IK计算阶段自动调用
    //前提：Animator Controller 的对应的Layer必须勾选 IK Pass
    void OnAnimatorIK(int layerIndex)
    {
        Debug.Log("OnAnimatorIK 被调用了");
        if (animator == null || leftHandIKTarget == null)
            return;

        //设置左手 IK 的权重（位置权重 + 旋转权重）
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikWeight);

        /*把 LeftHandIKTarget 空物体的世界坐标位置和世界旋转传给 IK 求解器
        求解器会自动反推 upperarm_l → lowerarm_l → hand_l 这条骨骼链应该怎么转*/
        //让左手去追 IK 目标的位置和旋转
        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKTarget.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIKTarget.rotation);

    }

}
