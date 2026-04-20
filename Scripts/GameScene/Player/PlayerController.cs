using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CameraController cameraController;
    private Animator animator;

    [Header("输入平滑")]
    public float accelerateTime = 0.1f;  // 从0加速到1需要的时间（秒）
    public float decelerateTime = 0.15f; // 从1减速到0需要的时间（秒）

    private float smoothH; // 平滑后的水平值
    private float smoothV; // 平滑后的垂直值

    [Header("转向")]
    public float turnSpeed = 10f; //转向速度

    [Header("重力")]
    public float gravity = -9.8f;
    [Header("移动")]
    public float speedMultiplier = 1f;  // Root Motion 速度倍率

    [Header("射击")]
    public Transform muzzlePoint;    // 枪口位置（挂在武器模型上的空物体）
    public LayerMask shootableMask;  // 可被射击的层
    public float shootRange = 100f;  // 射击最大距离

    private CharacterController controller;
    private float verticalSpeed;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        cameraController = Camera.main.GetComponent<CameraController>();
    }

    // Update is called once per frame
    void Update()
    {

        HandleInput();
        HandleRotation();
        //处理开火
        HandleFire();
    }

    /// <summary>
    /// 处理输入并传给 Animator
    /// </summary>
    private void HandleInput()
    {
         float h = Input.GetAxisRaw("Horizontal");
         float v = Input.GetAxisRaw("Vertical");

        // 自定义平滑：有输入时用加速时间，无输入时用减速时间
        float smoothTime;

        smoothTime = (Mathf.Abs(h) > 0.01f) ? accelerateTime : decelerateTime;
        //Mathf.MoveTowards(current, target, maxDelta)
        //每帧最多走 maxDelta 这么远，往 target 靠近
        /*Time.deltaTime / smoothTime 是怎么来的推导:
        总距离 = 1（从 0 到 1）
        总时间 = smoothTime 秒
        速度 = 总距离 / 总时间 = 1 / smoothTime（每秒走多少）
        每帧移动量 = 速度 × 每帧时间 = (1 / smoothTime) × Time.deltaTime
           = Time.deltaTime / smoothTime*/
        smoothH = Mathf.MoveTowards(smoothH, h, Time.deltaTime / smoothTime);

        smoothTime = (Mathf.Abs(v) > 0.01f) ? accelerateTime : decelerateTime;
        smoothV = Mathf.MoveTowards(smoothV, v, Time.deltaTime / smoothTime);

        animator.SetFloat("HSpeed", smoothH);
        animator.SetFloat("VSpeed", smoothV);
    }

    private void HandleRotation()
    {
        // === OTS 模式：角色始终面朝摄像机方向 ===
        if (cameraController.CurrentMode == CameraController.CameraMode.OTS)
        {
            /*什么 OTS 模式下用 Quaternion.Euler(0, otsYaw, 0) 而不直接用摄像机的 rotation？
            因为摄像机有 pitch（上下看），但角色不应该上下倾斜——角色只需要水平方向跟着转。
            把 pitch 设为 0、只用 yaw，角色就只左右转，不会歪倒。*/
            Quaternion targetRotation = Quaternion.Euler(0, cameraController.OTSYaw, 0);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
            return;  // OTS模式处理完直接返回，不走下面的战术模式逻辑
        }

        // === 战术模式：===
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        //角色转向,注意：只有朝前走的时候才转向，朝后走不转向
        if (h != 0 || v > 0)
        {
            Transform cam = Camera.main.transform;
            Vector3 camForward = cam.forward;
            //去掉摄像头的竖直分量，使得角色始终在同一个平面上移动，不会"钻地"
            camForward.y = 0;
            //单位化摄像机方向向量
            camForward.Normalize();
            Vector3 camRight = cam.right;
            camRight.y = 0;
            camRight.Normalize();
            //摄像机的方向向量需要一个一个得到
            //得到方向向量后要与是否输入按键相结合
            Vector3 moveDir = camForward * Mathf.Max(v, 0) + camRight * h; //注意：计算转向方向时，只用前进分量和左右分量 如果只按左/右（v=0），角色转向侧面 如果按前+左/右（v>0），角色转向斜前方

            //这个方法可以得到看向传入向量的目标向量，重点是目标向量
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);

            //平滑转向
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                                                    Time.deltaTime * turnSpeed);
        }
    }

    void OnAnimatorMove()
    {
        //拿到动画这一帧产生的位移，乘以速度倍率
        //animator.deltaPosition：这一帧动画产生的位移（移了多远）
        Vector3 rootMotion = animator.deltaPosition * speedMultiplier;

        //处理重力
        if (controller.isGrounded)
        {
            //保持贴地
            verticalSpeed = -1f;
        }
        else
        {
            //加速下落
            verticalSpeed += gravity * Time.deltaTime;
        }

        //把3个轴上的运动合并
        rootMotion.y = verticalSpeed * Time.deltaTime;

        controller.Move(rootMotion);
    }

    private void HandleFire()
    {
        // 只有 OTS 模式才能射击
        if (cameraController.currentMode != CameraController.CameraMode.OTS)
            return;

        //鼠标左键按下
        if(Input.GetMouseButtonDown(0))
        {
            // 触发攻击动画
            animator.SetBool("IsAttacking", true);

            //=== 第一条射线：从摄像机穿过屏幕中心，找到瞄准点 ===
            //从摄像机位置出发、穿过屏幕正中心（准星位置）的射线。Unity 提供了一个现成的方法：
            Ray camRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Vector3 aimPoint;

            if (Physics.Raycast(camRay,                 //射线（包含起点和方向）
                                out RaycastHit camHit,  //输出参数，Unity 把碰撞信息填进去
                                shootRange,             //射线最远检测多少米
                                shootableMask))         //只检测哪些层
                aimPoint = camHit.point;
            else
                //返回 射线起点 + 方向 × distance 的那个世界坐标点。
                aimPoint = camRay.GetPoint(shootRange);     //沿射线方向走多少米

            Vector3 shootDir = (aimPoint - muzzlePoint.position).normalized;

            //第二条我们有分开的起点和方向，直接传两个参数更直观。
            if (Physics.Raycast(muzzlePoint.position, shootDir, out RaycastHit hit, shootRange, shootableMask))
            {
                //打中了东西
                Debug.Log("击中: " + hit.collider.gameObject.name);
                Debug.DrawLine(muzzlePoint.position, aimPoint, Color.red, 100f);
            }
            else
            {
                // 没打中
                Debug.DrawLine(muzzlePoint.position, aimPoint, Color.yellow, 100f);
            }
        }
    }
}
