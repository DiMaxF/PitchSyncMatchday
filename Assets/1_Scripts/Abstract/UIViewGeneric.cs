using UniRx;
using UnityEngine;

public abstract class UIView<TData> : UIView
{
    protected readonly ReactiveProperty<TData> DataProperty = new ReactiveProperty<TData>();

    protected override void OnEnable()
    {
        base.OnEnable();

        DataProperty
         .Where(data => data != null) 
         .Subscribe(_ => UpdateUI())
         .AddTo(this);

        UIManager.SubscribeToView(this, (TData data) =>
        {
            DataProperty.Value = data;
        }).AddTo(this);
    }

    public virtual void Init(TData initialData = default)
    {
        DataProperty.Value = initialData;

        if (initialData != null)  UpdateUI();
    }
    public void Init(object data)
    {
        if (data is TData typedData)
        {
            Init(typedData);
        }
        else
        {
            new Error($"Type mismatch in {this.GetType().Name}: expected {typeof(TData).Name}, got {(data != null ? data.GetType().Name : "null")}", "IView<TData>");
        }
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