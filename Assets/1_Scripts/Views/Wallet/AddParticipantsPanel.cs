using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class AddParticipantsPanel : UIView
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button dropdownShow;
    [SerializeField] private Button dropdownHide;
    [SerializeField] private ListContainer dropdownPlayers;

    private WalletDataManager Wallet => DataManager.Wallet;
    private LineupDataManager Lineup => DataManager.Lineup;

    protected override void OnEnable()
    {
        base.OnEnable();
        //dropdownPlayers.gameObject.SetActive(false);
    }

    protected override void Subscribe()
    {
        base.Subscribe();

        if (dropdownPlayers != null)
        {
            dropdownPlayers.Init(Lineup.PlayersPoolAsObject);
            dropdownPlayers.gameObject.SetActive(false);

            AddToDispose(UIManager.SubscribeToView(dropdownPlayers, (PlayerModel player) =>
            {
                if (player != null)
                {
                    Wallet.AddParticipant(player.id, player.name);
                    HideDropdown();
                    Hide();
                }
            }));
        }

        if (dropdownShow != null)
        {
            dropdownShow.OnClickAsObservable()
                .Subscribe(_ => ShowDropdown())
                .AddTo(this);
        }

        if (dropdownHide != null)
        {
            dropdownHide.OnClickAsObservable()
                .Subscribe(_ => HideDropdown())
                .AddTo(this);
        }

        if (closeButton != null)
        {
            closeButton.OnClickAsObservable()
                .Subscribe(_ => Hide())
                .AddTo(this);
        }
    }

    private void ShowDropdown()
    {
        if (dropdownPlayers != null)
        {
            dropdownPlayers.gameObject.SetActive(true);
        }
        if (dropdownShow != null)
        {
            dropdownShow.gameObject.SetActive(false);
        }
        if (dropdownHide != null)
        {
            dropdownHide.gameObject.SetActive(true);
        }
    }

    private void HideDropdown()
    {
        if (dropdownPlayers != null)
        {
            dropdownPlayers.gameObject.SetActive(false);
        }
        if (dropdownShow != null)
        {
            dropdownShow.gameObject.SetActive(true);
        }
        if (dropdownHide != null)
        {
            dropdownHide.gameObject.SetActive(false);
        }
    }

    public void InitForAdd()
    {
        HideDropdown();
        Show();
    }
}
