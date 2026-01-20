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
    [SerializeField] private ConfirmPanel confirmPanel;

    private LineupDataManager Lineup => DataManager.Lineup;
    private int? _pendingPlayerIdToRemove;
    private TeamSide? _pendingTeamSide;

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

        if (playersList != null)
        {
            AddToDispose(UIManager.SubscribeToView(playersList, (int playerId) =>
            {
                var data = DataProperty.Value;
                if (data != null)
                {
                    ShowConfirmRemoveFromTeam(playerId, data.teamSide);
                }
            }));

            AddToDispose(UIManager.SubscribeToView(playersList, (SquadPlayerModel player) =>
            {
                var data = DataProperty.Value;
                if (player != null && data != null)
                {
                    Lineup.ToggleCaptain(player.playerId, data.teamSide);
                }
            }));
        }

        if (confirmPanel != null)
        {
            AddToDispose(UIManager.SubscribeToView(confirmPanel, (bool confirmed) =>
            {
                if (confirmed && _pendingPlayerIdToRemove.HasValue && _pendingTeamSide.HasValue)
                {
                    Lineup.RemovePlayerFromTeam(_pendingPlayerIdToRemove.Value, _pendingTeamSide.Value);
                }
                _pendingPlayerIdToRemove = null;
                _pendingTeamSide = null;
            }));
        }
    }

    private void ShowConfirmRemoveFromTeam(int playerId, TeamSide teamSide)
    {
        var player = Lineup.GetPlayerById(playerId);
        if (player == null || confirmPanel == null) return;

        _pendingPlayerIdToRemove = playerId;
        _pendingTeamSide = teamSide;

        var model = new ConfirmPanelModel
        {
            title = "Remove Player",
            subtitle = $"Are you sure you want to remove {player.name} from the team?",
            acceptText = "Remove",
            declineText = "Cancel"
        };

        confirmPanel.Init(model);
        confirmPanel.Show();
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

