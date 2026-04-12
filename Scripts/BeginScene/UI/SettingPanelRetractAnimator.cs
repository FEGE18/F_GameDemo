using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// SettingPanel 弹窗动画控制器（独立脚本）。
/// 
/// 这个脚本负责两类动画：
/// 1. 弹出：从按钮位置展开到 imgBK 的最终位置。
/// 2. 弹回：从 imgBK 当前位置收回到按钮位置。
/// 
/// 设计目标：
/// 1. 不依赖你现有的 BasePanel/UIManager 逻辑，可单独挂载和调用。
/// 2. 支持外部传入回调，在弹回结束后执行（比如关闭面板）。
/// 3. 自动处理坐标换算：按钮中心点 -> 面板本地坐标。
/// 4. 动画期间支持防重复触发，避免状态错乱。
/// </summary>
public class SettingPanelRetractAnimator : MonoBehaviour
{
    [Header("必须引用")]
    [Tooltip("需要做弹回动画的目标节点，通常拖 SettingPanel 下的 imgBK")]
    public RectTransform imgBK;

    [Tooltip("动画所在的根 RectTransform,通常就是 SettingPanel 根节点")]
    public RectTransform rootRect;

    [Tooltip("根 Canvas。用于坐标换算时决定是否要使用 UI Camera")]
    public Canvas rootCanvas;

    [Header("动画参数")]
    [Tooltip("弹出总时长（秒）")]
    public float openDuration = 0.30f;

    [Tooltip("弹出时的起始缩放，越小越像从按钮处弹出")]
    [Range(0.01f, 1f)]
    public float startScale = 0.1f;

    [Tooltip("弹回总时长（秒）")]
    public float closeDuration = 0.22f;

    [Tooltip("弹回结束时的缩放比例。一般小于 1,看起来像缩回按钮")]
    [Range(0.01f, 1f)]
    public float targetScale = 0.08f;

    [Tooltip("开启后：首次 SetAnchorButton 时自动播放一次弹出动画。可在不改 BeginPanel 的情况下直接生效")]
    public bool autoPlayPopupOnFirstAnchor = true;

    [Tooltip("是否在动画期间禁用 rootCanvasGroup 交互，防止连点")]
    public bool disableInputWhileAnimating = true;

    [Tooltip("可选：根节点上的 CanvasGroup。用于动画期间拦截点击")]
    public CanvasGroup rootCanvasGroup;

    /// <summary>
    /// 上一次缓存的“目标按钮”。
    /// 方便外部只设置一次锚点，后续直接调用无参弹回。
    /// </summary>
    private RectTransform _lastAnchorButton;

    /// <summary>
    /// 缓存 imgBK 的“最终停靠状态”（即弹出动画终点）。
    /// 这个状态一般来自预制体初始摆放值。
    /// </summary>
    private Vector2 _cachedFinalAnchoredPos;
    private float _cachedFinalScale = 1f;
    private bool _hasCachedFinalState = false;

    /// <summary>
    /// 标记本次启用周期是否已经自动播放过一次弹出。
    /// 目的是避免你在关闭分支再次调用 SetAnchorButton 时误触发“再弹出”。
    /// </summary>
    private bool _hasAutoPoppedThisEnable = false;

    /// <summary>
    /// 当前动画协程句柄。
    /// 若连续触发会先停旧协程，再开新协程，避免两个动画抢同一节点。
    /// </summary>
    private Coroutine _animCoroutine;

    /// <summary>
    /// 对外只读：当前是否在播放弹回动画。
    /// 外部可据此做防抖处理。
    /// </summary>
    public bool IsPlaying { get; private set; }

    private void OnEnable()
    {
        // 每次面板启用，允许下一次首锚点触发自动弹出。
        _hasAutoPoppedThisEnable = false;
    }

    private void Awake()
    {
        // 兜底：如果没手动拖 rootRect，默认取当前物体的 RectTransform。
        if (rootRect == null)
        {
            rootRect = transform as RectTransform;
        }

        // 兜底：如果没手动拖 rootCanvas，自动向父级查找 Canvas。
        if (rootCanvas == null)
        {
            rootCanvas = GetComponentInParent<Canvas>();
        }

        // 兜底：如果没手动拖 CanvasGroup，尝试在当前物体获取。
        if (rootCanvasGroup == null)
        {
            rootCanvasGroup = GetComponent<CanvasGroup>();
        }

        // 尽早缓存“最终位置”，作为弹出终点。
        CacheFinalStateIfNeeded();
    }

