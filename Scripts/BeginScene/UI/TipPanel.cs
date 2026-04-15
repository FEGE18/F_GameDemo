using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel : BasePanel
{
    //面板显示内容
    public TextMeshProUGUI txtInfo;
    //确定按钮
    public Button btnSure;
    protected override void Init()
    {
        btnSure.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<TipPanel>();
        });
    }

    /// <summary>
    /// 提供给外部改变提示内容的方法
    /// </summary>
    /// <param name="info"></param>
    public void ChangeInfo(string info)
    {
        txtInfo.text = info;
    }
}
