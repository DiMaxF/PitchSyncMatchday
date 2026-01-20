using System;
using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

    protected override void OnEnable()
    {
        base.OnEnable();

        if (roles != null)
        {
            InitializeRoles();
            roles.Init(Lineup.PositionFiltersAsObject);
        }
    }
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
                    UpdateRolesSelection();
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
        Lineup.PositionFilters.Clear();

        foreach (PlayerPosition position in Enum.GetValues(typeof(PlayerPosition)))
        {
            Lineup.PositionFilters.Add(new ToggleButtonModel
            {
                name = position.ToString(),
                selected = position == _selectedPosition
            });
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
        _avatarPath = player.avatarPath;
        
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
                var newSelected = position == _selectedPosition;
                if (filter.selected != newSelected)
                {
                    Lineup.PositionFilters[i] = new ToggleButtonModel
                    {
                        name = filter.name,
                        selected = newSelected
                    };
                }
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
#if UNITY_ANDROID
        NativeFilePicker.PickFile(OnImagePicked, "image/*");
#elif UNITY_IOS
        NativeFilePicker.PickFile(OnImagePicked, "public.image");
#elif UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select image", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(path))
        {
            OnImagePicked(path);
        }
#else
        Debug.LogWarning("File picker not supported on this platform");
#endif
    }

    private void OnImagePicked(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        try
        {
            byte[] imageData = File.ReadAllBytes(path);
            Texture2D sourceTexture = new Texture2D(2, 2);
            
            if (!sourceTexture.LoadImage(imageData))
            {
                Debug.LogError("Failed to load image from path: " + path);
                return;
            }

            Sprite sprite = FileUtils.ProcessAvatarImage(sourceTexture, 500);
            
            UnityEngine.Object.Destroy(sourceTexture);
            
            if (sprite == null)
            {
                return;
            }
            
            _selectedAvatar = sprite;
            
            if (avatar != null)
            {
                avatar.sprite = sprite;
            }

            if (_editingPlayerId.HasValue)
            {
                var playerId = _editingPlayerId.Value;
                var fileName = $"player_{playerId}_avatar.png";
                FileUtils.SaveImage(sprite, fileName, 500);
                _avatarPath = fileName;
            }
            else
            {
                var tempFileName = $"player_temp_{DateTime.UtcNow.Ticks}_avatar.png";
                FileUtils.SaveImage(sprite, tempFileName, 500);
                _avatarPath = tempFileName;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to process image: {ex.Message}");
        }
    }

}
