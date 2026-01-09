using DG.Tweening;
using UnityEngine;

public class FadeAnimation : MonoBehaviour, IAnimationComponent
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float startAlpha = 0f;
    [SerializeField] AnimationConfig config;
    [SerializeField] int order = 0;
    [SerializeField] bool parallel = false;

    public int Order => order;
    public bool IsParallel => parallel;

    private float originalAlpha;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        originalAlpha = canvasGroup.alpha;
    }

    public Tween AnimateShow()
    {
        canvasGroup.alpha = startAlpha;
        return canvasGroup.DOFade(originalAlpha, config.Duration)
            .SetEase(config.Ease)
            .SetDelay(config.Delay);
    }

    public Tween AnimateHide()
    {
        canvasGroup.alpha = originalAlpha;
        return canvasGroup.DOFade(startAlpha, config.Duration).SetEase(config.Ease);
    }
}