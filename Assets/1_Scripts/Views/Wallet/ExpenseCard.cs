using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ExpenseCard : UIView<ExpenseModel>
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text amountText;
    [SerializeField] private SwipeToDelete swipeToDelete;
    [SerializeField] private Button deleteButton;

    protected override void Subscribe()
    {
        base.Subscribe();

        if (deleteButton != null)
        {
            deleteButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (DataProperty.Value != null)
                    {
                        UIManager.TriggerAction(this, DataProperty.Value.id);
                    }
                })
                .AddTo(this);
        }

        if (swipeToDelete != null)
        {
            System.Action onDelete = () =>
            {
                if (DataProperty.Value != null)
                {
                    UIManager.TriggerAction(this, DataProperty.Value.id);
                }
            };

            swipeToDelete.OnDelete += onDelete;
            AddToDispose(Disposable.Create(() => swipeToDelete.OnDelete -= onDelete));
        }
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = DataProperty.Value;
        if (data == null) return;

        if (amountText != null)
        {
            amountText.text = $"${data.amount}";
        }
        if (nameText != null)
        {
            nameText.text = $"{data.name}";
        }

        /*if (icon != null && data.icon != null)
        {
            //icon.sprite = data.;
        }*/
    }
}
