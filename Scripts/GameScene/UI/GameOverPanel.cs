using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverPanel : BasePanel
{
    public TextMeshProUGUI txtWin;
    public TextMeshProUGUI txtInfo;
    public TextMeshProUGUI txtMoney;

    public Button btnSure;
    protected override void Init()
    {
        btnSure.onClick.AddListener(() =>
        {
            // 恢复时间，否则新场景也是暂停的
            Time.timeScale = 1f;   
            //隐藏面板
            UIManager.Instance.HidePanel<GameOverPanel>();
            UIManager.Instance.HidePanel<GamePanel>();
            //切换场景
            SceneManager.LoadScene("BeginScene");
        });
    }

    public void SetResult(bool isWin)
    {
        txtWin.text = isWin ? "游戏胜利!" : "游戏失败!";
        txtWin.color  = isWin ? Color.yellow : Color.red;
        txtInfo.text = isWin ? "胜利奖励" : "失败奖励";
        txtMoney.text = "金币:---";  // 占位，后期接金币系统
    }
}
