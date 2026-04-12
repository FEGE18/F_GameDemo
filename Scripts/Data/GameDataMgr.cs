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

    private GameDataMgr()
    {
        //默认初始化一些数据
        musicData = JsonMgr.Instance.LoadData<MusicData>("MusicData");
    }

    public void SaveMusicData()
    {
        JsonMgr.Instance.SaveData(musicData, "MusicData");
    }

}
