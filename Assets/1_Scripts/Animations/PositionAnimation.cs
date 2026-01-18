using DG.Tweening;
using UnityEngine;

public class PositionAnimation : MonoBehaviour, IAnimationComponent
{
    [SerializeField] RectTransform rectTransform;
    [SerializeField] Vector2 hidePosition;
    [SerializeField] AnimationConfig config;
    [SerializeField] int order = 0;
    [SerializeField] bool parallel = false;

    public int Order => order;
    public bool IsParallel => parallel;
    private Vector2 showPosition;
    private bool _hasAnimated = false;

    private void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        showPosition = rectTransform.anchoredPosition;
    }

    public Tween AnimateShow()
    {
        if (!_hasAnimated)
        {
            rectTransform.anchoredPosition = hidePosition;
        }
        _hasAnimated = true;
        return rectTransform.DOAnchorPos(showPosition, config.Duration)
            .SetEase(config.Ease)
            .SetDelay(config.Delay);
    }

    public Tween AnimateHide()
    {
        if (!_hasAnimated)
        {
            rectTransform.anchoredPosition = showPosition;
        }
        _hasAnimated = true;
        return rectTransform.DOAnchorPos(hidePosition, config.Duration).SetEase(config.Ease);
    }
}
