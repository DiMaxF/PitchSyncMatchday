using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class AddParticipantsPanel : UIView
{
    [SerializeField] private Text selectedPlayerText;
    [SerializeField] private InputField nameInput;
    [SerializeField] private InputField amountInput;
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
                    if (nameInput != null && string.IsNullOrWhiteSpace(nameInput.text))
                    {
                        nameInput.text = selectedPlayer.name;
                    }

                    HideDropdown();
                    UpdateSaveButtonState();
                }
            }));
        }

        selectedPlayer = null;
        if (selectedPlayerText != null)
        {
            selectedPlayerText.text = "None";
        }

        if (nameInput != null)
        {
            AddToDispose(nameInput.OnValueChangedAsObservable()
                .Subscribe(_ => UpdateSaveButtonState())
                .AddTo(this));
        }

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

        UpdateSaveButtonState();
    }


    private void Save() 
    {
        string participantName = "";
        int? playerId = null;
        float amount= 0;

        if (selectedPlayer != null)
        {
            playerId = selectedPlayer.id;
            participantName = selectedPlayer.name;
        }

        if (nameInput != null && !string.IsNullOrWhiteSpace(nameInput.text))
        {
            participantName = nameInput.text;
        }
        if (amountInput != null && float.TryParse(amountInput.text, out var am))
        {
            amount = am;
        }
        if (string.IsNullOrWhiteSpace(participantName))
        {
            return;
        }

        Wallet.AddParticipant(playerId, participantName, amount);
        Hide();
    }

    private void UpdateSaveButtonState()
    {
        if (saveButton == null) return;

        bool hasPlayer = selectedPlayer != null;
        bool hasName = nameInput != null && !string.IsNullOrWhiteSpace(nameInput.text);

        saveButton.interactable = hasPlayer || hasName;
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
        selectedPlayer = null;
        if (selectedPlayerText != null)
        {
            selectedPlayerText.text = "None";
        }
        if (nameInput != null)
        {
            nameInput.text = "";
        }
        HideDropdown();
        UpdateSaveButtonState();
        Show();
    }
}
