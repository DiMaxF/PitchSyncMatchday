using UniRx;
using UnityEngine;

public class ProfileDataManager : IDataManager
{
    private readonly AppModel _appModel;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    public ReactiveProperty<int> MatchesPlayed { get; }
    public ReactiveProperty<int> BookingsCount { get; }
    public ReactiveProperty<int> LineupCreatedCount { get; }
    public ReactiveProperty<bool> NotificationsEnabled { get; }
    public ReactiveProperty<PitchSize> DefaultPitchSize { get; }
    public ReactiveProperty<MatchDuration> DefaultDuration { get; }
    public ReactiveProperty<string> ProfileName { get; }
    public ReactiveProperty<string> ProfileEmail { get; }
    public ReactiveProperty<string> ProfileUserpicPath { get; }

    public ProfileDataManager(AppModel appModel)
    {
        _appModel = appModel;

        MatchesPlayed = new ReactiveProperty<int>(appModel.matchesPlayed);
        BookingsCount = new ReactiveProperty<int>(appModel.bookingsCount);
        LineupCreatedCount = new ReactiveProperty<int>(appModel.lineupCreatedCount);
        NotificationsEnabled = new ReactiveProperty<bool>(appModel.notificationsEnabled);
        DefaultPitchSize = new ReactiveProperty<PitchSize>(appModel.defaultPitchSize);
        DefaultDuration = new ReactiveProperty<MatchDuration>(appModel.defaultDuration);
        ProfileName = new ReactiveProperty<string>(appModel.profileName);
        ProfileEmail = new ReactiveProperty<string>(appModel.profileEmail);
        ProfileUserpicPath = new ReactiveProperty<string>(appModel.profileUserpicPath);

        MatchesPlayed.Subscribe(value => _appModel.matchesPlayed = value).AddTo(_disposables);
        BookingsCount.Subscribe(value => _appModel.bookingsCount = value).AddTo(_disposables);
        LineupCreatedCount.Subscribe(value => _appModel.lineupCreatedCount = value).AddTo(_disposables);
        NotificationsEnabled.Subscribe(value => _appModel.notificationsEnabled = value).AddTo(_disposables);
        DefaultPitchSize.Subscribe(value => _appModel.defaultPitchSize = value).AddTo(_disposables);
        DefaultDuration.Subscribe(value => _appModel.defaultDuration = value).AddTo(_disposables);
        ProfileName.Subscribe(value => _appModel.profileName = value).AddTo(_disposables);
        ProfileEmail.Subscribe(value => _appModel.profileEmail = value).AddTo(_disposables);
        ProfileUserpicPath.Subscribe(value => _appModel.profileUserpicPath = value).AddTo(_disposables);
    }

    public ProfileInfo BuildProfileInfo()
    {
        Sprite userpicSprite = null;
        if (!string.IsNullOrEmpty(ProfileUserpicPath.Value))
        {
            userpicSprite = FileUtils.LoadImageAsSprite(ProfileUserpicPath.Value);
        }
        
        if (userpicSprite == null)
        {
            userpicSprite = Resources.Load<Sprite>("ic_profile_0");
        }

        return new ProfileInfo
        {
            userpic = userpicSprite,
            name = ProfileName.Value,
            email = ProfileEmail.Value
        };
    }

    public void UpdateProfile(string name, string email, Sprite userpic, string userpicPath)
    {
        ProfileName.Value = name;
        ProfileEmail.Value = email;
        
        if (userpic != null && !string.IsNullOrEmpty(userpicPath))
        {
            FileUtils.SaveImage(userpic, userpicPath, 500);
            ProfileUserpicPath.SetValueAndForceNotify(userpicPath);
        }
        
        DataManager.Instance.SaveAppModel();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}

