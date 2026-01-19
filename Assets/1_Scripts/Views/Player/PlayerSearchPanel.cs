using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSearchPanel : UIView
{
    [SerializeField] private Button closeButton;
    [SerializeField] private InputField searchBar;
    [SerializeField] private ListContainer cards;

    private TeamSide _targetTeamSide;
    private LineupDataManager Lineup => DataManager.Lineup;

    protected override void Subscribe()
    {
        base.Subscribe();

        if (searchBar != null)
        {
            searchBar.OnValueChangedAsObservable()
                .Subscribe(query =>
                {
                    Lineup.SearchQuery.Value = query;
                })
                .AddTo(this);
        }

        if (cards != null)
        {
            cards.Init(Lineup.AvailablePlayersAsObject);

            AddToDispose(UIManager.SubscribeToView(cards, (PlayerModel player) =>
            {
                if (player != null)
                {
                    Lineup.SelectPlayerForTeam(player.id, _targetTeamSide);
                }
            }));
        }

        if (closeButton != null)
        {
            closeButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Hide();
                })
                .AddTo(this);
        }
    }

    public void InitForTeam(TeamSide teamSide)
    {
        _targetTeamSide = teamSide;
        if (searchBar != null)
        {
            searchBar.text = "";
        }
        Lineup.SearchQuery.Value = "";
        Show();
    }

}
