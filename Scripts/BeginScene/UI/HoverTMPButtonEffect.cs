using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 给 TMP 按钮做一个简单的悬停、按下、抬起动画效果。
/// 作用：
/// 1. 鼠标移入时，按钮轻微放大，文字变亮
/// 2. 鼠标按下时，按钮轻微缩小，文字变深
/// 3. 鼠标移出时，恢复默认状态
///
/// 这个脚本不依赖美术资源，纯代码实现，适合你现在这种项目阶段。
/// </summary>
public class HoverTMPButtonEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    // 按钮本体的 RectTransform。
    // 一般就是挂脚本的这个 UI 物体本身。
    [SerializeField] private RectTransform target;

    // 按钮上的 TMP 文字。
    // 如果不手动拖，会在 Awake 里自动去子物体里找。
    [SerializeField] private TextMeshProUGUI targetText;

    // 默认文字颜色。
    // 用来在鼠标移出后恢复。
    [SerializeField] private Color normalColor = Color.white;

    // 鼠标悬停时文字颜色。
    // 这个颜色可以更亮一点，形成“可交互”的反馈。
    [SerializeField] private Color hoverColor = new Color(1f, 0.95f, 0.6f, 1f);

    // 鼠标按下时文字颜色。
    // 一般比悬停色更深一点，强调“按下”状态。
    [SerializeField] private Color pressColor = new Color(1f, 0.8f, 0.4f, 1f);

    // 悬停时按钮放大的倍率。
    // 1.08 表示比原来大 8%。
    [SerializeField] private float hoverScale = 1.08f;

    // 按下时按钮缩小的倍率。
    // 0.96 表示比原来小 4%。
    [SerializeField] private float pressScale = 0.96f;

    // 动画速度。
    // 数值越大，动画变化越快。
    [SerializeField] private float animSpeed = 12f;

    // 悬停时，按钮上下跳动的高度。
    // 数值越小越轻微，建议先从 6 到 10 之间试。
    [SerializeField] private float hoverJumpOffset = 8f;

    // 单次跳动从下到上的时间。
    // 时间越短，动作越利落；时间越长，动作越柔和。
    [SerializeField] private float jumpUpTime = 0.09f;

    // 单次跳动从上回到原位的时间。
    // 一般和 jumpUpTime 差不多，或者稍微长一点。
    [SerializeField] private float jumpDownTime = 0.11f;

    // 两次跳动之间的停顿时间。
    // 这个值太小会显得很“抖”，太大又会显得不连贯。
    [SerializeField] private float jumpPauseTime = 0.08f;

    // 按钮初始缩放值。
    // 用于在动画结束时恢复原始大小。
    private Vector3 normalScale;

    // 按钮初始位置。
    // 用于跳动效果结束后回到原位。
    private Vector2 normalAnchoredPosition;

    // 当前正在执行的缩放协程。
    // 用来避免多个动画同时跑，造成抖动。
    private Coroutine scaleCoroutine;

    // 当前正在执行的颜色协程。
    // 和缩放协程一样，防止颜色动画冲突。
    private Coroutine colorCoroutine;

    // 当前正在执行的跳动协程。
    // 用来保证悬停跳动只有一条协程在跑。
    private Coroutine jumpCoroutine;

    // 记录鼠标当前是否还停留在按钮上。
    // 这个状态会影响跳动协程是否继续执行。
    private bool isPointerHovering;

    /// <summary>
    /// 初始化组件引用和默认状态。
    /// Awake 会比 Start 更早执行，所以适合做引用缓存。
    /// </summary>
    private void Awake()
    {
        // 如果没有手动指定 target，就默认使用当前物体的 RectTransform。
        if (target == null)
            target = GetComponent<RectTransform>();

        // 如果没有手动指定 targetText，就自动从子物体中找 TMP 文字。
        // 这样你挂到按钮根节点上，通常就能直接工作。
        if (targetText == null)
            targetText = GetComponentInChildren<TextMeshProUGUI>(true);

        // 记录按钮的初始缩放。
        // 之后悬停、按下、恢复都基于这个值计算。
        normalScale = target.localScale;

        // 记录按钮的初始位置。
        // 跳动动画会围绕这个位置做偏移。
        normalAnchoredPosition = target.anchoredPosition;

        // 如果找到了文字，就把当前颜色记录为默认颜色。
        // 这样即使你在 Inspector 里改了颜色，也能作为初始状态。
        if (targetText != null)
            normalColor = targetText.color;
    }

    /// <summary>
    /// 鼠标移入按钮区域时触发。
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 记录当前处于悬停状态。
        // 这个标记会告诉跳动协程：当前应该继续播放。
        isPointerHovering = true;

        // 悬停状态：轻微放大 + 变亮。
        PlayEffect(hoverScale, hoverColor);

        // 开始悬停跳动。
        // 如果之前已经有跳动协程在运行，先停掉，避免重复执行。
        StartHoverJump();
    }

    /// <summary>
    /// 鼠标移出按钮区域时触发。
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        // 记录当前已经离开按钮。
        // 跳动协程会根据这个值停止。
        isPointerHovering = false;

        // 恢复默认状态。
        PlayEffect(1f, normalColor);

        // 停止跳动，并把位置恢复到初始值。
        StopHoverJump();
    }

    /// <summary>
    /// 鼠标按下按钮时触发。
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // 按下状态：轻微缩小 + 变深。
        PlayEffect(pressScale, pressColor);
    }

    /// <summary>
    /// 鼠标松开按钮时触发。
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        // 松开后如果鼠标还停在按钮上，就恢复成悬停状态。
        // 如果鼠标已经离开，则恢复默认状态。
        if (isPointerHovering)
            PlayEffect(hoverScale, hoverColor);
        else
            PlayEffect(1f, normalColor);
    }

    /// <summary>
    /// 统一播放缩放和颜色动画。
    /// 这里把“目标缩放倍率”和“目标文字颜色”一次性传进来，
    /// 这样代码结构更清楚，也方便以后继续扩展。
    /// </summary>
    private void PlayEffect(float scaleFactor, Color textColor)
    {
        // 如果上一次缩放动画还没结束，先停掉，避免多个协程同时改 scale。
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        // 如果上一次颜色动画还没结束，先停掉，避免多个协程同时改颜色。
        if (colorCoroutine != null)
            StopCoroutine(colorCoroutine);

        // 计算最终目标缩放值。
        // normalScale 是初始大小，scaleFactor 是倍率。
        scaleCoroutine = StartCoroutine(ScaleTo(normalScale * scaleFactor));

        // 如果找到了文字，就开始颜色过渡。
        if (targetText != null)
            colorCoroutine = StartCoroutine(ColorTo(textColor));
    }

    /// <summary>
    /// 开始悬停跳动。
    /// 这里单独开一个协程，让按钮在悬停期间持续做轻微上下跳动。
    /// </summary>
    private void StartHoverJump()
    {
        // 如果前一个跳动协程还在跑，先停掉。
        if (jumpCoroutine != null)
            StopCoroutine(jumpCoroutine);

        // 启动新的跳动协程。
        jumpCoroutine = StartCoroutine(HoverJumpLoop());
    }

    /// <summary>
    /// 停止悬停跳动，并把按钮位置恢复到初始状态。
    /// </summary>
    private void StopHoverJump()
    {
        // 如果有跳动协程，直接停止。
        if (jumpCoroutine != null)
        {
            StopCoroutine(jumpCoroutine);
            jumpCoroutine = null;
        }

        // 立即恢复原始位置，避免按钮离开时停在半空中。
        target.anchoredPosition = normalAnchoredPosition;
    }

    /// <summary>
    /// 悬停时循环播放一个轻微跳动。
    /// 这个效果是“上去一点，再回来一点，再停顿一下”，
    /// 让按钮看起来更活泼，但又不会太花哨。
    /// </summary>
    private IEnumerator HoverJumpLoop()
    {
        // 只要鼠标还停留在按钮上，就一直循环。
        while (isPointerHovering)
        {
            // 先向上跳一点。
            yield return MoveAnchoredPosition(
                normalAnchoredPosition + new Vector2(0f, hoverJumpOffset),
                jumpUpTime
            );

            // 再回到原位。
            yield return MoveAnchoredPosition(
                normalAnchoredPosition,
                jumpDownTime
            );

            // 停顿一下，避免动作太密集，看起来像抖动而不是跳动。
            yield return new WaitForSecondsRealtime(jumpPauseTime);
        }

        // 保险处理：循环结束后，确保位置归位。
        target.anchoredPosition = normalAnchoredPosition;
    }

    /// <summary>
    /// 按钮移动的协程控制函数
    /// </summary>
    /// <param name="targetPosition">动画最终的目标位置</param>
    /// <param name="duration">移动动画的持续时间</param>
    /// <returns></returns>
    private IEnumerator MoveAnchoredPosition(Vector2 targetPosition, float duration)
    {
        // 起点位置取当前值，这样每次移动都能从当前位置平滑过去。
        Vector2 startPosition = target.anchoredPosition;

        // 如果持续时间太短，就直接设置，避免除零或者不必要的运算。
        if (duration <= 0f)
        {
            target.anchoredPosition = targetPosition;
            //**立即跳出/终止**当前协同程序，后面的代码**不会执行**。
            yield break;
        }

        float elapsed = 0f;

        // 在规定时间内逐帧过渡。
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            // 计算当前进度。
            float t = Mathf.Clamp01(elapsed / duration);

            // 使用 SmoothStep 让移动曲线更柔和。
            // 这样不会出现生硬的匀速直线移动感。
            float smoothT = t * t * (3f - 2f * t);

            target.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, smoothT);

            //yield return null 的意思是：挂起当前函数，把控制权还给 Unity 主线程，告诉主线程：等到下一帧的时候，再把我唤醒，从这里继续执行。
            //在 Unity 的生命周期里，协程的这句 yield return null 是在每一帧的 Update 之后被唤醒的。
            yield return null;
        }

        // 最后强制对齐到目标位置。
        target.anchoredPosition = targetPosition;
    }

    /// <summary>
    /// 平滑缩放到目标大小。
    /// 这里用协程逐帧插值，而不是一下子改成最终值，
    /// 这样视觉上更柔和。
    /// </summary>
    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        // 当当前大小还没有接近目标大小时，就持续插值。
        while (Vector3.Distance(target.localScale, targetScale) > 0.001f)
        {
            // 使用 Lerp 做平滑过渡。
            // Time.unscaledDeltaTime 的好处是：
            // 即使游戏暂停或 Time.timeScale 变化，UI 动画也能照常进行。
            //Time.unscaledDeltaTime 不收Time.timeScale影响，按真实帧间隔变化
            target.localScale = Vector3.Lerp(
                target.localScale,
                targetScale,
                Time.unscaledDeltaTime * animSpeed
            );

            /*
            第 1 帧：进入循环，算出 1.01。执行 yield return null，函数在这里停住，游戏画面渲染一次。
            第 2 帧：从 yield return null 醒来，再次进入循环条件，算出 1.02。执行 yield return null，停住，渲画面。
            第 N 帧：终于算到了 1.08，达到目标跳出 while 循环。协程结束。
            在玩家眼里看来，就是在好几帧的时间里，按钮慢慢变大了。
            */
            yield return null;
        }

        // 最后强制对齐到精确值，避免浮点误差。
        target.localScale = targetScale;
    }

    /// <summary>
    /// 平滑过渡文字颜色。
    /// </summary>
    private IEnumerator ColorTo(Color targetColor)
    {
        // 只要颜色还没完全接近目标颜色，就继续插值。
        while (Vector4.Distance(targetText.color, targetColor) > 0.001f)
        {
            // 使用 Lerp 逐帧过渡颜色。
            targetText.color = Color.Lerp(
                targetText.color,
                targetColor,
                Time.unscaledDeltaTime * animSpeed
            );

            yield return null;
        }

        // 最终对齐目标颜色。
        targetText.color = targetColor;
    }
}