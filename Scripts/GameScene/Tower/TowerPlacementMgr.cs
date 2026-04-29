using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TowerPlacementMgr : MonoBehaviour
{
    public static TowerPlacementMgr Instance{ get; private set; }

    //现在要放的塔的数据，它表示的就是现在要放的塔
    private TowerInfo _currentInfo;

    //是否在放置模式中
    private bool _isPlacing;

    //预览幽灵，放置时提示的半透明模型
    private GameObject _ghost;

    [Header("Ghost 设置")]
    public Material ghostMaterial;  // 在 Inspector 里拖入 GhostMaterial

    [Tooltip("塔底部到轴心点的距离，防止塔陷入地面")]
    public float placementYOffset = 0.5f;


    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 进入放置模式时准备的逻辑
    /// </summary>
    /// <param name="info">外部传入的点击创建的防御塔</param>
    public void StartPlacement(TowerInfo info)
    {
        //在玩家点击一个防御塔但还没确定放下位置时，又点击另一个防御塔，此时要清除上一个幽灵体
        CancelPlacement();

        //把外部传入的防御塔数据缓存
        _currentInfo = info;
        //设为放置模式
        _isPlacing = true;

        //创建ghost
        GameObject prefab = Resources.Load<GameObject>(_currentInfo.res);
        _ghost = Instantiate(prefab);

        //把Ghost上所有 Renderer 材质换成透明材质
        Renderer[] renderers = _ghost.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            r.material = ghostMaterial;
        }
    }

    /// <summary>
    /// 取消防御塔幽灵体
    /// </summary>
    public void CancelPlacement()
    {
        //关闭状态门，Update() 里的 Raycast 停止执行
        _isPlacing = false;
        //清空数据，防止旧数据被误用
        _currentInfo = null;

        if (_ghost != null)
        {
            Destroy(_ghost);
            _ghost = null;
        }
    }

    private void Update()
    {
        // 不在放置模式，直接返回
        if (!_isPlacing) return;

        //把鼠标屏幕坐标转成一条3D射线
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //让这条射线打出去，如果打到了物体，把碰撞信息存到hit
        if (Physics.Raycast(ray, out RaycastHit hit,
        //最大射线距离：射线无限延伸，打到就是打到，不限距离
        Mathf.Infinity,
        //如果射线打到了敌人、障碍物等。防御塔不能在上面，所以只能在Ground层响应
        LayerMask.GetMask("Ground")))
        {
            //在y轴上方偏移一点，防止“陷”进地面
            _ghost.transform.position = hit.point + Vector3.up * placementYOffset;
        }

        //左键
        if (Input.GetMouseButtonDown(0))
        {
            //确认放塔
            ConfirmPlacement();
        }
        //右键
        if (Input.GetMouseButtonDown(1))
        {
            //取消放塔
            CancelPlacement();
        }
    }

    /// <summary>
    /// 确认放塔
    /// </summary>
    private void ConfirmPlacement()
    {
        //扣钱（进到这里说明一定够钱）
        GameManager.Instance.SpendMoney(_currentInfo.money);

        //在 ghost 位置生成真塔
        GameObject prefab = Resources.Load<GameObject>(_currentInfo.res);
        GameObject tower = Instantiate(prefab, _ghost.transform.position, _ghost.transform.rotation);

        //把塔的数据传给 TowerController中初始化
        tower.GetComponent<TowerController>().Init(_currentInfo);

        //退出放置位置
        CancelPlacement();
    }
}
