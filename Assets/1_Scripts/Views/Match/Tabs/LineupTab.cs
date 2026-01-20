using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class LineupTab : UIView
{
    [SerializeField] private MatchSchemeView schemeView;
    [SerializeField] private Button shareLineupImage;

    private MatchCenterDataManager MatchCenter => DataManager.MatchCenter;

    protected override void Subscribe()
    {
        base.Subscribe();
        if (schemeView != null)
        {
            AddToDispose(MatchCenter.CurrentLineup.Subscribe(lineup =>
            {
                if (lineup != null)
                {
                    schemeView.Init(lineup);
                }
            }));
        }
    }
}
