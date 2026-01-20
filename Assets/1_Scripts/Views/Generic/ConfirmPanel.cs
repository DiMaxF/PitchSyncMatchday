using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPanel : UIView<bool>
{
    [SerializeField] private Text title;
    [SerializeField] private Text subtitle;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;


    ConfirmPanelModel model;

    protected override void Subscribe()
    {
        base.Subscribe();

        if (acceptButton != null)
        {
            acceptButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Trigger(true);
                    Hide();
                })
                .AddTo(this);
        }

        if (declineButton != null)
        {
            declineButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Trigger(false);
                    Hide();
                })
                .AddTo(this);
        }
    }

    public void Init(ConfirmPanelModel model) 
    {
        this.model = model;
        UpdateUI();
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = model;
        if (data == null) return;

        if (title != null)
        {
            title.text = data.title ?? "";
        }

        if (subtitle != null)
        {
            subtitle.text = data.subtitle ?? "";
        }


    }
}