    /// <summary>
    /// 缓存目标按钮（锚点）。
    /// 
    /// 典型使用：
    /// 1) 在打开设置时，把 BeginPanel 的 btnSetting 传进来缓存。
    /// 2) 关闭时直接调用 PlayRetract(null, onComplete) 或 PlayRetractToLast(onComplete)。
    /// 与BeginPanel.btnSetting.onClick配合使用，点击时将设置按钮的坐标传到_lastAnchorButton上
    /// </summary>
    /// <param name="anchorButton">弹回目标按钮的 RectTransform</param>
    public void SetAnchorButton(RectTransform anchorButton)
    {
        _lastAnchorButton = anchorButton;

        // 兼容你当前 BeginPanel 的接法：
        // 打开面板时只调用 SetAnchorButton，也可以自动触发一次弹出动画。
        // 仅在当前启用周期首次触发，避免关闭分支再次 SetAnchorButton 时误触发。
        if (autoPlayPopupOnFirstAnchor && !_hasAutoPoppedThisEnable && _lastAnchorButton != null && !IsPlaying)
        {
            _hasAutoPoppedThisEnable = true;
            //自动触发一次播放
            PlayPopFrom(_lastAnchorButton);
        }
    }

    /// <summary>
    /// 播放弹出动画：
    /// 从目标按钮位置飞到 imgBK 的最终停靠位置，并从 startScale 缩放到最终缩放。
    /// </summary>
    /// <param name="anchorButton">弹出起点按钮，可为 null（null 时用缓存按钮）</param>
    /// <param name="onFinished">动画结束回调，可为 null</param>
    public void PlayPopFrom(RectTransform anchorButton, UnityAction onFinished = null)
    {
        if (imgBK == null)
        {
            Debug.LogWarning("[SettingPanelRetractAnimator] imgBK 未绑定，无法播放弹出动画。");
            onFinished?.Invoke();
            return;
        }

        if (rootRect == null)
        {
            Debug.LogWarning("[SettingPanelRetractAnimator] rootRect 未绑定，无法进行坐标换算。");
            onFinished?.Invoke();
            return;
        }

        if (anchorButton != null)
        {
            _lastAnchorButton = anchorButton;
        }

        if (_lastAnchorButton == null)
        {
            Debug.LogWarning("[SettingPanelRetractAnimator] 未提供弹出按钮，且没有历史缓存按钮。");
            onFinished?.Invoke();
            return;
        }

        CacheFinalStateIfNeeded();

        if (_animCoroutine != null)
        {
            StopCoroutine(_animCoroutine);
            _animCoroutine = null;
        }

        _animCoroutine = StartCoroutine(CoPopup(onFinished));
    }

    /// <summary>
    /// 播放弹回动画（可直接指定目标按钮）。
    /// 
    /// 若 anchorButton 传 null，则自动使用上一次缓存的按钮。
    /// 动画结束后会回调 onFinished，可在里面执行关闭面板等逻辑。
    /// </summary>
    /// <param name="anchorButton">目标按钮，可为 null</param>
    /// <param name="onFinished">动画完成回调，可为 null</param>
    public void PlayRetract(RectTransform anchorButton, UnityAction onFinished = null)
    {
        if (imgBK == null)
        {
            Debug.LogWarning("[SettingPanelRetractAnimator] imgBK 未绑定，无法播放弹回动画。");
            onFinished?.Invoke();
            return;
        }

        if (rootRect == null)
        {
            Debug.LogWarning("[SettingPanelRetractAnimator] rootRect 未绑定，无法进行坐标换算。");
            onFinished?.Invoke();
            return;
        }

        // 如果本次有传按钮，更新缓存；否则沿用历史缓存。
        if (anchorButton != null)
        {
            _lastAnchorButton = anchorButton;
        }

        // 没有可用目标按钮时，无法确定弹回终点，直接回调并退出。
        if (_lastAnchorButton == null)
        {
            Debug.LogWarning("[SettingPanelRetractAnimator] 未提供目标按钮，且没有历史缓存按钮。");
            onFinished?.Invoke();
            return;
        }

        CacheFinalStateIfNeeded();

        // 若动画正在播，先停掉旧动画，保证状态一致。
        if (_animCoroutine != null)
        {
            StopCoroutine(_animCoroutine);
            _animCoroutine = null;
        }

        _animCoroutine = StartCoroutine(CoRetract(onFinished));
    }

