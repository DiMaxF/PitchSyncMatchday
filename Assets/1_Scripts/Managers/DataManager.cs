using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    [SerializeField] AppConfig config;
    AppModel _appModel = new AppModel();

    public static NavigationDataManager Navigation => Instance._navbar;
    public static PitchFinderDataManager PitchFinder => Instance._pitchFinder;
    
    private NavigationDataManager _navbar;
    private PitchFinderDataManager _pitchFinder;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
       
        InitManagers();
    }

    public void InitManagers() 
    {
        _navbar = new NavigationDataManager(config);
        _pitchFinder = new PitchFinderDataManager(config);
    }
}
