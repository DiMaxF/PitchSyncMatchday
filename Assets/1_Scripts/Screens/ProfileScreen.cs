using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ProfileScreen : UIScreen
{
    [SerializeField] private ProfileCard profileInfo;
    [SerializeField] private Button editButton;
    [SerializeField] private Button bookingsButton;
    [SerializeField] private Button walletButton;
    [SerializeField] private SimpleToggle notification;
    [SerializeField] private Text matchesPlayedText;
    [SerializeField] private Text bookingsText;
    [SerializeField] private Text lineupCreatedText;
    [SerializeField] private ProfileEditPanel profileEditPanel;
    [SerializeField] private Image avatar;

    private ProfileDataManager Profile => DataManager.Profile;

    protected override void SubscribeToData()
    {
        base.SubscribeToData();

        if (editButton != null)
        {
            AddToDispose(editButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (profileEditPanel != null)
                    {
                        var profileInfoData = Profile.BuildProfileInfo();
                        profileEditPanel.Init(profileInfoData);
                        profileEditPanel.Show();
                    }
                }));
        }

        if (bookingsButton != null)
        {
            AddToDispose(bookingsButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    ScreenManager?.Show(Screens.MyBookingScreen);
                }));
        }

        if (walletButton != null)
        {
            AddToDispose(walletButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                }));
        }

        if (notification != null)
        {
            notification.Init(Profile.NotificationsEnabled.Value);
            AddToDispose(UIManager.SubscribeToView(notification, (bool value) =>
            {
                Profile.NotificationsEnabled.Value = value;
            }));
        }

        if (matchesPlayedText != null)
        {
            AddToDispose(Profile.MatchesPlayed.Subscribe(value =>
            {
                matchesPlayedText.text = value.ToString();
            }));
        }

        if (bookingsText != null)
        {
            AddToDispose(Profile.BookingsCount.Subscribe(value =>
            {
                bookingsText.text = value.ToString();
            }));
        }

        if (lineupCreatedText != null)
        {
            AddToDispose(Profile.LineupCreatedCount.Subscribe(value =>
            {
                lineupCreatedText.text = value.ToString();
            }));
        }

        if (profileInfo != null)
        {
            var profileInfoData = Profile.BuildProfileInfo();
            profileInfo.Init(profileInfoData);

            AddToDispose(Profile.ProfileName.Subscribe(_ => UpdateProfileInfo()));
            AddToDispose(Profile.ProfileEmail.Subscribe(_ => UpdateProfileInfo()));
            AddToDispose(Profile.ProfileUserpicPath.Subscribe(_ => UpdateProfileInfo()));
        }

        if (avatar != null)
        {
            AddToDispose(Profile.ProfileUserpicPath.Subscribe(_ => UpdateAvatar()));
            UpdateAvatar();
        }
    }

    private void UpdateProfileInfo()
    {
        if (profileInfo != null)
        {
            var profileInfoData = Profile.BuildProfileInfo();
            profileInfo.Init(profileInfoData);
        }
    }

    private void UpdateAvatar()
    {
        if (avatar != null)
        {
            Sprite userpicSprite = null;
            if (!string.IsNullOrEmpty(Profile.ProfileUserpicPath.Value))
            {
                userpicSprite = FileUtils.LoadImageAsSprite(Profile.ProfileUserpicPath.Value);
            }
            
            if (userpicSprite == null)
            {
                userpicSprite = Resources.Load<Sprite>("ic_profile_0");
            }
            
            avatar.sprite = userpicSprite;
        }
    }

    protected override void RefreshViews()
    {
        base.RefreshViews();

        if (matchesPlayedText != null)
        {
            matchesPlayedText.text = Profile.MatchesPlayed.Value.ToString();
        }

        if (bookingsText != null)
        {
            bookingsText.text = Profile.BookingsCount.Value.ToString();
        }

        if (lineupCreatedText != null)
        {
            lineupCreatedText.text = Profile.LineupCreatedCount.Value.ToString();
        }

        if (profileInfo != null)
        {
            var profileInfoData = Profile.BuildProfileInfo();
            profileInfo.Init(profileInfoData);
        }
    }
}
