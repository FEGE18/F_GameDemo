using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePanel : BasePanel
{
    public Image imgHP;
    public TextMeshProUGUI txtHP;
    public TextMeshProUGUI txtMoney;
    //HP的初始宽，可以在外面修改有多宽
    public float hpw = 500;

    public Button btnQuit;

    //下方造塔组合控件的父对象，主要用于控制显隐
    public Transform botTrans;

    //管理3个复合控件
    public List<TowerBtn> towerBtns = new List<TowerBtn>();

    //射击点十字准星
    public Image imgCrosshair;
    protected override void Init()
    {
        //监听按钮事件
        btnQuit.onClick.AddListener(() =>
        {
            //隐藏游戏界面
            UIManager.Instance.HidePanel<GamePanel>();
            //弹出设置界面
        });

        //一开始隐藏下方造塔的UI
        botTrans.gameObject.SetActive(false);

        //默认隐藏准星
        imgCrosshair.gameObject.SetActive(false);

        //初始化造塔按钮
        List<TowerInfo> towerList = GameDataMgr.Instance.towerInfoList;
        for(int i = 0;i < towerBtns.Count && i < towerList.Count;i++)
        {
            Debug.Log($"Setup 第{i}个按钮，towerBtns[i]={towerBtns[i]?.name}，info.name={towerList[i]?.name}");
            towerBtns[i].Setup(towerList[i]);
        }
    }

    /// <summary>
    /// 给外部更新血量
    /// </summary>
    /// <param name="hp"></param>
    /// <param name="maxHP"></param>
    public void UpdateHP(int hp, int maxHP)
    {
        txtHP.text = hp + "/" + maxHP;
        //改变UI界面的长宽时，要变成RectTransform
        ((RectTransform)imgHP.transform).sizeDelta = new Vector2((float)hp / maxHP * hpw, 47);
    }

    /// <summary>
    /// 更新金币数量
    /// </summary>
    /// <param name="money"></param>
    public void UpdateMoney(int money)
    {
        //更新文字
        txtMoney.text = money.ToString();

        //同时遍历 towerBtns，让每个按钮跟当前金币比一下，如果钱不够，按钮变灰，不让点
        foreach (var btn in towerBtns)
        {
            btn.SetInteractable(money);
        }
    }

    /// <summary>
    /// 给外部控制准星显示/隐藏（OTS模式显示，其他模式隐藏）
    /// </summary>
    /// <param name="show"></param>
    public void SetCrosshairShow(bool show)
    {
        imgCrosshair.gameObject.SetActive(show);
    }

    void LateUpdate()
    {
        // 战术模式下显示造塔 UI，OTS 模式下隐藏
        // OTS模式下显示射击准星，战术模式下隐藏
        CameraController cam = Camera.main.GetComponent<CameraController>();
    if (cam != null)
    {
        bool isTactical = cam.CurrentMode == CameraController.CameraMode.Tactical;
            botTrans.gameObject.SetActive(isTactical);

        bool isOTS = cam.currentMode == CameraController.CameraMode.OTS;
            imgCrosshair.gameObject.SetActive(isOTS);
    }
    }
}
