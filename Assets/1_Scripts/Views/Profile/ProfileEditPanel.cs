using System;
using System.IO;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ProfileEditPanel : UIView<ProfileInfo>
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button changePhoto;
    [SerializeField] private InputField nameInput;
    [SerializeField] private InputField emailInput;
    [SerializeField] private Image userpic;

    private ProfileDataManager Profile => DataManager.Profile;
    private Sprite _selectedUserpic;
    private string _userpicPath = "";

    protected override void Subscribe()
    {
        base.Subscribe();

        if (saveButton != null)
        {
            saveButton.OnClickAsObservable()
                .Subscribe(_ => OnSaveClicked())
                .AddTo(this);
        }

        if (changePhoto != null)
        {
            changePhoto.OnClickAsObservable()
                .Subscribe(_ => OnChangePhotoClicked())
                .AddTo(this);
        }

        if (closeButton != null)
        {
            closeButton.OnClickAsObservable()
                .Subscribe(_ => Hide())
                .AddTo(this);
        }
    }

    public override void Init(ProfileInfo initialData = default)
    {
        base.Init(initialData);

        var data = DataProperty.Value;
        if (data != null)
        {
            if (nameInput != null)
            {
                nameInput.text = data.name ?? "";
            }

            if (emailInput != null)
            {
                emailInput.text = data.email ?? "";
            }

            if (userpic != null)
            {
                userpic.sprite = data.userpic;
                _selectedUserpic = data.userpic;
            }

            _userpicPath = Profile.ProfileUserpicPath.Value ?? "";
        }
    }

    private void OnSaveClicked()
    {
        if (nameInput == null || string.IsNullOrWhiteSpace(nameInput.text))
        {
            return;
        }

        string name = nameInput.text;
        string email = emailInput != null ? emailInput.text : "";

        Profile.UpdateProfile(name, email, _selectedUserpic, _userpicPath);
        Hide();
    }

    private void OnChangePhotoClicked()
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
            
            _selectedUserpic = sprite;
            
            if (userpic != null)
            {
                userpic.sprite = sprite;
            }

            string fileName = "profile_userpic.png";
            FileUtils.SaveImage(sprite, fileName, 500);
            _userpicPath = fileName;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to process image: {ex.Message}");
        }
    }
}
