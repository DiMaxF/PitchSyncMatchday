using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SquadPanel : UIView<SquadPanelModel>
{
    [SerializeField] private Image squadIcon;
    [SerializeField] private Text squadNameText;
    [SerializeField] private Text playerCountText;
    [SerializeField] public ListContainer playersList;
    [SerializeField] private Button addPlayerButton;

    protected override void Subscribe()
    {
        base.Subscribe();
        
        if (addPlayerButton != null)
        {
            addPlayerButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Trigger(DataProperty.Value);
                })
                .AddTo(this);
        }
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = DataProperty.Value;
        if (data == null) return;

        if (squadIcon != null && data.squadIcon != null)
        {
            squadIcon.sprite = data.squadIcon;
        }

        if (squadNameText != null)
        {
            squadNameText.text = $"Squad {data.teamSide}";
        }

        if (playerCountText != null)
        {
            playerCountText.text = data.playerCount.ToString();
        }

        if (playersList != null && data.players != null)
        {
            var playersAsObject = new System.Collections.Generic.List<object>();
            foreach (var player in data.players)
            {
                playersAsObject.Add(player);
            }
            playersList.Init(playersAsObject);
        }
    }
}

