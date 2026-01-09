using UnityEditor;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField] AppConfig config;
    AppModel _appModel = new AppModel();

    public NavbarDataManager NavbarDataManager;

    private void OnEnable()
    {
        InitManagers();
    }

    public void InitManagers() 
    {
        NavbarDataManager = new NavbarDataManager(config);
    }
}
