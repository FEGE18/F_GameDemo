using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    private CameraController _cam;

    [SerializeField] private Transform _camAnchor; // 在 Inspector 拖入一个空物体

    //玩家控制脚本
    public PlayerController Player{ get; private set; }

    private void Awake()
    {
        Instance = this;
        //先拿到摄像机的脚本
        _cam = Camera.main.GetComponent<CameraController>();
    }

    /// <summary>
    /// 给GameManager调用的赋值函数
    /// </summary>
    /// <param name="player"></param>
    public void RegisterPlayer(PlayerController player)
    {
        Player = player;
    }

    /// <summary>
    /// 由BossBrain.Start()调用，触发Boss出场动画
    /// </summary>
    /// <param name="bossTransform"></param>
    public void PlayBossIntro(Transform bossTransform)
    {
        StartCoroutine(CoBossIntro(bossTransform));
    }

    private IEnumerator CoBossIntro(Transform bossTransform)
    {
        //冻结玩家输入
        CutsceneManager.Instance.Player.isControllable = false;

        //把摄像头切到Boss附近
        _camAnchor.position = bossTransform.position + bossTransform.forward * 8 + bossTransform.up ;
        _camAnchor.rotation = bossTransform.rotation * Quaternion.Euler(0, 180, 0);
        _cam.EnterFixedMode(_camAnchor);

        //等待镜头平滑到位
        yield return new WaitForSeconds(0.5f);

        //让玩家欣赏Boss 1.5s
        yield return new WaitForSeconds(5f);

        //镜头还原战术模式
        _cam.ExitFixedMode();

        //恢复玩家输入
        CutsceneManager.Instance.Player.isControllable = true;
    }
}
