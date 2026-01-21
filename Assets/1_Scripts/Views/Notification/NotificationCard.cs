using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class NotificationCard : UIView<NotificationModel>
{
    [SerializeField] private Button markRead;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Text value;
    [SerializeField] private Text dateTime;
    [SerializeField] private BaseView notRead;
    [SerializeField] private SwipeToDelete swipeToDelete;

    protected override void Subscribe()
    {
        base.Subscribe();

        if (markRead != null)
        {
            markRead.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (DataProperty.Value != null)
                    {
                        UIManager.TriggerAction(this, DataProperty.Value.id);
                    }
                })
                .AddTo(this);
        }

        if (deleteButton != null)
        {
            deleteButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (DataProperty.Value != null)
                    {
                        UIManager.TriggerAction(this, new NotificationAction(DataProperty.Value.id, true));
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
                    UIManager.TriggerAction(this, new NotificationAction(DataProperty.Value.id, true));
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

        if (value != null)
        {
            value.text = !string.IsNullOrEmpty(data.message) ? data.message : data.type;
        }

        if (dateTime != null)
        {
            if (TryParseDateTime(data.createdAtIso, out var parsedDate))
            {
                dateTime.text = parsedDate.ToString("dd.MM.yyyy HH:mm");
            }
            else if (TryParseDateTime(data.scheduledAtIso, out var scheduledDate))
            {
                dateTime.text = scheduledDate.ToString("dd.MM.yyyy HH:mm");
            }
            else
            {
                dateTime.text = string.Empty;
            }
        }

        if (notRead != null)
        {
            if (!data.isRead)
            {
                notRead.Hide();
            }
            else
            {
                notRead.Show();
            }
        }
    }

    private bool TryParseDateTime(string iso, out DateTime dateTime)
    {
        return DateTime.TryParse(iso, out dateTime);
    }
}
