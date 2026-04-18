using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //在 Inspector 里拖入玩家角色
    [Header("跟随目标")]
    public Transform target;

    //通过这两个变量结合三角函数，就可以得到摄像机的准确位置，即离人的水平和竖直位置
    [Header("战术模式")]
    //摄像机到角色的距离
    public float tacticalDistance = 8f;
    //摄像机俯视角度
    public float tacticalAngle = 50f;

    //给SmoothDamp函数当作记录帧与帧之间速度状态的参数
    private Vector3 currentVelocity;


    void LateUpdate()
    {
        //Unity三角函数计算的是弧度，所以需要把角度转弧度
        float rad = tacticalAngle * Mathf.Deg2Rad;
        //高度偏移
        float heightOffset = tacticalDistance * Mathf.Sin(rad);
        //水平偏移
        float horizontalOffset = tacticalDistance * Mathf.Cos(rad);
        //整体偏移位置
        Vector3 offset = -target.forward * horizontalOffset + Vector3.up * heightOffset;
        //摄像机最终目标位置
        Vector3 desiredPosition = target.position + offset;

        //SmoothDamp会完全到达目标位置，且不依赖帧率。效果是远快近慢
        transform.position = Vector3.SmoothDamp(
            transform.position, //当前位置
            desiredPosition,    //目标位置
            ref currentVelocity,//引用传递，内部自动更新
            0.1f);              //滑动时间，越小越快到达
    }

}
