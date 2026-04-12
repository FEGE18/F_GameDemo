using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : BasePanel
{
    public Toggle togMusic;
    public Toggle togSound;
    public Slider sliderMusic;
    public Slider sliderSound;

    //给外部判断是否正在显示的变量，主要用于在显示设置界面时，点击其他按钮无效
    public bool isShow = false;

    protected override void Init()
    {
        //初始化面板显示内容，根据本地存储的设置数据来初始化
        MusicData music = GameDataMgr.Instance.musicData;
        //初始化开关控件的状态
        togMusic.isOn = music.musicOpen;
        togSound.isOn = music.soundOpen;
        //初始化拖动条控件的大小
        sliderMusic.value = music.musicValue;
        sliderSound.value = music.soundValue;
        
        
        togMusic.onValueChanged.AddListener((b) =>
        {
            //让背景音乐进行开关
            BKMusic.Instance.SetIsOpen(b);
            //记录开关的数据
            GameDataMgr.Instance.musicData.musicOpen = b;
        });

        togSound.onValueChanged.AddListener((b) =>
        {
            //记录音效开关的数据
            GameDataMgr.Instance.musicData.soundOpen = b;
        });

        sliderMusic.onValueChanged.AddListener((f) =>
        {
            //让背景音乐大小改变
            BKMusic.Instance.ChangeValue(f);
            //记录背景音乐大小改变
            GameDataMgr.Instance.musicData.musicValue = f;
        });

        sliderSound.onValueChanged.AddListener((f) =>
        {
            //记录音效大小的数据
            GameDataMgr.Instance.musicData.soundValue = f;
        });
    }

   
}
