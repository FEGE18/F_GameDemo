  using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    // 单例模式
    public static GameManager Instance { get; private set; }

    [Header("角色生成点")]
    // 角色生成点
    public Transform spawnPoint;

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

    public void GameOver()
    {
        Debug.Log("游戏失败！基地被摧毁");
        // 后续：显示失败面板、停止刷怪等，都在这里加
    }
    
    public void Win()
    {
        Debug.Log("游戏胜利！所有波次清空！");
    // 后续：显示胜利面板等，都在这里加
    }

}
