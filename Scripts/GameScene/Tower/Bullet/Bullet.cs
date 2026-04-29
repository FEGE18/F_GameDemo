using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Transform _target;
    private int _damage;
    private float _speed = 15f;

    //目标丢失后，最多再飞几秒
    private float _lifetime = 3f;

    /// <summary>
    /// 由 TowerController 调用，传入追踪目标和伤害值
    /// </summary>
    /// <param name="target"></param>
    /// <param name="damage"></param>
    public void Init(Transform target, int damage)
    {
        _target = target;
        _damage = damage;
    }

    private void Update()
    {
        _lifetime -= Time.deltaTime;
        if (_lifetime <= 0)
        {
            Destroy(gameObject);
            return;
        }

        // 每帧向目标移动  
        //追踪导弹，每帧改变位移方向
        Vector3 dir = (_target.position - transform.position).normalized;
        transform.position += dir * _speed * Time.deltaTime;

        //目标已经死亡，保持当前方向直线飞
        //!_target 等价于 _target == null || _target.gameObject == null
        if (!_target )
        {
            transform.Translate(Vector3.forward * _speed * Time.deltaTime);
            return;
        }

        
        //到达目标（距离小于 0.3 认为命中）
        if(Vector3.Distance(transform.position,_target.position)<=0.3f)
        {
            if(!_target)
                _target.GetComponent<Damageable>()?.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }
}
