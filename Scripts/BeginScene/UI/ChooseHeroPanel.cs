using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChooseHeroPanel : BasePanel
{
    //选人左右键
    public Button btnLeft;
    public Button btnRight;
    //购买按钮
    public Button btnUnlock;
    public TextMeshProUGUI txtUnlock;
    //开始和返回
    public Button btnStart;
    public Button btnBack;
    //左上角拥有的钱
    public TextMeshProUGUI txtMoney;
    //角色姓名
    public Text txtName;
    

    protected override void Init()
    {
        
    }
}
