using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class OutlineAnimation : MonoBehaviour, IAnimationComponent
{
    [SerializeField] Outline graphic;
    [SerializeField] Color hideColor = Color.clear;
    [SerializeField] AnimationConfig config;
    [SerializeField] int order = 0;
    [SerializeField] bool parallel = false;

    public int Order => order;
    public bool IsParallel => parallel;
    private Color showColor;
    private bool _hasAnimated = false;

    private void Awake()
    {
        if (graphic == null) graphic = GetComponent<Outline>();
        showColor = graphic.effectColor;
    }

    public Tween AnimateShow()
    {
        if (!_hasAnimated)
        {
            graphic.effectColor = hideColor;
        }
        _hasAnimated = true;
        return graphic.DOColor(showColor, config.Duration)
            .SetEase(config.Ease)
            .SetDelay(config.Delay);
    }

    public Tween AnimateHide()
    {
        if (!_hasAnimated)
        {
            graphic.effectColor = showColor;
        }
        _hasAnimated = true;
        return graphic.DOColor(hideColor, config.Duration).SetEase(config.Ease);
    }
}
