using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

    //角色预设体创建的位置
    private Transform heroPos;

    //当前场景中显示的对象
    private GameObject heroObj;
    //当前使用的角色数据
    private RoleInfo nowRoleData;
    //当前使用角色数据的索引
    private int nowIndex;

    protected override void Init()
    {
        //找到场景中放置角色预设体的位置
        heroPos = GameObject.Find("HeroPos").transform;

        //更新左上角玩家拥有的钱
        txtMoney.text = GameDataMgr.Instance.playerData.haveMoney.ToString();

        btnLeft.onClick.AddListener(() =>
        {
            --nowIndex;
            if (nowIndex < 0)
                nowIndex = GameDataMgr.Instance.roleInfoList.Count - 1;
            //模型的更新
            ChangeHero();
        });
        btnRight.onClick.AddListener(() =>
        {
            ++nowIndex;
            if (nowIndex >= GameDataMgr.Instance.roleInfoList.Count)
                nowIndex = 0;
            //模型的更新
            ChangeHero();
        });
        btnStart.onClick.AddListener(() =>
        {
            //记录当前显示的角色
            GameDataMgr.Instance.nowSelRole = nowRoleData;

            //隐藏自己并显示场景选择面板
            UIManager.Instance.HidePanel<ChooseHeroPanel>();
            UIManager.Instance.ShowPanel<ChooseScenePanel>();
        });
        btnBack.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<ChooseHeroPanel>();

            //让摄像机转回去后，显示开始界面
            Camera.main.GetComponent<CameraAnimator>().BackMeun(() =>
            {
                UIManager.Instance.ShowPanel<BeginPanel>();
            });
        });
        btnUnlock.onClick.AddListener(() =>
        {
            //点击解锁按钮的逻辑
            PlayerData data = GameDataMgr.Instance.playerData;
            if (data.haveMoney >= nowRoleData.lockMoney)
            {
                //够买逻辑
                data.haveMoney -= nowRoleData.lockMoney;
                txtMoney.text = data.haveMoney.ToString();
                //记录购买id
                data.buyHero.Add(nowRoleData.id);
                //保存数据
                GameDataMgr.Instance.SavePlayerData();

                //更新解锁按钮
                UpdateLockBtn();

                //提示购买成功
                UIManager.Instance.ShowPanel<TipPanel>().ChangeInfo("购买成功!");
            }
            else
            {
                //提示购买失败
                UIManager.Instance.ShowPanel<TipPanel>().ChangeInfo("金钱不足");
            }
        });

        ChangeHero();
    }

    /// <summary>
    /// 更新场景上要显示的模型
    /// </summary>
    private void ChangeHero()
    {
        if (heroObj != null)
        {
            Destroy(heroObj);
            heroObj = null;
        }
        //取出数据的一条 根据索引值
        nowRoleData = GameDataMgr.Instance.roleInfoList[nowIndex];
        //实例化对象，并记录下来，用于下次切换时删除
        heroObj = Instantiate(Resources.Load<GameObject>(nowRoleData.res), heroPos.position, heroPos.rotation);

        txtName.text = nowRoleData.tips;

        //根据解锁相关数据，来决定是否显示解锁按钮
        UpdateLockBtn();
    }

    private void UpdateLockBtn()
    {
        //如果，该角色需要解锁，且没有解锁的话，就需要显示解锁按钮，并隐藏开始按钮
        if (nowRoleData.lockMoney > 0 && !GameDataMgr.Instance.playerData.buyHero.Contains(nowRoleData.id))
        {
            //更新解锁按钮显示，并更新上面的钱
            btnUnlock.gameObject.SetActive(true);
            txtUnlock.text = "$" + nowRoleData.lockMoney;
            //隐藏开始按钮，因为角色未解锁
            btnStart.gameObject.SetActive(false);
        }
        else
        {
            btnUnlock.gameObject.SetActive(false);
            btnStart.gameObject.SetActive(true);
        }
    }

    public override void HideMe(UnityAction callBack = null)
    {
        base.HideMe(callBack);
        //每次隐藏自己时，把自己当前显示的3D模型角色删除
        if(heroObj != null)
        {
            DestroyImmediate(heroObj);
            heroObj = null;
        }
    }

}
