using Cysharp.Threading.Tasks;
using UnityEngine;

public class LoadingScreen : UIScreen
{
    protected override void OnEnable()
    {
        base.OnEnable();
        Application.targetFrameRate = 60;

    }

    private void Start()
    {
        ScreenManager?.Show(Screens.LoadingScreen);
        WaitForLoad();
    }

    private async void WaitForLoad() 
    {
        await UniTask.Delay(2893);
        ScreenManager?.Show(Screens.OnboradingScreen);
    }
}
