using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PitchCard : UIView<StadiumModel>
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text distanceText;
    [SerializeField] private Text priceText;
    [SerializeField] private Text locationNameText;
    [SerializeField] private Image photo;
    [SerializeField] private Button pickButton;
    [SerializeField] private RatingView ratingView;

    protected override void Subscribe()
    {
        base.Subscribe();
        pickButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                Trigger(DataProperty.Value);
            })
            .AddTo(this);
    }

    public override void UpdateUI()
    {
        var data = DataProperty.Value;

        if (nameText != null) nameText.text = data.name;
    }
}
