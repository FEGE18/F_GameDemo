using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // === 对象池管理（静态） ===

    //子弹对象池，静态字段保证所有子弹共用同一个池
    //为什么用静态？因为不同的防御塔发射的子弹应该从同一个池里取，避免重复创建多个池
    private static ObjectPool<Bullet> _pool;

    //记录初始化时使用的 Prefab，用于检测场景切换时 Prefab 是否变化
    //为什么需要记录？场景切换后可能加载了不同的子弹 Prefab，需要清理旧池重新初始化
    private static Bullet _prefab;

    /// <summary>
    /// 初始化子弹对象池（游戏开始时调用一次）
    /// </summary>
    /// <param name="prefab">子弹的 Prefab 模板</param>
    /// <param name="capacity">池的初始容量，建议设为同屏最大子弹数</param>
    /// <param name="maxSize">池的最大容量，防止无限扩容导致内存爆炸</param>
    public static void InitPool(Bullet prefab, int capacity = 50, int maxSize = 200)
    {
        //如果已经初始化过同一个 Prefab，直接返回，避免重复创建池
        if (_pool != null && _prefab == prefab)
        {
            return;
        }

        //如果 Prefab 变了（比如场景切换），先清理旧池
        if (_pool != null)
        {
            _pool.Clear();
        }

        //保存 Prefab 引用
        _prefab = prefab;

        //创建新的对象池
        //onGet: 取出时激活 GameObject，让子弹开始运行
        //onRelease: 归还时禁用 GameObject，停止 Update 等生命周期函数
        _pool = new ObjectPool<Bullet>(
            prefab,
            parent: null,  //不指定父节点，对象池会自动创建一个根节点
            defaultCapacity: capacity,
            maxSize: maxSize,
            onGet: bullet => bullet.gameObject.SetActive(true),
            onRelease: bullet => bullet.gameObject.SetActive(false)
        );
    }

    /// <summary>
    /// 从对象池中生成一个子弹（替代 Instantiate）
    /// </summary>
    /// <param name="position">子弹的初始位置（通常是炮管位置）</param>
    /// <param name="rotation">子弹的初始旋转（通常是炮管朝向）</param>
    /// <returns>已经初始化好位置和旋转的子弹实例</returns>
    public static Bullet Spawn(Vector3 position, Quaternion rotation)
    {
        //如果池还没初始化，报错并返回 null
        //为什么不自动初始化？因为需要传入 Prefab，这里拿不到
        if (_pool == null)
        {
            Debug.LogError("[Bullet] 对象池未初始化，请先调用 Bullet.InitPool()");
            return null;
        }

        //从池中取出一个子弹
        Bullet bullet = _pool.Get();

        //取出失败（池已满且无法扩容）
        if (bullet == null)
        {
            Debug.LogWarning("[Bullet] 对象池已满，无法生成新子弹");
            return null;
        }

        //设置子弹的世界位置和世界旋转
        //为什么在这里设置？因为每次发射的位置和朝向都不同，不能在池里预设
        bullet.transform.SetPositionAndRotation(position, rotation);

        //返回已经准备好的子弹实例
        return bullet;
    }

    /// <summary>
    /// 将子弹归还到对象池（替代 Destroy）
    /// </summary>
    public void Despawn()
    {
        //如果池还没初始化（极端情况，比如脚本加载顺序问题），直接销毁对象
        if (_pool == null)
        {
            Destroy(gameObject);
            return;
        }

        //将自己归还到对象池
        //Release 会调用 onRelease 回调，禁用 GameObject
        _pool.Release(this);
    }

    /// <summary>
    /// 清理对象池（场景切换时调用）
    /// </summary>
    public static void ClearPool()
    {
        //清空池中的所有对象
        _pool?.Clear();

        //重置静态引用
        _pool = null;
        _prefab = null;
    }

    // === 子弹业务逻辑（原有代码） ===

    private Transform _target;
    private int _damage;
    private float _speed = 15f;

    //目标丢失后，最多再飞几秒
    private float _lifetime = 3f;

    /// <summary>
    /// 由 TowerController 调用，传入追踪目标和伤害值
    /// </summary>
    /// <param name="target">要追踪的目标（敌人）</param>
    /// <param name="damage">子弹的伤害值</param>
    public void Init(Transform target, int damage)
    {
        _target = target;
        _damage = damage;

        //重置生命周期计时器
        //为什么要重置？因为对象池复用的对象可能还保留着上一次的计时器状态
        _lifetime = 3f;
    }

    private void Update()
    {
        _lifetime -= Time.deltaTime;
        if (_lifetime <= 0)
        {
            //生命周期结束，归还到对象池而不是销毁
            Despawn();
            return;
        }

        //!_target 等价于 _target == null || _target.gameObject == null
        if (_target)
        {
            // 每帧向目标移动
            //追踪导弹，每帧改变位移方向
            Vector3 dir = (_target.position - transform.position).normalized;
            transform.position += dir * _speed * Time.deltaTime;

            //到达目标（距离小于 0.3 认为命中）
            if (Vector3.Distance(transform.position, _target.position) <= 0.3f)
            {
                _target.GetComponent<Damageable>()?.TakeDamage(_damage);

                //命中后归还到对象池而不是销毁
                Despawn();
            }
        }

        else
        {
            //目标已经死亡，保持当前方向直线飞
            transform.Translate(Vector3.forward * _speed * Time.deltaTime);
        }

    }
}