    /// <summary>
    /// 播放弹回动画（使用缓存按钮）。
    /// 这是对 PlayRetract(null, onFinished) 的语义化封装。
    /// </summary>
    /// <param name="onFinished">动画完成回调，可为 null</param>
    public void PlayRetractToLast(UnityAction onFinished = null)
    {
        PlayRetract(null, onFinished);
    }


    /// <summary>
    /// 播放左右晃动动画，用于提示用户"先关掉设置面板"。
    /// 不影响 imgBK 的最终位置，晃完会回到原位。
    /// </summary>
    /// <param name="shakeAmount">单次晃动的水平像素偏移量</param>
    /// <param name="shakeTimes">晃动次数（一左一右算两次）</param>
    /// <param name="shakeDuration">整个晃动的总时长（秒）</param>
    public void PlayShake(float shakeAmount = 18f, int shakeTimes = 4, float shakeDuration = 0.35f)
    {
        if (IsPlaying) return;
        if (imgBK == null) return;

        if (_animCoroutine != null)
        {
            StopCoroutine(_animCoroutine);
            _animCoroutine = null;
        }

        _animCoroutine = StartCoroutine(CoShake(shakeAmount, shakeTimes, shakeDuration));
    }
    
    /// <summary>
    /// 晃动协程：让 imgBK 在水平方向上来回偏移，最后回到原位。
    /// 原理：用 Sin 函数生成 [-1, 1] 的周期波形，乘以偏移量，叠加到原始位置上。
    /// </summary>
    private IEnumerator CoShake(float shakeAmount, int shakeTimes,float shakeDuration)
    {
        IsPlaying = true;
        // 记录晃动开始前的原始位置，晃完要精确还原
        Vector2 originalPos = imgBK.anchoredPosition;

        float timer = 0f;
        float duration = Mathf.Max(0.01f, shakeDuration);

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);

            //Sin波形控制左右偏移：shakeTimes 决定晃几个来回
            // t 从 0→1，乘以 shakeTimes * PI 让波形完成指定次数的半周期
            float wave = Mathf.Sin(t * shakeTimes * Mathf.PI);

            // (1 - t) 让振幅逐渐衰减：开头晃得大，结尾自然停，不会突然刹车。模拟的是线性阻尼，越震动振幅越小
            float offset = wave * shakeAmount * (1f - t);

            imgBK.anchoredPosition = originalPos + new Vector2(offset, 0f);

