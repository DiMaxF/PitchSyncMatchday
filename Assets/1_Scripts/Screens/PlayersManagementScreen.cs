using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlayersManagementScreen : UIScreen
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button addButton;
    [SerializeField] private PlayerAddPanel addPanel;
    [SerializeField] private ListContainer players;

    private LineupDataManager Lineup => DataManager.Lineup;

    protected override void OnEnable()
    {
        base.OnEnable();
        addPanel.gameObject.SetActive(false);
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
                Lineup.RemovePlayer(playerId);
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
    }
}
