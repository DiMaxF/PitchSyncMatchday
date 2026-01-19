using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class DataManager : MonoBehaviour
{
    private const string APP_MODEL_FILE = "AppModel.json";

    public static DataManager Instance { get; private set; }
    [SerializeField] AppConfig config;
    AppModel _appModel;

    public static NavigationDataManager Navigation => Instance._navbar;
    public static PitchFinderDataManager PitchFinder => Instance._pitchFinder;
    public static BookingDataManager Booking => Instance._booking;
    public static BookingConfirmDataManager BookingConfirm => Instance._bookingConfirm;
    public static ProfileDataManager Profile => Instance._profile;

    private NavigationDataManager _navbar;
    private PitchFinderDataManager _pitchFinder;
    private BookingDataManager _booking;
    private BookingConfirmDataManager _bookingConfirm;
    private ProfileDataManager _profile;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
       
        LoadAppModel();
        InitManagers();
    }

    private void LoadAppModel()
    {
        if (FileUtils.FileExists(APP_MODEL_FILE))
        {
            _appModel = FileUtils.LoadJson<AppModel>(APP_MODEL_FILE);
        }
        else
        {
            _appModel = new AppModel();
        }
    }

    public void SaveAppModel()
    {
        FileUtils.SaveJson(_appModel, APP_MODEL_FILE);
    }

    public void InitManagers() 
    {
        _navbar = new NavigationDataManager(config);
        _pitchFinder = new PitchFinderDataManager(config, _appModel);
        _booking = new BookingDataManager(_appModel, config);
        _bookingConfirm = new BookingConfirmDataManager(_appModel, config);
        _profile = new ProfileDataManager(_appModel);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveAppModel();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveAppModel();
        }
    }

    private void OnDestroy()
    {
        SaveAppModel();
    }
}
