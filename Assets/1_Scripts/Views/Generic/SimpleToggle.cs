using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SimpleToggle : UIView<bool>
{
    [SerializeField] private Button action;

    private bool _previousSelectedState;
    private bool _isInitialized = false;
    protected override bool ListenToSelfEvents => false;

    public override void Init(bool initialData = default)
    {
        _isInitialized = false;
        _previousSelectedState = false;
        base.Init(initialData);
    }

    protected override void Subscribe()
    {
        base.Subscribe();
        if (action != null)
        {
            action.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Trigger(!DataProperty.Value);
                })
                .AddTo(this);
        }
    }

    public override void UpdateUI()
    {
        var data = DataProperty.Value;

        if (!_isInitialized)
        {
            _previousSelectedState = data;
            _isInitialized = true;
            if (data)
            {
                ShowAsync().Forget();
            }
            else
            {
                HideAsync().Forget();
            }
            return;
        }

        if (_previousSelectedState != data)
        {
            _previousSelectedState = data;
            if (data)
            {
                ShowAsync().Forget();
            }
            else
            {
                HideAsync().Forget();
            }
        }
    }

    public override async UniTask HideAsync()
    {
        var animController = GetComponent<AnimationController>();
        if (animController != null)
            await animController.PlayAsync(false);
    }
}
