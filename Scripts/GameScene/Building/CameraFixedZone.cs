using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFixedZone : MonoBehaviour
{
    [Header("固定摄像机点（手动放置的空物体）")]
    public Transform cameraPoint;

    private CameraController cameraController;

    void Start()
    {
        // 找到场景中的摄像机控制器
        cameraController = Camera.main.GetComponent<CameraController>();
    }

    void OnTriggerEnter(Collider other)
    {
        // 只有玩家进入才触发（用Tag判断）
        if (other.CompareTag("Player"))
        {
            cameraController.EnterFixedMode(cameraPoint);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cameraController.ExitFixedMode();
        }
    }
}
