using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SquadPlayerCard : UIView<SquadPlayerModel>
{
    [SerializeField] private Image image;
    [SerializeField] private Text nameText;
    [SerializeField] private Text positionBadge;
    [SerializeField] private Text squadNumberText;
    [SerializeField] private Button removeButton;
    [SerializeField] private SimpleToggle captainToggle;
    [SerializeField] private SwipeToDelete swipeToDelete;

    protected override void Subscribe()
    {
        base.Subscribe();
        
        if (removeButton != null)
        {
            removeButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (DataProperty.Value != null)
                    {
                        UIManager.TriggerAction(this, DataProperty.Value.playerId);
                    }
                })
                .AddTo(this);
        }

        if (captainToggle != null)
        {
            AddToDispose(UIManager.SubscribeToView(captainToggle, (bool isCaptain) =>
            {
                if (DataProperty.Value != null)
                {
                    Trigger(DataProperty.Value);
                }
            }));
        }

        if (swipeToDelete != null)
        {
            System.Action onDelete = () =>
            {
                if (DataProperty.Value != null)
                {
                    UIManager.TriggerAction(this, DataProperty.Value.playerId);
                }
            };

            swipeToDelete.OnDelete += onDelete;
            AddToDispose(Disposable.Create(() => swipeToDelete.OnDelete -= onDelete));
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
        if (image != null)
        {
            image.sprite = DataManager.Lineup.GetPlayerById(data.playerId).avatar;
        }
        
        if (positionBadge != null)
        {
            positionBadge.text = data.position.ToString();
        }

        if (captainToggle != null)
        {
            captainToggle.Init(data.isCaptain);
        }

        if (squadNumberText != null)
        {
            squadNumberText.text = data.squadNumber.ToString();
        }
    }
}

