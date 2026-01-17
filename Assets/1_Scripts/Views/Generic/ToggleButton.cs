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

    public override void Init(ToggleButtonModel initialData = default)
    {
        base.Init(initialData);
        if (initialData != null)
        {
            _previousSelectedState = initialData.selected;
            _isInitialized = true;
        }
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
            valueText.text = data.selected && !string.IsNullOrEmpty(removeStr)
                ? removeStr
                : data.name;
        }

        if (!_isInitialized)
        {
            _previousSelectedState = data.selected;
            _isInitialized = true;
            if (data.selected)
            {
                ShowAsync().Forget();
            }
            else
            {
                HideAsync().Forget();
            }
            return;
        }

        new Log($"{data.name} {_previousSelectedState} {data.selected}", "ToggleButton");
        if (_previousSelectedState != data.selected)
        {
            _previousSelectedState = data.selected;
            if (data.selected)
            {
                ShowAsync().Forget();
            }
            else
            {
                HideAsync().Forget();
            }
        }
    }
}