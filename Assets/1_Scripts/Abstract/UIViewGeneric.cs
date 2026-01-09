using UniRx;
using UnityEngine;

public abstract class UIView<TData> : UIView
{
    protected readonly ReactiveProperty<TData> DataProperty = new ReactiveProperty<TData>();

    protected override void OnEnable()
    {
        base.OnEnable();

        DataProperty.Subscribe(_ => UpdateUI()).AddTo(this);

        UIManager.SubscribeToView(this, (TData data) =>
        {
            DataProperty.Value = data;
        }).AddTo(this);
    }

    public virtual void Init(TData initialData = default)
    {
        DataProperty.Value = initialData;
        UpdateUI();
    }

    protected void Trigger(TData data)
    {
        UIManager.TriggerAction(this, data);
    }

    public IReadOnlyReactiveProperty<TData> Data => DataProperty;
}