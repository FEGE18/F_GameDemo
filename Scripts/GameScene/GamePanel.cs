using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    protected override void Init()
    {
        
    }
}
