using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BeginPanel : BasePanel
{
    public Button btnStart;
    public Button btnSetting;
    public Button btnQuit;

    public SettingPanelRetractAnimator settingPanel;
    private RectTransform btnSettingRect;

    protected override void Init()
    {
        btnSettingRect = btnSetting.GetComponent<RectTransform>();
#warning BeginPanel : 按钮逻辑还没补完

        btnStart.onClick.AddListener(() =>
        {
            //如果设置面板正在显示，不响应开始按钮
            SettingPanel sp = UIManager.Instance.GetPanel<SettingPanel>();
            if (sp != null && sp.isShow)
            {
                // 取到 SettingPanel 上的动画脚本，播放晃动提示
                SettingPanelRetractAnimator anim = sp.GetComponent<SettingPanelRetractAnimator>();
                if (anim != null)
                    anim.PlayShake();
                return;
            }

            //隐藏开始界面
            UIManager.Instance.HidePanel<BeginPanel>();

            //播放摄像机开始动画，再显示选角面板
            Camera.main.GetComponent<CameraAnimator>().BeginGame(() =>
            {
                //显示选角面板
                print("选角面板");
            });
        });

        btnSetting.onClick.AddListener(() =>
        {
            SettingPanel panel = UIManager.Instance.GetPanel<SettingPanel>();

            // 没开：先开面板
            if (panel == null)
            {
                panel = UIManager.Instance.ShowPanel<SettingPanel>();
                //设置按钮正在显示开启
                panel.isShow = true;

                // 取到你新增的弹回脚本（挂在 SettingPanel 上）
                SettingPanelRetractAnimator anim = panel.GetComponent<SettingPanelRetractAnimator>();
                if (anim != null)
                {
                    anim.SetAnchorButton(btnSettingRect);
                }
                return;
            }

            // 已开：执行弹回再关闭
            SettingPanelRetractAnimator closeAnim = panel.GetComponent<SettingPanelRetractAnimator>();
            if (closeAnim != null && !closeAnim.IsPlaying)
            {
                closeAnim.SetAnchorButton(btnSettingRect);
                closeAnim.PlayRetractToLast(() =>
                {
                    panel.isShow = false;
                    UIManager.Instance.HidePanel<SettingPanel>(false);

                    //保存背景音乐设置
                    GameDataMgr.Instance.SaveMusicData();
                });
            }
        });

        btnQuit.onClick.AddListener(() =>
        {
             //如果设置面板正在显示，不响应开始按钮
            SettingPanel sp = UIManager.Instance.GetPanel<SettingPanel>();
            if (sp != null && sp.isShow)
            {
                // 取到 SettingPanel 上的动画脚本，播放晃动提示
                SettingPanelRetractAnimator anim = sp.GetComponent<SettingPanelRetractAnimator>();
                if (anim != null)
                    anim.PlayShake();
                return;
            }

            //这个API只在游戏打包发布出去之后才有用，在编辑模式下没用
            Application.Quit();

        });

    }
}