            yield return null;

        }
        //强制归位，消除浮点误差
        imgBK.anchoredPosition = originalPos;

        IsPlaying = false;
        //必须手动置null
        _animCoroutine = null;
    }

    /// <summary>
    /// 弹出协程：
    /// - 起点：目标按钮中心换算后的 rootRect 本地坐标 + startScale
    /// - 终点：imgBK 缓存的最终停靠位置 + 缓存的最终缩放
    /// </summary>
    private IEnumerator CoPopup(UnityAction onFinished)
    {
        // ── 第一步：锁定状态，防止外部重复触发 ────────────────
        IsPlaying = true;
        // IsPlaying 是 public 只读属性，外部 BeginPanel 会检查它做防抖

        // ── 第二步：动画期间禁用交互 ──────────────────────────
        if (disableInputWhileAnimating && rootCanvasGroup != null)
        {
            rootCanvasGroup.interactable   = false; // 控件不响应输入
            rootCanvasGroup.blocksRaycasts = false; // 射线也不会打到这个面板上
        }
        // 为什么同时设两个？
        // interactable=false  → 控件变灰，但射线还是会被拦截（不会穿透到下层按钮）
        // blocksRaycasts=false → 射线完全穿透，下层 BeginPanel 的按钮也可以收到事件
        // 两个都关就是"完全透明+不可用"

        // ── 第三步：确定起点和终点 ────────────────────────────
        Vector2 fromPos = GetButtonCenterInRootLocal(_lastAnchorButton);
        // 起点：把 btnSetting 的中心点换算到 SettingPanel 坐标系下
        // 换算原理见后文

        Vector2 toPos = _cachedFinalAnchoredPos;
        // 终点：Awake 里缓存的预制体初始位置（你在 Editor 里摆好的位置）

        float fromScale = startScale;      // 起始缩放，Inspector 里设的 0.1
        float toScale   = _cachedFinalScale; // 终点缩放，通常是 1.0（预制体初始值）

        // ── 第四步：立即把 imgBK 移到起点 ──────────────────────
        imgBK.anchoredPosition = fromPos;
        imgBK.localScale       = Vector3.one * fromScale;
        // 为什么要手动设起点？
        // 因为协程不是瞬间开始的，上一帧 imgBK 可能还在其他位置
        // 如果不设，第一帧 Lerp 的 fromPos 是对的，但 imgBK 视觉上还在旧位置
        // 会有一帧闪烁，所以提前强制归位

        // ── 第五步：核心动画循环 ──────────────────────────────
        float duration = Mathf.Max(0.01f, openDuration); // 防止 0 导致除以 0
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            // 用 unscaledDeltaTime 而不是 deltaTime
            // 区别：如果游戏 TimeScale=0（暂停状态），deltaTime=0 动画会卡死
            //       unscaledDeltaTime 不受 TimeScale 影响，UI 动画依然能播

            float t = Mathf.Clamp01(timer / duration);
            // t 是 [0, 1] 的归一化进度
            // timer=0      → t=0.0（起点）
            // timer=0.15   → t=0.5（中途）
            // timer=0.30   → t=1.0（终点）
            // Clamp01 防止最后一帧 timer 超出 duration 导致 t>1

            float eased = EaseOutBack(t);
            // 把线性的 t 通过缓动曲线变成非线性的 eased
            // EaseOutBack 的特点：在 t=1 附近会超过 1.0，然后弹回来
            // 这就是"回弹"感觉的来源（imgBK 会稍微超过终点再弹回）

            imgBK.anchoredPosition = Vector2.LerpUnclamped(fromPos, toPos, eased);
            // LerpUnclamped 而不是 Lerp：
            // Lerp(a, b, t) 当 t>1 会被夹到 b，看不到超出效果
            // LerpUnclamped(a, b, t) 允许 t>1，imgBK 才能真正"超过终点"

            float scale = Mathf.LerpUnclamped(fromScale, toScale, eased);
            imgBK.localScale = Vector3.one * scale;

            yield return null; // ← 把控制权还给 Unity，等下一帧再继续
        }

        // ── 第六步：强制收尾 ──────────────────────────────────
        imgBK.anchoredPosition = toPos;
        imgBK.localScale       = Vector3.one * toScale;
        // 为什么还要强制设一次？
        // 因为最后一帧 timer 可能超过 duration，t 被 Clamp 到 1.0
        // 但 eased = EaseOutBack(1.0) 理论上等于 1.0，但浮点误差可能是 0.9999...
        // 强制赋值保证动画结束时位置和缩放精确停在终点

        // ── 第七步：恢复交互 ──────────────────────────────────
        if (disableInputWhileAnimating && rootCanvasGroup != null)
        {
            rootCanvasGroup.interactable   = true;
            rootCanvasGroup.blocksRaycasts = true;
        }

        IsPlaying      = false;
        _animCoroutine = null;

        onFinished?.Invoke(); // 弹出结束后调用回调（如果有的话）
    }

    /// <summary>
    /// 弹回协程：
    /// - 起点：imgBK 当前 anchoredPosition / 当前 localScale
    /// - 终点：目标按钮中心换算后的 rootRect 本地坐标 / targetScale
    /// </summary>
    private IEnumerator CoRetract(UnityAction onFinished)
    {
        IsPlaying = true;

        if (disableInputWhileAnimating && rootCanvasGroup != null)
        {
            rootCanvasGroup.interactable   = false;
            rootCanvasGroup.blocksRaycasts = false;
        }

        // ── 差异一：起点从 imgBK 当前状态读取（不是按钮位置）────
        Vector2 fromPos = imgBK.anchoredPosition;  // 当前停靠位置（就是 _cachedFinalAnchoredPos）
        float fromScale = imgBK.localScale.x;      // 当前缩放（通常是 1.0）
        // 为什么不直接用 _cachedFinalAnchoredPos？
        // 因为理论上你可以在动画播到一半时触发关闭（虽然被 IsPlaying 防掉了）
        // 读当前值更稳健，永远从"视觉上的真实位置"出发

        // ── 差异二：终点是按钮位置，缩小到 targetScale ──────────
        Vector2 toPos  = GetButtonCenterInRootLocal(_lastAnchorButton);
        float toScale  = targetScale; // Inspector 里设的 0.08

        float duration = Mathf.Max(0.01f, closeDuration);
        float timer = 0f;

        // ── 差异三：注意这里没有"强制归位到起点"的代码 ──────────
        // CoPopup 有：imgBK.anchoredPosition = fromPos;
        // CoRetract 没有这行！
        // 原因：弹回的起点就是 imgBK 当前所在位置，不需要先归位

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t     = Mathf.Clamp01(timer / duration);

            // ── 差异四：曲线换成了 EaseInCubic ──────────────────
            float eased = EaseInCubic(t);
            // EaseOutBack 的值域超过 [0,1]（有回弹），不适合"收回"
            // EaseInCubic = t³，值域严格在 [0,1]，保证 imgBK 不会超过按钮位置
            // 形状：开头慢，后段陡，视觉上是"先犹豫一下，然后加速吸回去"

            imgBK.anchoredPosition = Vector2.LerpUnclamped(fromPos, toPos, eased);
            float scale            = Mathf.LerpUnclamped(fromScale, toScale, eased);
            imgBK.localScale       = Vector3.one * scale;

            yield return null;
        }

        // 强制收尾
        imgBK.anchoredPosition = toPos;
        imgBK.localScale       = Vector3.one * toScale;

        if (disableInputWhileAnimating && rootCanvasGroup != null)
        {
            rootCanvasGroup.interactable   = true;
            rootCanvasGroup.blocksRaycasts = true;
        }

        IsPlaying      = false;
        _animCoroutine = null;

        onFinished?.Invoke();
        // 这里的 onFinished 通常是：
        // () => UIManager.Instance.HidePanel<SettingPanel>(false)
        // 等动画弹回结束后，才真正销毁 SettingPanel 对象
        // 如果在动画还没结束就销毁，imgBK 操作就是在操作一个 null 引用
    }

    /// <summary>
    /// 缓存 imgBK 的最终停靠状态。
    /// 
    /// 说明：
    /// 1) 一般以预制体初始值作为“弹出终点”。
    /// 2) 只在首次或显式需要时缓存，避免每次动画都覆盖终点。
    /// </summary>
    private void CacheFinalStateIfNeeded()
    {
        if (_hasCachedFinalState)
        {
            return;
        }

        if (imgBK == null)
        {
            return;
        }

        _cachedFinalAnchoredPos = imgBK.anchoredPosition;
        _cachedFinalScale = imgBK.localScale.x;
        _hasCachedFinalState = true;
    }

    /// <summary>
    /// 将目标按钮中心点转换为 rootRect 的本地坐标。
    /// 
    /// 为什么要做这一步：
    /// UI 动画通常操作 anchoredPosition（本地坐标）。
    /// 但按钮在层级中的世界坐标/屏幕坐标并不能直接赋给 anchoredPosition，
    /// 所以必须经过一次坐标空间换算。
    /// </summary>
    private Vector2 GetButtonCenterInRootLocal(RectTransform buttonRect)
    {
        if (buttonRect == null || rootRect == null)
        {
            // 异常兜底：返回当前点，至少不会跳飞。
            return imgBK != null ? imgBK.anchoredPosition : Vector2.zero;
        }

        // 取按钮矩形中心点（本地）并转到世界坐标。
        Vector3 worldCenter = buttonRect.TransformPoint(buttonRect.rect.center);

        Camera uiCamera = null;
        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            // Overlay 模式不需要相机；Camera/WorldSpace 模式需要对应 UI 相机。
            uiCamera = rootCanvas.worldCamera;
        }

        // 世界坐标 -> 屏幕坐标
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldCenter);

        // 屏幕坐标 -> rootRect 本地坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootRect,
            screenPoint,
            uiCamera,
            out Vector2 localPoint
        );

        return localPoint;
    }

    /// <summary>
    /// 三次缓入曲线：x^3
    /// 特点：开头慢，后段加速，适合“收回”类动画。
    /// </summary>
    private float EaseInCubic(float x)
    {
        return x * x * x;
    }

    /// <summary>
    /// 回弹缓出曲线，适合“弹出”类动画。
    /// </summary>
    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}
