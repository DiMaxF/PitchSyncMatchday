using UnityEditor;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    [SerializeField] AppConfig config;
    AppModel _appModel = new AppModel();

    public static NavigationDataManager Navigation => Instance._navbar;
    private NavigationDataManager _navbar;

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

        //new Log($"{_navbar.Buttons.Count} {_navbar.Buttons[0].screen}", "DataManager");
    }
}
