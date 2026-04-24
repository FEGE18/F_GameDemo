using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTargetSelector : MonoBehaviour
{
    //隔多久计算一次该追谁
    [SerializeField]
    private float _updateInterval = 0.5f;

    //敌人的大脑，主控制脚本
    private EnemyBase _brain;

    private void Awake()
    {
        _brain = GetComponent<EnemyBase>();
    }

    private void Start()
    {
        StartCoroutine(SelectTargetLoop());
    }

    private IEnumerator SelectTargetLoop()
    {
        while (true)
        {
            UpdateTarget();
            yield return new WaitForSeconds(_updateInterval);
        }
    }
    
    /// <summary>
    /// 更新目标的主要逻辑函数
    /// </summary>
        private void UpdateTarget()
    {
        //拿到注册表中所有目标
        IReadOnlyList<Transform> targets = EnemyTargetRegistry.GetAll();
        //若目标为空，直接返回
        if (targets.Count == 0) return;

        //记录最高分的辅助参数
        float bestScore = -1f;
        Transform bestTarget = null;
        foreach (var t in targets)
        {
            //拿到每个目标的权重
            TargetRegistrar reg = t.GetComponent<TargetRegistrar>();
            float weight = reg != null ? reg.weight : 1f;

            //计算与目标之间的距离
            float distance = Vector3.Distance(t.position, this.transform.position);

            //计算评分
            float score = weight / (distance + 0.01f);

            //比较并记录对高分
            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = t;
            }
        }
        //把最高分记录到_brain的目标中
        if (bestTarget != null)
            _brain.Target = bestTarget;
    } 
}

