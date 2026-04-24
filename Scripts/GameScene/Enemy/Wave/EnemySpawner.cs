using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("波次配置")]
    public WaveData[] waves;

    [Header("出生点")]
    public Transform[] spawnPoints;

    //当前进行到第几波（索引）
    private int _currentWave = 0;
    //当前波还有几只怪活着
    private int _aliveCount = 0;

    void Start()
    {

    }

    private void SpawnOne(WaveData wave)
    {
        //随机生成点
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

        //加载并实例化
        GameObject prefab = Resources.Load<GameObject>(wave.monsterRes);
        GameObject enemy = Instantiate(prefab, point.position, point.rotation);

        //订阅死亡事件，计数器+1
        _aliveCount++;
        Damageable dmg = enemy.GetComponent<Damageable>();
        // Spawner 需要知道 "是我这波生的怪死了" 
        // 所以 谁生的怪，谁订阅，精准到每一个 GameObject 实例
        dmg.OnDeath += OnEnemyDied;
    }
    
    private void OnEnemyDied()
    {
        //死亡后计数器-1
        _aliveCount--;
    }
}
