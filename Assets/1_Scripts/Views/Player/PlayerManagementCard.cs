using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManagementCard : UIView<PlayerModel>
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text positionBadge;
    [SerializeField] private Image avatarImage;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;

    protected override void Subscribe()
    {
        base.Subscribe();
        
        if (editButton != null)
        {
            editButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (DataProperty.Value != null)
                    {
                        Trigger(DataProperty.Value);
                    }
                })
                .AddTo(this);
        }

        if (deleteButton != null)
        {
            deleteButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (DataProperty.Value != null)
                    {
                        UIManager.TriggerAction(this, DataProperty.Value.id);
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

        if (avatarImage != null)
        {
            avatarImage.sprite = data.avatar;
        }
    }
}

