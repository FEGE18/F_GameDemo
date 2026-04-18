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

    //SmoothDamp 的平滑时间
    [Header("跟随平滑度")]
    [Range(0.01f, 1f)]
    public float followSmooth = 0.1f;

    [Header("缩放")]
    public float minDistance = 4f;  //最近距离
    public float maxDistance = 18f; //最远距离
    public float zoomSpeed = 2f;    //滚轮灵敏度

    [Header("旋转")]
    public float rotateSpeed = 3f;  //鼠标灵敏度
    private float currentYaw;       //当前水平旋转角度

    [Header("遮挡检测")]
    public LayerMask obstacleMask;

    //给SmoothDamp函数当作记录帧与帧之间速度状态的参数
    private Vector3 currentVelocity;


    void Start()
    {
        if (target != null)
            currentYaw = target.eulerAngles.y;
    }
    void LateUpdate()
    {

        if (target == null) return;

        HandleZoom();

        //摄像机最终目标位置
        Vector3 desiredPosition = CalculateTacticalPosition();

        //SmoothDamp会完全到达目标位置，且不依赖帧率。效果是远快近慢
        transform.position = Vector3.SmoothDamp(
            transform.position, //当前位置
            desiredPosition,    //目标位置
            ref currentVelocity,//引用传递，内部自动更新
           followSmooth);              //滑动时间，越小越快到达

        //让摄像机看向角色
        transform.LookAt(target.position + Vector3.up * 1f);
    }

    /// <summary>
    /// 处理滚轮缩放
    /// </summary>
    private void HandleZoom()
    {
        //鼠标滚轮改变距离（向前滚 > 0，向后滚 < 0）
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        tacticalDistance -= scroll * zoomSpeed;
        //限制范围
        tacticalDistance = Mathf.Clamp(tacticalDistance, minDistance, maxDistance);
    }

    /// <summary>
    /// 计算俯瞰模式的摄像机目标位置，及处理按住右键旋转鼠标改变角度的方法
    /// </summary>
    /// <returns></returns>
    private Vector3 CalculateTacticalPosition()
    {
        // 按住右键时旋转
        if (Input.GetMouseButton(1)) // 1 = 右键
        {
            float mouseX = Input.GetAxis("Mouse X");
            currentYaw += mouseX * rotateSpeed;
        }


        //初始计算摄像机角度及位置的逻辑
        //Unity三角函数计算的是弧度，所以需要把角度转弧度
        float rad = tacticalAngle * Mathf.Deg2Rad;
        //高度偏移
        float heightOffset = tacticalDistance * Mathf.Sin(rad);
        //水平偏移
        float horizontalOffset = tacticalDistance * Mathf.Cos(rad);

        //增加鼠标移动旋转视角功能
        //用 currentYaw 角度算出水平方向（不再依赖角色朝向）
        Quaternion rotation = Quaternion.Euler(0, currentYaw, 0);
        Vector3 horizontalDir = rotation * (-Vector3.forward);
        //整体偏移位置
        Vector3 offset = horizontalDir * horizontalOffset + Vector3.up * heightOffset;

        return target.position + offset;
    }


}
