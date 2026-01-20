using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class AddParticipantsPanel : UIView
{
    [SerializeField] private Button closeButton;
    [SerializeField] private InputField searchBar;
    [SerializeField] private ListContainer cards;

    private WalletDataManager Wallet => DataManager.Wallet;
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
            cards.Init(Lineup.PlayersPoolAsObject);

            AddToDispose(UIManager.SubscribeToView(cards, (PlayerModel player) =>
            {
                if (player != null)
                {
                    Wallet.AddParticipant(player.id, player.name);
                    Hide();
                }
            }));
        }

        if (closeButton != null)
        {
            closeButton.OnClickAsObservable()
                .Subscribe(_ => Hide())
                .AddTo(this);
        }
    }

    public void InitForAdd()
    {
        if (searchBar != null)
        {
            searchBar.text = "";
        }
        Lineup.SearchQuery.Value = "";
        Show();
    }
}
