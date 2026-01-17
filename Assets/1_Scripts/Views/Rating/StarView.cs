using Cysharp.Threading.Tasks;
using UnityEngine;

public class StarView : UIView<bool>
{
    private bool _previousSelectedState;
    private bool _isInitialized = false;

    public override void Init(bool initialData = default)
    {
        base.Init(initialData);
        _previousSelectedState = initialData;
        _isInitialized = true;
    }

    public override void UpdateUI()
    {
        var data = DataProperty.Value;
        new Log($"{transform.GetEntityId()} {data}", "StarView");
        if (!_isInitialized)
        {
            _previousSelectedState = data;
            _isInitialized = true;
            
            if (gameObject.activeSelf)
            {
                if (data)
                {
                    ShowAsync().Forget();
                }
                else
                {
                    HideAsync().Forget();
                }
            }
            return;
        }

        if (_previousSelectedState != data && gameObject.activeSelf)
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