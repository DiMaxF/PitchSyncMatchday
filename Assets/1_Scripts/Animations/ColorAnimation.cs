using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ColorAnimation : MonoBehaviour, IAnimationComponent
{
    [SerializeField] Graphic graphic;
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
        if (graphic == null) graphic = GetComponent<Graphic>();
        showColor = graphic.color;
    }

    public Tween AnimateShow()
    {
        if (!_hasAnimated)
        {
            graphic.color = hideColor;
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
            graphic.color = showColor;
        }
        _hasAnimated = true;
        return graphic.DOColor(hideColor, config.Duration).SetEase(config.Ease);
    }
}
