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
        StartCoroutine(RunWaves());
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
    
    private IEnumerator RunWaves()
    {
        //外部循环，每一次i++，就是一波怪物的生成
        for (int i = 0; i < waves.Length; i++)
        {
            //记录当前是第几波
            _currentWave = i;
            WaveData wave = waves[i];
            //暂停协程，等待下一波开始
            yield return new WaitForSeconds(wave.delayBeforeWave);

            //内层循环，每一次j++，就是一波内的每一只怪物生成
            for (int j = 0; j < wave.count; j++)
            {
                //生成一只小怪
                SpawnOne(wave);
                //暂停协程，等待下一只小怪生成
                yield return new WaitForSeconds(wave.spawnInterval);
            }

            //WaitUntil 怎么工作的？
            //Unity 每帧会调用这个小函数检查结果：
                //返回 false → 继续等，下一帧再检查
                //返回 true → 条件满足，协程从这行继续往下执行
            yield return new WaitUntil(() => _aliveCount <= 0);
            Debug.Log($"第{i + 1}波清空");
        }
        GameManager.Instance.Win();
    }
}
