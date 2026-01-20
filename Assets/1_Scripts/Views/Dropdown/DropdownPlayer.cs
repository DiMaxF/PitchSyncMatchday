using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class DropdownPlayer : UIView<PlayerModel>
{
    [SerializeField] private Button action;
    [SerializeField] private Text name;

    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = DataProperty.Value;
        if (data == null) return;
        if (name != null)
        {
            name.text = data.name;
        }
        if (action != null)
        {
            action.OnClickAsObservable  ()
                .Subscribe(_ => Trigger(data))
                .AddTo(this);
        }
    }
}
