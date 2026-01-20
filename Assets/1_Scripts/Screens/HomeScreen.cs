using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
public class HomeScreen : UIScreen
{
    [SerializeField] private Button pitchFinderButton;
    [SerializeField] private Button myBookingButton;
    [SerializeField] private Button walletButton;
    [SerializeField] private Button lineupButton;

    protected override void SubscribeToData()
    {
        base.SubscribeToData();
        pitchFinderButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                ScreenManager.Show(Screens.PitchFinderScreen);
            })
            .AddTo(this);

        myBookingButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                ScreenManager.Show(Screens.MyBookingScreen);
            })
            .AddTo(this);
        walletButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                ScreenManager.Show(Screens.WalletScreen);
            })
            .AddTo(this);
        lineupButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                ScreenManager.Show(Screens.LineupScreen);
            })
            .AddTo(this);
    }
}
