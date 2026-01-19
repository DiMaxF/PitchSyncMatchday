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
        if (ratingView != null) ratingView.Init(data.rating);
        if (priceText != null) priceText.text = $"From ${data.basePricePerHour}";
        if (locationNameText != null) locationNameText.text = $"{data.address}";
        if (photo != null) photo.sprite = data.photo;
    }
}
