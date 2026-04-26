using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// UI面板的基类，所有UI面板都要继承这个类
/// </summary>
public abstract class BasePanel : MonoBehaviour
{

    //专门用于控制面板透明度的组件
    private CanvasGroup _canvasGroup;
    //淡入淡出的速度
    private float _alphaSeed = 10f;

    //当前面板是隐藏还是显示的状态，默认是隐藏的
    public bool IsShow = false;

    //当隐藏完毕后的回调函数，UIManager在调用HideMe方法时会传入一个回调函数，
    // 当面板完全隐藏后，就会调用这个回调函数。在这个回调函数中可以实现一些隐藏完毕后的逻辑，比如销毁面板等等
    private UnityAction hideCallBack = null;
    
    protected virtual void Awake()
    {
        //一开始就获取面板上挂载的组件
        _canvasGroup = GetComponent<CanvasGroup>();
        //如果子组件忘记添加这样一个脚本了，就自动添加一个
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        Init();
    }

    /// <summary>
    /// 注册控件事件的方法，所有的子面板 都需要区注册一些控件事件 
    /// 所以把这个方法抽象出来，让子面板必须实现这个方法
    /// </summary>
    protected abstract void Init();

    /// <summary>
    /// 显示面板的方法，主要用来给UIManager调用 
    /// UIManager在显示面板的时候会调用这个方法，来让面板自己去做一些显示的准备工作，比如淡入等等
    /// </summary>
    public virtual void ShowMe()
    {
        //显示面板时，先把面板的透明度设置为0，然后在update里慢慢淡入，使之变为1
        _canvasGroup.alpha = 0;
        IsShow = true;
    }

    /// <summary>
    /// 隐藏面板的方法，主要用来给UIManager调用
    /// UIManager在隐藏面板的时候会调用这个方法，来让面板自己去做一些隐藏的准备工作，比如淡出等等
    /// </summary>
    public virtual void HideMe(UnityAction callBack = null )
    {
        //隐藏面板时，直接把面板的透明度设置为1，然后在update里慢慢淡出，使之变为0
        _canvasGroup.alpha = 1;
        IsShow = false;

        //让自己的回调函数等于外部传入的回调函数，方便在淡出完成后再update中调用
        hideCallBack = callBack;
    }
    // Update is called once per frame
    void Update()
    {
        //当出于显示状态时，如果透明度还没有达到1，就继续增加透明度，直到达到1为止
        //淡入
        if (IsShow && _canvasGroup.alpha != 1)
        {
            _canvasGroup.alpha += Time.unscaledDeltaTime * _alphaSeed;
            if (_canvasGroup.alpha >= 1)
                _canvasGroup.alpha = 1;
        }
        //淡出
        if (!IsShow && _canvasGroup.alpha != 0)
        {
            _canvasGroup.alpha -= Time.unscaledDeltaTime * _alphaSeed;
            if (_canvasGroup.alpha <= 0)
            {
                _canvasGroup.alpha = 0;
                //当淡出完成后，如果有回调函数，就调用回调函数。方便进行一些操作
                hideCallBack?.Invoke();
            }
        }
        
        
    }
}
