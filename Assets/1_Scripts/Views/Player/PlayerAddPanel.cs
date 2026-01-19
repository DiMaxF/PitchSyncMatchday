using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAddPanel : UIView
{
    [SerializeField] private Button saveButton;
    [SerializeField] private Button addImage;
    [SerializeField] private Button closeButton;
    [SerializeField] private ListContainer roles;
    [SerializeField] private InputField nameInput;
    [SerializeField] private Image avatar;

    private PlayerPosition _selectedPosition = PlayerPosition.GK;
    private Sprite _selectedAvatar;
    private string _avatarPath = "";
    private int? _editingPlayerId;

    private LineupDataManager Lineup => DataManager.Lineup;


    protected override void Subscribe()
    {
        base.Subscribe();

        if (roles != null)
        {
            InitializeRoles();
            roles.Init(Lineup.PositionFiltersAsObject);

            AddToDispose(UIManager.SubscribeToView(roles, (ToggleButtonModel data) =>
            {
                if (Enum.TryParse<PlayerPosition>(data.name, out var position))
                {
                    _selectedPosition = position;
                }
            }));
        }

        if (saveButton != null)
        {
            saveButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    OnSaveClicked();
                })
                .AddTo(this);
        }

        if (addImage != null)
        {
            addImage.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    OnAddImageClicked();
                })
                .AddTo(this);
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

    private void InitializeRoles()
    {
        if (Lineup.PositionFilters.Count == 0)
        {
            foreach (PlayerPosition position in Enum.GetValues(typeof(PlayerPosition)))
            {
                Lineup.PositionFilters.Add(new ToggleButtonModel
                {
                    name = position.ToString(),
                    selected = position == _selectedPosition
                });
            }
        }
    }

    public void InitForAdd()
    {
        _editingPlayerId = null;
        _selectedPosition = PlayerPosition.GK;
        _selectedAvatar = null;
        _avatarPath = "";
        
        if (nameInput != null)
        {
            nameInput.text = "";
        }

        if (avatar != null)
        {
            avatar.sprite = null;
        }

        UpdateRolesSelection();
        Show();
    }

    public void InitForEdit(PlayerModel player)
    {
        if (player == null)
        {
            InitForAdd();
            return;
        }

        _editingPlayerId = player.id;
        _selectedPosition = player.position;
        _selectedAvatar = player.avatar;
        
        if (nameInput != null)
        {
            nameInput.text = player.name;
        }

        if (avatar != null)
        {
            avatar.sprite = player.avatar;
        }

        UpdateRolesSelection();
        Show();
    }

    private void UpdateRolesSelection()
    {
        for (int i = 0; i < Lineup.PositionFilters.Count; i++)
        {
            var filter = Lineup.PositionFilters[i];
            if (Enum.TryParse<PlayerPosition>(filter.name, out var position))
            {
                Lineup.PositionFilters[i] = new ToggleButtonModel
                {
                    name = filter.name,
                    selected = position == _selectedPosition
                };
            }
        }
    }

    private void OnSaveClicked()
    {
        if (nameInput == null || string.IsNullOrWhiteSpace(nameInput.text))
        {
            return;
        }

        if (_editingPlayerId.HasValue)
        {
            Lineup.UpdatePlayer(_editingPlayerId.Value, nameInput.text, _selectedPosition, _selectedAvatar, _avatarPath);
        }
        else
        {
            Lineup.AddPlayer(nameInput.text, _selectedPosition, _selectedAvatar, _avatarPath);
        }

        Hide();
    }

    private void OnAddImageClicked()
    {
    }

}
