using System;
using UniRx;
using UnityEngine;

public abstract class UIView<TData> : UIView
{
    protected readonly ReactiveProperty<TData> DataProperty = new ReactiveProperty<TData>();

    private IDisposable _updateSubscription;

    protected virtual bool ListenToSelfEvents => true;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (!ListenToSelfEvents) return;

        UIManager.SubscribeToView(this, (TData data) =>
        {
            DataProperty.Value = data;
        }).AddTo(this);
    }

    public virtual void Init(TData initialData = default)
    {
        SubscribeToDataChanges();
        DataProperty.Value = initialData;
    }

    public void Init(object data)
    {
        if (data is TData typedData)
        {
            Init(typedData);
        }
        else
        {
            new Error($"Type mismatch in {this.GetType().Name}: expected {typeof(TData).Name}, got {(data != null ? data.GetType().Name : "null")}", "UIView<TData>");
        }
    }

    protected void SubscribeToDataChanges()
    {
        if (_updateSubscription != null) return;

        _updateSubscription = DataProperty
            .Where(data => data != null)
            .Subscribe(_ => UpdateUI())
            .AddTo(this);
    }

    public override void UpdateUI()
    {
        if (DataProperty.Value == null) return;
    }

    protected void Trigger(TData data)
    {
        UIManager.TriggerAction(this, data);
    }

    public IReadOnlyReactiveProperty<TData> Data => DataProperty;
}