using DG.Tweening;
using UnityEngine;

public class ScaleAnimation : MonoBehaviour, IAnimationComponent
{
    [SerializeField] Transform targetTransform;
    [SerializeField] Vector3 hideScale = Vector3.zero;
    [SerializeField] AnimationConfig config;
    [SerializeField] int order = 0;
    [SerializeField] bool parallel = false;

    public int Order => order;
    public bool IsParallel => parallel;
    private Vector3 showScale;

    private void Awake()
    {
        if (targetTransform == null) targetTransform = transform;
        showScale = targetTransform.localScale;
    }

    public Tween AnimateShow()
    {
        targetTransform.localScale = hideScale;
        return targetTransform.DOScale(showScale, config.Duration)
            .SetEase(config.Ease)
            .SetDelay(config.Delay);
    }

    public Tween AnimateHide()
    {
        targetTransform.localScale = showScale;
        return targetTransform.DOScale(hideScale, config.Duration).SetEase(config.Ease);
    }
}

