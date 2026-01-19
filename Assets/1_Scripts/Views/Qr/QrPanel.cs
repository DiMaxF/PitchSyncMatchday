using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class QrPanel : UIView<Sprite>
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Image qrImage;

    protected override void Subscribe()
    {
        base.Subscribe();
        
        if (closeButton != null)
        {
            closeButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    new Log("Click", "QrPanel");
                    Hide();
                    //HideAsync().Forget();
                })
                .AddTo(this);
        }
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        
        if (qrImage != null && DataProperty.Value != null)
        {
            qrImage.sprite = DataProperty.Value;
        }
    }
}
