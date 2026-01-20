using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class NotificationsScreen : UIScreen
{
    [SerializeField] private ListContainer notifications;
    [SerializeField] private Button markAllRead;
    [SerializeField] private Button deleteAll;
    [SerializeField] private Button back;
    [SerializeField] private ConfirmPanel confirmPanel;

    private NotificationsDataManager Notifications => DataManager.Notifications;

    protected override void OnEnable()
    {
        base.OnEnable();
        if (confirmPanel != null) confirmPanel.gameObject.SetActive(false);
    }

    protected override void SubscribeToData()
    {
        base.SubscribeToData();

        if (notifications != null)
        {
            notifications.Init(Notifications.QueueAsObject);

            AddToDispose(UIManager.SubscribeToView(notifications, (int notificationId) =>
            {
                Notifications.MarkRead(notificationId);
            }));

            AddToDispose(UIManager.SubscribeToView(notifications, (NotificationAction action) =>
            {
                if (action != null && action.isDelete)
                {
                    Notifications.DeleteNotification(action.notificationId);
                }
            }));
        }

        if (markAllRead != null)
        {
            AddToDispose(markAllRead.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Notifications.MarkAllRead();
                }));
        }

        if (deleteAll != null)
        {
            AddToDispose(deleteAll.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    ShowConfirmDeleteAll();
                }));
        }

        if (confirmPanel != null)
        {
            AddToDispose(UIManager.SubscribeToView(confirmPanel, (bool confirmed) =>
            {
                if (confirmed)
                {
                    Notifications.DeleteAllNotifications();
                }
            }));
        }

        if (back != null)
        {
            AddToDispose(back.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    ScreenManager?.Back();
                }));
        }
    }

    private void ShowConfirmDeleteAll()
    {
        if (confirmPanel == null) return;

        var model = new ConfirmPanelModel
        {
            title = "Удалить все уведомления",
            subtitle = "Вы уверены, что хотите удалить все уведомления?",
            acceptText = "Удалить",
            declineText = "Отмена"
        };

        confirmPanel.Init(model);
        confirmPanel.Show();
    }
}
