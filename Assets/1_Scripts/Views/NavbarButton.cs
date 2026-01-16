using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class NavbarButton : UIView<NavbarButtonModel>
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button action;
    [SerializeField] private Text label;

    protected override void Subscribe() 
    {
        base.Subscribe();
        action.OnClickAsObservable()
            .Subscribe(_ =>
            {
                Trigger(DataProperty.Value);
            })
            .AddTo(this);
    }

    public override void UpdateUI()
    {
        var data = DataProperty.Value;

        if (iconImage != null) iconImage.sprite = data.icon;
        if (label != null) label.text = data.label;
    }
}
