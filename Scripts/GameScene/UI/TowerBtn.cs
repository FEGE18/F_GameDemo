using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 控制选择建造防御塔按钮整体的脚本
/// </summary>
public class TowerBtn : MonoBehaviour
{
    public Image imgPic;
    public TextMeshProUGUI txtTip;
    public TextMeshProUGUI txtMoney;

    //内部数据，用来存放这个按钮管理的防御塔的数据
    private TowerInfo _info;

    public void Setup(TowerInfo info)
    {
        _info = info;

        //加载图片
        Sprite sp = Resources.Load<Sprite>(info.imgRes);
        if (sp != null) imgPic.sprite = sp;
        //把名字和金额绑定
        txtTip.text = info.name;
        txtMoney.text = "$" + info.money;

        //绑定点击事件
        GetComponent<Button>().onClick.AddListener(OnClick);
    }
    
    /// <summary>
    /// 根据当前金币决定按钮是否可以被点击
    /// </summary>
    /// <param name="currentMoney"></param>
    public void SetInteractable(int currentMoney)
    {
        GetComponent<Button>().interactable = currentMoney >= _info.money;
    }
    
    private void OnClick()
    {
        // 通知放塔管理器：玩家想放这种塔
        // 下一步再填
        Debug.Log("选择了塔：" + _info.name);
    }
}
