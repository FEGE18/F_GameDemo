using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 控制选择建造防御塔按钮整体的脚本
/// </summary>
public class TowerBtn : MonoBehaviour
{
    public Image imgPic;
    public TextMeshProUGUI txtTip;
    public TextMeshProUGUI txtMoney;

    //内部数据，用来存放这个按钮管理的防御塔的数据
    private TowerInfo _info;

    //抖动协程
    private Coroutine _shakeCoroutine;

    // 当前是否买得起（不是 interactable，是我们自己管的状态）
    private bool _canAfford = true;

    // 控制整体变暗的 CanvasGroup
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        //尝试获取 CanvasGroup，没有就自动加一个
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    } 

    public void Setup(TowerInfo info)
    {
        _info = info;

        //加载图片
        Sprite sp = Resources.Load<Sprite>(info.imgRes);
        if (sp != null) imgPic.sprite = sp;
        //把名字和金额绑定
        txtTip.text = info.name;
        txtMoney.text = "$" + info.money;

        //绑定点击事件
        GetComponent<Button>().onClick.AddListener(OnClick);
    }
    
    /// <summary>
    /// 根据当前金币决定按钮是否可以被点击
    /// </summary>
    /// <param name="currentMoney"></param>
    public void SetInteractable(int currentMoney)
    {
        if (_info == null) return;

        _canAfford = currentMoney >= _info.money;

        //视觉反馈：买得起全亮，买不起半透明变暗
        _canvasGroup.alpha = _canAfford ? 1f : 0.5f;
    }

    private void OnClick()
    {
        //如果支付不起
        if (!_canAfford)
        {
            PlayShake();
            return;
        }
        //如果支付得起
        // 通知放塔管理器：玩家想放这种塔
        TowerPlacementMgr.Instance.StartPlacement(_info);
    }

    /// <summary>
    /// 开启协程
    /// </summary>
    private void PlayShake()
    {
        if (_shakeCoroutine != null)
            StopCoroutine(_shakeCoroutine);

        _shakeCoroutine = StartCoroutine(CoShake());
    }


    /// <summary>
    /// 处理按钮的抖动的协程
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoShake()
    {
        RectTransform rt = GetComponent<RectTransform>();
        Vector2 originPos = rt.anchoredPosition;

        float timer = 0f;
        float duration = 0.35f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float wave = Mathf.Sin(t * 4 * Mathf.PI);
            float offset = wave * 18f * (1f - t);
            rt.anchoredPosition = originPos + new Vector2(offset, 0f);
            yield return null;
        }

        rt.anchoredPosition = originPos;
        _shakeCoroutine = null;
    }

}
