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
        txtMoney.text = money.ToString();
    }
}
