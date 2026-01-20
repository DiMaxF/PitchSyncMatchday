using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlayersManagementScreen : UIScreen
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button addButton;
    [SerializeField] private PlayerAddPanel addPanel;
    [SerializeField] private ListContainer players;
    [SerializeField] private ConfirmPanel confirmPanel;

    private LineupDataManager Lineup => DataManager.Lineup;
    private int? _pendingPlayerIdToRemove;

    protected override void OnEnable()
    {
        base.OnEnable();
        addPanel.gameObject.SetActive(false);
        if (confirmPanel != null)
        {
            confirmPanel.gameObject.SetActive(false);
        }
    }

    protected override void SubscribeToData()
    {
        base.SubscribeToData();

        if (players != null)
        {
            players.Init(Lineup.PlayersPoolAsObject);

            AddToDispose(UIManager.SubscribeToView(players, (PlayerModel player) =>
            {
                if (player != null && addPanel != null)
                {
                    addPanel.InitForEdit(player);
                }
            }));

            AddToDispose(UIManager.SubscribeToView(players, (int playerId) =>
            {
                ShowConfirmRemovePlayer(playerId);
            }));
        }

        if (addButton != null)
        {
            AddToDispose(addButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (addPanel != null)
                    {
                        addPanel.Show();
                        addPanel.InitForAdd();
                    }
                }));
        }

        if (backButton != null)
        {
            AddToDispose(backButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    ScreenManager?.Show(Screens.LineupScreen);
                }));
        }

        if (confirmPanel != null)
        {
            AddToDispose(UIManager.SubscribeToView(confirmPanel, (bool confirmed) =>
            {
                if (confirmed && _pendingPlayerIdToRemove.HasValue)
                {
                    Lineup.RemovePlayer(_pendingPlayerIdToRemove.Value);
                }
                _pendingPlayerIdToRemove = null;
            }));
        }
    }

    private void ShowConfirmRemovePlayer(int playerId)
    {
        var player = Lineup.GetPlayerById(playerId);
        if (player == null || confirmPanel == null) return;

        _pendingPlayerIdToRemove = playerId;

        var model = new ConfirmPanelModel
        {
            title = "Delete Player",
            subtitle = $"Are you sure you want to delete {player.name}? This action cannot be undone.",
            acceptText = "Delete",
            declineText = "Cancel"
        };

        confirmPanel.Init(model);
        confirmPanel.Show();
    }
}
