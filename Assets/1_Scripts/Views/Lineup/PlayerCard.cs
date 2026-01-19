using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCard : UIView<PlayerModel>
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text positionBadge;
    [SerializeField] private Button selectButton;

    protected override void Subscribe()
    {
        base.Subscribe();
        
        if (selectButton != null)
        {
            selectButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (DataProperty.Value != null)
                    {
                        Trigger(DataProperty.Value);
                    }
                })
                .AddTo(this);
        }
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = DataProperty.Value;
        if (data == null) return;

        if (nameText != null)
        {
            nameText.text = data.name;
        }

        if (positionBadge != null)
        {
            positionBadge.text = data.position.ToString();
        }
    }
}

