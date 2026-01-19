using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LayoutUpdater : MonoBehaviour
{
    private VerticalLayoutGroup _vertical;
    private HorizontalLayoutGroup _horizontal;

    private void OnEnable()
    {
        _vertical = GetComponent<VerticalLayoutGroup>();
        _horizontal = GetComponent<HorizontalLayoutGroup>();
        ForceUpdate();
    }

    public void ForceUpdate() 
    {
        if (_vertical != null) UpdateVertical();
        if (_horizontal != null) UpdateHorizontal();
    }

    private void UpdateVertical() 
    {
        float angle = _vertical.spacing;
        DOTween.To(() => angle, x => angle = x, _vertical.spacing + 1, 0.2f)
            .OnUpdate(() => {
                _vertical.spacing = angle;
            });
    }

    private void UpdateHorizontal() 
    {
        float angle = _horizontal.spacing;
        DOTween.To(() => angle, x => angle = x, _horizontal.spacing + 1, 0.2f)
            .OnUpdate(() => {
                _horizontal.spacing = angle;
            });
    }
}
