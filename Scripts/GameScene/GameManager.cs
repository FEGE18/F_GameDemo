using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("角色生成点")]
    // 角色生成点
    public Transform spawnPoint;
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


}
