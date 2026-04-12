using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraAnimator : MonoBehaviour
{
    private Animator animator;
    //动画播放结束后调用的委托函数，用于记录动画播放完成后想做的事
    private UnityAction overAction;
    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
    }

    /// <summary>
    /// 给外部提供的进入游戏的方法
    /// </summary>
    /// <param name="action">传进来的回调函数，在动画播放完后调用</param>
    public void BeginGame(UnityAction action)
    {
        animator.SetTrigger("Begin");
        overAction = action;
    }

    /// <summary>
    /// 给外部提供的回到主菜单的方法
    /// </summary>
    /// <param name="action">传进来的回调函数，在动画播放完后调用</param>
    public void BackMeun(UnityAction action)
    {
        animator.SetTrigger("Back");
        overAction = action;
    }
    //当动画播放完的时候，调用的方法
    public void PlayerOver()
    {
        overAction?.Invoke();
        overAction = null;
    }

}
