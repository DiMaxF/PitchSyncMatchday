using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : UIView<ToggleButtonModel>
{
    [SerializeField] private Button button;
    [SerializeField] private Text valueText;
    [SerializeField] private string removeStr;

    private bool _previousSelectedState;
    private bool _isInitialized = false;
    protected override bool ListenToSelfEvents => false;
    public override void Init(ToggleButtonModel initialData = default)
    {
        _isInitialized = false;
        _previousSelectedState = false;
        base.Init(initialData);
    }

    protected override void Subscribe()
    {
        base.Subscribe();
        if (button != null)
        {
            button.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Trigger(DataProperty.Value);
                })
                .AddTo(this);
        }
    }

    public override void UpdateUI()
    {
        var data = DataProperty.Value;
        if (data == null) return;

        if (valueText != null)
        {
            valueText.text = removeStr.Equals("") ? data.name : data.name.Replace(removeStr, "");
        }

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