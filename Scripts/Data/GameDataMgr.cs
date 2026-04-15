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

    //音乐相关数据
    public MusicData musicData;

    //角色数据
    public List<RoleInfo> roleInfoList;

    //玩家相关数据
    public PlayerData playerData;

    private GameDataMgr()
    {
        //默认初始化一些数据
        musicData = JsonMgr.Instance.LoadData<MusicData>("MusicData");
        //读取角色数据
        roleInfoList = JsonMgr.Instance.LoadData<List<RoleInfo>>("RoleInfo");
        //获取玩家相关数据
        playerData = JsonMgr.Instance.LoadData<PlayerData>("PlayerData");
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
