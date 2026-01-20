using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class NavbarButton : UIView<NavbarButtonModel>
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button action;
    [SerializeField] private Text label;

    private bool _previousSelectedState;
    private bool _isInitialized = false;
    protected override bool ListenToSelfEvents => false;

    public override void Init(NavbarButtonModel initialData = default)
    {
        _isInitialized = false;
        _previousSelectedState = false;
        base.Init(initialData);
    }

    protected override void Subscribe() 
    {
        base.Subscribe();
        action.OnClickAsObservable()
            .Subscribe(_ =>
            {
                Trigger(DataProperty.Value);
            })
            .AddTo(this);
    }

    public override void UpdateUI()
    {
        var data = DataProperty.Value;
        if (data == null) return;

        if (iconImage != null) iconImage.sprite = data.icon;
        if (label != null) label.text = data.label;

        if (!_isInitialized)
        {
            _previousSelectedState = data.selected;
            _isInitialized = true;
            
            if (data.selected)
            {
                Show();
            }
            else
            {
                Hide();
            }
            return;
        }

        if (_previousSelectedState != data.selected)
        {
            _previousSelectedState = data.selected;
            if (data.selected)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }
    }

    public override async UniTask HideAsync()
    {
        if (_animController != null)
        {
            await _animController.PlayAsync(false);
        }
    }
}
