using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 专门用来管理数据的一个类
/// </summary>
public class GameDataMgr
{
    private static GameDataMgr _instance;
    public static GameDataMgr Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameDataMgr();
            return _instance;
        }
    }

    //记录选择的角色数据，用于之后的游戏场景中的创建
    public RoleInfo nowSelRole;

    //音乐相关数据
    public MusicData musicData;

    //角色数据
    public List<RoleInfo> roleInfoList;

    //玩家相关数据
    public PlayerData playerData;

    //场景数据
    public List<SceneInfo> sceneInfoList;

    private GameDataMgr()
    {
        //默认初始化一些数据
        musicData = JsonMgr.Instance.LoadData<MusicData>("MusicData");
        //读取角色数据
        roleInfoList = JsonMgr.Instance.LoadData<List<RoleInfo>>("RoleInfo");
        //获取玩家相关数据
        playerData = JsonMgr.Instance.LoadData<PlayerData>("PlayerData");
        //获取场景数据
        sceneInfoList = JsonMgr.Instance.LoadData<List<SceneInfo>>("SceneInfo");
    }

    public void SaveMusicData()
    {
        JsonMgr.Instance.SaveData(musicData, "MusicData");
    }

    public void SavePlayerData()
    {
        JsonMgr.Instance.SaveData(playerData, "PlayerData");
    }

}
