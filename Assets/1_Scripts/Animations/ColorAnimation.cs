using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ColorAnimation : MonoBehaviour, IAnimationComponent
{
    [SerializeField] Graphic graphic;
    [SerializeField] Color startColor = Color.clear;
    [SerializeField] AnimationConfig config;
    [SerializeField] int order = 0;
    [SerializeField] bool parallel = false;

    public int Order => order;
    public bool IsParallel => parallel;

    private Color originalColor;

    private void Awake()
    {
        if (graphic == null) graphic = GetComponent<Graphic>();
        originalColor = graphic.color;
    }

    public Tween AnimateShow()
    {
        graphic.color = startColor;
        return graphic.DOColor(originalColor, config.Duration)
            .SetEase(config.Ease)
            .SetDelay(config.Delay);
    }

    public Tween AnimateHide()
    {
        graphic.color = originalColor;
        return graphic.DOColor(startColor, config.Duration).SetEase(config.Ease);
    }
}
