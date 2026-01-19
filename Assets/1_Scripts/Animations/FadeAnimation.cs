using DG.Tweening;
using UnityEngine;

public class FadeAnimation : MonoBehaviour, IAnimationComponent
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float hideAlpha = 0f;
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
        canvasGroup.alpha = hideAlpha;
        canvasGroup.interactable = false;
        return canvasGroup.DOFade(originalAlpha, config.Duration)
            .SetEase(config.Ease)
            .SetDelay(config.Delay).OnComplete(() => canvasGroup.interactable = true);
    }

    public Tween AnimateHide()
    {
        canvasGroup.alpha = originalAlpha;
        return canvasGroup.DOFade(hideAlpha, config.Duration).SetEase(config.Ease).OnComplete(() => canvasGroup.interactable = true); ;
    }
}