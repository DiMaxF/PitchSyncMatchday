using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class CounterView : UIView<int>
{
    [SerializeField] private Text value;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;

    protected override void Subscribe()
    {
        base.Subscribe();
        plusButton.OnClickAsObservable().Subscribe(_ =>
        {
            DataProperty.Value++;
            Trigger(DataProperty.Value);
        }).AddTo(this);
        minusButton.OnClickAsObservable().Subscribe(_ =>
        {
            if (DataProperty.Value > 0)
            {
                DataProperty.Value--;
                Trigger(DataProperty.Value);
            }
        }).AddTo(this);
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = DataProperty.Value;
        if (data < 0)
        {
            data = 0;
            DataProperty.Value = 0;
        }
        value.text = data.ToString();   

    }
}
