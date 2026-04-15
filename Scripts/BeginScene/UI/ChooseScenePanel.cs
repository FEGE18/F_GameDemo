using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChooseScenePanel : BasePanel
{
    public Button btnLeft;
    public Button btnRight;
    public Button btnStart;
    public Button btnBack;

    //场景图片和描述
    public Text txtInfo;
    public Image imgScene;

    //记录当前数据索引
    private int _nowIndex;
    //记录当前选择的数据
    private SceneInfo _nowSceneInfo;

    protected override void Init()
    {
        int maxCount = GameDataMgr.Instance.sceneInfoList.Count;
        btnLeft.onClick.AddListener(() =>
        {
            --_nowIndex;
            if (_nowIndex < 0)
                _nowIndex = maxCount - 1;
        });

        btnRight.onClick.AddListener(() =>
        {
            ++_nowIndex;
            if (_nowIndex >= maxCount) _nowIndex = 0;
        });

        btnStart.onClick.AddListener(() =>
        {
            //隐藏当前面板
            UIManager.Instance.HidePanel<ChooseScenePanel>();
            //切换场景
        });

        btnBack.onClick.AddListener(() =>
        {
            //隐藏当前面板
            UIManager.Instance.HidePanel<ChooseScenePanel>();
            //显示角色选择界面
            UIManager.Instance.ShowPanel<ChooseHeroPanel>();
        });

        //一打开面板应该更新一次面板信息
        ChangeScene();
    }

/// <summary>
/// 切换当前显示的场景信息
/// </summary>
    public void ChangeScene()
    {
        _nowSceneInfo = GameDataMgr.Instance.sceneInfoList[_nowIndex];
        //更新图片和显示的图片信息
        imgScene.sprite = Resources.Load<Sprite>(_nowSceneInfo.imgRes);

        txtInfo.text = "名称:\n" + _nowSceneInfo.name + "\n"
                        + "描述:\n" + _nowSceneInfo.tip;
    }
}
