using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [Header("飘字设置")]
    //每秒往上飘多少米
    public float floatSpeed = 1.5f;
    //每秒透明度减多少
    public float fadeSpeed = 2f;
    //多少秒后销毁
    public float destroyTime = 0.8f;

    private TextMeshPro _tmp;
    private Color _color;

    //外部调用：传入伤害值，启动飘字
    public void Init(int damage)
    {
        _tmp = GetComponent<TextMeshPro>();
        _tmp.text = damage.ToString();
        //拿到初始颜色
        _color = _tmp.color;
        //从开始时记录销毁时间
        Destroy(gameObject, destroyTime);
    }

    private void Update()
    {
        //往上飘
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        //渐隐
        _color.a -= fadeSpeed * Time.deltaTime;
        _tmp.color = _color;

        //Billboard 始终面向摄像机，字不歪
        transform.forward = Camera.main.transform.forward;
    }
}
