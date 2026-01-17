using Cysharp.Threading.Tasks;
using UnityEngine;

public class StarView : UIView<bool>
{
    private bool _previousSelectedState;
    private bool _isInitialized = false;
    protected override bool ListenToSelfEvents => false;
    public override void Init(bool initialData = default)
    {
        _isInitialized = false;
        _previousSelectedState = false;
        base.Init(initialData);
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
}