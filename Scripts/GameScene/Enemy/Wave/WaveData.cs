using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Unity Inspector 只能显示被序列化的类，
//加了这个标签，WaveData[] 才能在 Inspector 里展开为一个可编辑的数组。
[System.Serializable]
public class WaveData
{
    //资源路线， "Monster/z1"
    public string monsterRes;
    //这波出几只
    public int count;
    //每只之间间隔几秒
    public float spawnInterval;
    //上一波结束后等多久
    public float delayBeforeWave;
}
