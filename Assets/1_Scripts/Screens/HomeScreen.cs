using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
public class HomeScreen : UIScreen
{
    [SerializeField] private Button pitchFinderButton;

    protected override void SubscribeToData()
    {
        base.SubscribeToData();
        pitchFinderButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                ScreenManager.Show(Screens.PitchFinderScreen);
               //DataManager.Navigation.SelectScreen(Screens.PitchFinderScreen);
            })
            .AddTo(this);
    }
}
