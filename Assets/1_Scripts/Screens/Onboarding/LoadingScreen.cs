using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : UIScreen
{
    protected override void OnEnable()
    {
        base.OnEnable();
        Application.targetFrameRate = 60;
    }

    protected override void SubscribeToData()
    {
        base.SubscribeToData();

        ShowAsync().Forget();
        WaitForLoad();
    }

    private async void WaitForLoad()
    {
        await UniTask.Delay(2893);
        
        if (ShouldSkipOnboarding())
        {
            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            ScreenManager?.Show(Screens.OnboradingScreen);
        }
    }

    private bool ShouldSkipOnboarding()
    {
        if (DataManager.Instance == null || DataManager.Profile == null)
        {
            return false;
        }

        bool hasDefaultPitchSize = DataManager.Profile.DefaultPitchSize.Value != PitchSize.Size7x7;
        bool hasDefaultDuration = DataManager.Profile.DefaultDuration.Value != MatchDuration.Min60;
        
        return hasDefaultPitchSize || hasDefaultDuration;
    }
}
