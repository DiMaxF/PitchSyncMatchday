using NUnit.Framework.Constraints;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class AddParticipantsPanel : UIView
{
    [SerializeField] private Text selectedPlayerText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button dropdownShow;
    [SerializeField] private Button dropdownHide;
    [SerializeField] private ListContainer dropdownPlayers;

    private WalletDataManager Wallet => DataManager.Wallet;
    private LineupDataManager Lineup => DataManager.Lineup;

    private PlayerModel selectedPlayer;

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
                    selectedPlayer = player;
                    selectedPlayerText.text = selectedPlayer.name;
                    HideDropdown();
                }
            }));
        }
        selectedPlayerText.text = "None";
        if (dropdownShow != null)
        {
            dropdownShow.OnClickAsObservable()
                .Subscribe(_ => ShowDropdown())
                .AddTo(this);
        }

        if (saveButton != null)
        {
            saveButton.OnClickAsObservable()
                .Subscribe(_ => Save())
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


    private void Save() 
    {
        Wallet.AddParticipant(selectedPlayer.id, selectedPlayer.name);
        Hide();
    }

    private void ShowDropdown()
    {
        if (dropdownPlayers != null)
        {
            dropdownPlayers.Show();
        }
        if (dropdownShow != null)
        {
            //dropdownShow.gameObject.SetActive(false);
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
            dropdownPlayers.Hide();
        }
        if (dropdownShow != null)
        {
            //dropdownShow.gameObject.SetActive(true);
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
