using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    private CameraController _cam;
    //玩家控制脚本
    public PlayerController Player{ get; private set; }

    private void Awake()
    {
        Instance = this;
        //先拿到摄像机的脚本
        _cam = Camera.main.GetComponent<CameraController>();
        //PlayerController 挂在玩家身上，还没生成，这里先不获取
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
        yield return null;
    }
}
