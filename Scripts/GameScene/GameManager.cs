  using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    // 单例模式
    public static GameManager Instance { get; private set; }

    [Header("角色生成点")]
    // 角色生成点
    public Transform spawnPoint;

    [Header("金币")]
    //存当前金币数
    public int money;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        // 把 UI 渲染摄像机重新绑定到本场景的 Main Camera
        UIManager.Instance.RebindCameraStack();

        //获取玩家选择的角色数据
        RoleInfo roleInfo = GameDataMgr.Instance.nowSelRole;

        // === 调试用：直接运行 GameScene 时，没有选角色，用默认角色 ===
        if (roleInfo == null)
        {
            roleInfo = GameDataMgr.Instance.roleInfoList[1]; // 取第一个角色
        }

        //从 Resources 加载角色预制体并生成
        GameObject playerPrefab = Resources.Load<GameObject>(roleInfo.res);
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        //把摄像机的跟随目标指向生成的角色
        CameraController camCtrl = Camera.main.GetComponent<CameraController>();
        camCtrl.target = player.transform;

        //显示游戏场景UI
        UIManager.Instance.ShowPanel<GamePanel>();
    }

    public bool SpendMoney(int amount)
    {
        if (money < amount) return false;
        money -= amount;

        //刷新UI
        UIManager.Instance.GetPanel<GamePanel>()?.UpdateMoney(money);
        return true;
    }

    public void GameOver()
    {
        Time.timeScale = 0f;   // 暂停游戏时间
        GameOverPanel panel = UIManager.Instance.ShowPanel<GameOverPanel>();
        panel.SetResult(false);
    }
    
    public void Win()
    {
        Time.timeScale = 0f;   // 暂停游戏时间
        GameOverPanel panel = UIManager.Instance.ShowPanel<GameOverPanel>();
        panel.SetResult(true);
    }

}
