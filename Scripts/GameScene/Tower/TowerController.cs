using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerController : MonoBehaviour
{
    [Header("炮管(旋转朝向目标))")]
    public Transform turretHead;

    //从 TowerInfo 读取的数据
    private float _attackRange;
    private int _attackDamage;
    private float _attackInterval;

    //内部状态
    private Transform _target;
    private float _lastAtkTime;

    //直接缓存子弹预设体，避免每次发射时从Resource加载，浪费性能
    [Header("子弹预制体")]
    public GameObject bulletPrefab;

    //子弹对象池是否已经初始化的标记
    //为什么需要标记？因为多个防御塔共用一个静态池，只需要初始化一次
    //如果每个塔都调用 InitPool，会造成不必要的重复检查
    private static bool _bulletPoolInitialized = false;


    //由外部（放塔逻辑）调用，传入本塔的配置逻辑
    public void Init(TowerInfo info)
    {
        _attackRange = info.atkRange;
        _attackDamage = info.atk;
        _attackInterval = info.atkInterval;

        //初始化子弹对象池（只在第一座防御塔初始化时执行一次）
        //为什么用静态标记？因为 Bullet 的对象池是静态的，所有防御塔共用
        //如果不加标记，每座塔都会调用 InitPool，虽然内部有判断但仍然浪费
        if (!_bulletPoolInitialized)
        {
            //从 bulletPrefab 的 GameObject 上获取 Bullet 组件
            //为什么要 GetComponent？因为对象池需要的是 Bullet 组件，不是 GameObject
            Bullet bulletComponent = bulletPrefab.GetComponent<Bullet>();

            //初始化子弹池
            //capacity: 50 表示预估同屏最多 50 发子弹（根据实际情况调整）
            //maxSize: 200 表示池的硬上限，防止极端情况下无限扩容
            Bullet.InitPool(bulletComponent, capacity: 50, maxSize: 200);

            //标记已初始化，后续的防御塔不再重复初始化
            _bulletPoolInitialized = true;
        }

        //开始协程
        StartCoroutine(SelectTargetLoop());

        TowerRegistry.Register(this.transform);
    }

    private void Update()
    {
        //如果没有目标，什么都不做，直接退出
        if (_target == null) return;

        //计算方向向量，从炮管指向目标
        Vector3 dir = _target.position - turretHead.position;
        //屏蔽掉俯仰头的旋转，直接设为0
        dir.y = 0;

        //如果dir 是零向量，LookRotation(Vector3.zero) 会报错，这行是防止这种极端情况。
        if (dir != Vector3.zero)
        {
            //把方向向量转换为"面朝这个方向的旋转值"。
            Quaternion targetRot = Quaternion.LookRotation(dir);
            turretHead.rotation = Quaternion.RotateTowards(
            //每帧最多转 180 × deltaTime 度（约每秒 180 度），平滑旋转不瞬间跳转。
            //如果想要更灵活的炮台，把 180 改大；想要笨重的炮台，改小。
                turretHead.rotation, targetRot, 180f * Time.deltaTime);
        }

        //冷却未到，本帧不攻击
        if (Time.time - _lastAtkTime < _attackInterval) return;
        //更新上次攻击时间
        _lastAtkTime = Time.time;

        //从对象池中生成子弹（替代 Instantiate）
        //Spawn 方法内部会从池中取出对象，并设置位置和旋转
        Bullet bullet = Bullet.Spawn(turretHead.position, turretHead.rotation);

        //生成失败（池已满或未初始化）
        if (bullet == null)
        {
            //记录警告，但不中断游戏逻辑
            //实际项目中可以根据需求决定是否要做降级处理（比如用 Instantiate 兜底）
            Debug.LogWarning("[TowerController] 子弹生成失败");
            return;
        }

        //初始化子弹的业务数据（目标和伤害）
        bullet.Init(_target, _attackDamage);
    }

    private IEnumerator SelectTargetLoop()
    {
        while (true)
        {
            //等待0.5秒
            yield return new WaitForSeconds(0.5f);
            UpdateTarget();
        }
    }

    private void UpdateTarget()
    {
        //拿到目标注册表里的所有对象
        var all = EnemyRegistry.GetAll();

        //记录最近怪物的对象及距离
        Transform nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Transform t in all)
        {
            //判断一个对象是否被销毁，就判断它的Transform是否为null
            if (t == null) continue;

            float dist = Vector3.Distance(transform.position, t.position);
            if (dist <= _attackRange && dist < nearestDist)
            {
                nearest = t;
                nearestDist = dist;
            }
        }
        //将找到的最近目标赋值给_target
        _target = nearest;
    }

    private void OnDestroy()
    {
        TowerRegistry.Unregister(this.transform);
    }
}
