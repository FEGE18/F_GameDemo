using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BKMusic : MonoBehaviour
{
    private static BKMusic _instance;
    public static BKMusic Instance => _instance;

    private AudioSource bkSource;

    //这种继承MonoBehaviour的单例类，一般在Awake函数里初始化
    //因为一旦被挂载在物体上必进入这个函数
    void Awake()
    {
        _instance = this;
        bkSource = this.GetComponent<AudioSource>();

        //通过数据来设置音乐的大小和开关
        MusicData data = GameDataMgr.Instance.musicData;
        SetIsOpen(data.musicOpen);
        ChangeValue(data.musicValue);
    }

    /// <summary>
    /// 开关背景音乐的方法
    /// </summary>
    /// <param name="isOpen">是否开关音乐</param>
    public void SetIsOpen(bool isOpen)
    {
        bkSource.mute = !isOpen;
    }

    /// <summary>
    /// 改变背景音乐大小的方法
    /// </summary>
    /// <param name="value">改变大小</param>
    public void ChangeValue(float value)
    {
        bkSource.volume = value;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
