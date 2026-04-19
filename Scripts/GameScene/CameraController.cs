using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum CameraMode
    {
        Tactical,   //战术模式
        Fixed,      //固定机位
        OTS,         //战斗模式
        Transitioning //过渡中
    }
    //让外部读取当前模式和 OTS 的水平角度
    public CameraMode CurrentMode => currentMode;
    public float OTSYaw => otsYaw;

    [Header("模式")]
    public CameraMode currentMode = CameraMode.Tactical;

    //固定机位的目标位置
    private Transform fixedPoint;

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
    public float fixedRotateSpeed = 5f; //固定模式下镜头旋转速度

    [Header("遮挡检测")]
    public LayerMask obstacleMask;

    //给SmoothDamp函数当作记录帧与帧之间速度状态的参数
    private Vector3 currentVelocity;

    [Header("OTS模式")]
    public float otsDistance = 1.5f;    // 摄像机在角色身后多远
    public float otsHeight = 1.5f;      //摄像机比角色高多少（大约肩膀高度）
    public float otsRightOffset = 2f;   // 摄像机往右偏移多少（过肩效果）
    public float otsHMouseSensitivity = 1f; //水平鼠标灵敏度
    public float otsVMouseSensitivity = 1f; //竖直鼠标灵敏度

    private float otsYaw;               // OTS 水平旋转角度
    private float otsPitch;             // OTS 垂直旋转角度（俯仰）

    [Header("过渡")]
    //过渡时间
    public float transitionDuration = 0.3f;
    //过渡完成后要进入的模式
    private CameraMode targetMode;
    //过渡起始位置
    private Vector3 transitionStartPos;
    // 过渡起始旋转
    private Quaternion transitionStartRot;  
    // 过渡计时器
    private float transitionTimer;          


    void Start()
    {
        if (target != null)
            currentYaw = target.eulerAngles.y;
    }
    void LateUpdate()
    {

        if (target == null) return;

        // === 模式切换 ===
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (currentMode == CameraMode.Tactical)
            {
                EnterOTSMode();
            }
            else if (currentMode == CameraMode.OTS)
            {
                EnterTacticalMode();
            }
        }

        switch (currentMode)
        {
            case CameraMode.Tactical:
                UpdateTactical();
                break;
            case CameraMode.Fixed:
                UpdateFixed();
                break;
            case CameraMode.OTS:
                UpdateOTS();
                break;
            case CameraMode.Transitioning:
                UpdateTransition();
                break;
        }
    }

    /// <summary>
    /// 战术视角的LateUpdate逻辑
    /// </summary>
    private void UpdateTactical()
    {
        HandleZoom();

        //摄像机最终目标位置
        Vector3 desiredPosition = CalculateTacticalPosition();

        //检测摄像机是否被遮挡
        desiredPosition = CheckOcclusion(desiredPosition);

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
    /// 固定视角的LateUpdate逻辑
    /// </summary>
    private void UpdateFixed()
    {
        if (fixedPoint == null) return;
        //
        transform.position = Vector3.SmoothDamp(transform.position, fixedPoint.position,
                                                   ref currentVelocity, followSmooth);

        //
        transform.rotation = Quaternion.Slerp(
                                            transform.rotation,                 //起始旋转角度
                                            fixedPoint.rotation,                //目标角度
                                            Time.deltaTime * fixedRotateSpeed); //插值比例，与帧率无关

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
    /// OST视角的Lateupdate逻辑
    /// </summary>
    private void UpdateOTS()
    {
        float mouseX = Input.GetAxis("Mouse X") * otsHMouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * otsVMouseSensitivity;

        otsYaw += mouseX;   //水平旋转
        //垂直旋转，Unity 的 Mouse Y 鼠标往上移返回正值。
        //但旋转里，pitch 增大是低头（往下看）。所以要取反：鼠标上移 → pitch 减小 → 抬头看天。
        otsPitch -= mouseY;
        otsPitch = Mathf.Clamp(otsPitch, -30f, 60f); // 限制俯仰角度

        /***第2步 根据旋转角度计算摄像机位置***/
        //
        Quaternion rotation = Quaternion.Euler(otsPitch, otsYaw, 0);

        // 摄像机在角色身后的方向（旋转后的"后方"）
        Vector3 backDir = rotation * Vector3.back;   // 身后方向
        Vector3 rightDir = rotation * Vector3.right; // 右方向

        // 摄像机目标位置 = 角色位置 + 后方偏移 + 上方偏移 + 右肩偏移
        Vector3 targetPos = target.position
            + Vector3.up * otsHeight          // 先抬高到肩膀高度
            + backDir * otsDistance            // 再往身后拉
            + rightDir * otsRightOffset;       // 再往右偏移（过肩效果)

        // === 碰撞检测（防穿墙）===
        targetPos = CheckOcclusion(targetPos);

        // === 第3步：平滑移动 + 设置朝向 ===
        // 平滑移动
        transform.position = targetPos;

        // 摄像机朝向 = 直接用鼠标控制的旋转
        transform.rotation = rotation;
    }

    private void UpdateTransition()
    {
        transitionTimer += Time.deltaTime;
        float t = Mathf.Clamp01(transitionTimer / transitionDuration);

        //
        t = Mathf.SmoothStep(0, 1, t);

        //
        Vector3 targetPos;
        Quaternion targetRot;

        if (targetMode == CameraMode.OTS)
        {
            //算出 OTS 目标位置（复用 OTS 的计算逻辑）
            Quaternion rotation = Quaternion.Euler(otsPitch, otsYaw, 0);
            Vector3 backDir = rotation * Vector3.back;
            Vector3 rightDir = rotation * Vector3.right;
            targetPos = target.position + Vector3.up * otsHeight + backDir * otsDistance + rightDir * otsRightOffset;
            targetRot = rotation;
        }
        else// 回到 Tactical
        {
            targetPos = CalculateTacticalPosition();
            targetRot = Quaternion.LookRotation(target.position + Vector3.up * 1f - targetPos);

        }

        // 插值
        transform.position = Vector3.Lerp(transitionStartPos, targetPos, t);
        transform.rotation = Quaternion.Slerp(transitionStartRot, targetRot, t);

        //过渡完成
        if(t >= 1f)
        {
            currentMode = targetMode;
        }
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

    /// <summary>
    /// 检测摄像机是否被遮挡
    /// </summary>
    /// <param name="desiredPosition">摄像机此时所在位置</param>
    /// <returns></returns>
    private Vector3 CheckOcclusion(Vector3 desiredPosition)
    {
        //射线检测是从玩家射向镜头的
        //所以要算出玩家位置指向镜头位置的向量
        Vector3 dirToCamera = desiredPosition - target.position;
        //向量的长度，即距离。 后面射线用它做"最大检测距离"——不需要检测比摄像机更远的物体
        float distToCamera = dirToCamera.magnitude;

        // ② 射线起点  从角色脚底抬高1m
        Vector3 rayOrigin = target.position + Vector3.up * 1f;

        /* === 调试用：在Scene视图画出射线 ===
        Debug.DrawRay(rayOrigin, dirToCamera.normalized * distToCamera, Color.red);*/

        //发射射线
        if (Physics.Raycast(rayOrigin,  //从哪发射
        dirToCamera.normalized,         //往哪个方向发射，把方向向量变成单位向量
        out RaycastHit hit,             //碰撞结构存到hit里，out 关键字的意思是"这个变量由函数内部赋值返回"
        distToCamera,                   //最大检测距离
        obstacleMask))                  //只检测这些层
        {
            /* === 调试用：碰到东西时画绿色线段 + 打印碰到了什么 ===
            Debug.DrawLine(rayOrigin, hit.point, Color.green);
            Debug.Log($"射线碰到了: {hit.collider.gameObject.name}, Layer: {hit.collider.gameObject.layer}");*/

            //有遮挡：拉近摄像机
            return hit.point - dirToCamera.normalized * 0.3f;
        }
        //无遮挡：保持原位
        return desiredPosition;
    }

    /// <summary>
    /// 提供给外部切换到固定模式的方法
    /// </summary>
    /// <param name="point">进入点即镜头点的位置</param>
    public void EnterFixedMode(Transform point)
    {
        currentMode = CameraMode.Fixed;
        fixedPoint = point;
    }

    /// <summary>
    /// 给外部离开固定模式的方法
    /// </summary>
    public void ExitFixedMode()
    {
        currentMode = CameraMode.Tactical;
        fixedPoint = null;
    }

    /// <summary>
    /// 切换到 OTS 模式
    /// </summary>
    private void EnterOTSMode()
    {
        targetMode = CameraMode.OTS;
        //新增过渡方法
        currentMode = CameraMode.Transitioning;
        transitionStartPos = transform.position;
        transitionStartRot = transform.rotation;
        transitionTimer = 0f;

        // 初始化 OTS 旋转角度为当前摄像机的朝向（避免切换时跳转）
        otsYaw = currentYaw;
        otsPitch = 0f;

        // 锁定并隐藏光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// 切换回战术模式
    /// </summary>
    private void EnterTacticalMode()
    {
        targetMode = CameraMode.Tactical;
         //新增过渡方法
        currentMode = CameraMode.Transitioning;
        transitionStartPos = transform.position;
        transitionStartRot = transform.rotation;
        transitionTimer = 0f;

        // 同步旋转角度（避免切换时跳转）
        currentYaw = otsYaw;

        // 解锁并显示光标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
