using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class NotificationButton : UIView
{
    [SerializeField] private Text counter;
    [SerializeField] private BaseView notificate;

    private NotificationsDataManager Notifications => DataManager.Notifications;

    protected override void Subscribe()
    {
        base.Subscribe();

        if (Notifications != null)
        {
            AddToDispose(Notifications.UnreadCount.Subscribe(count =>
            {
                UpdateUI(count);
            }));
        }
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        if (Notifications != null)
        {
            UpdateUI(Notifications.UnreadCount.Value);
        }
    }

    private void UpdateUI(int unreadCount)
    {
        if (notificate != null)
        {
            if (unreadCount > 0)
            {
                notificate.Show();
            }
            else
            {
                notificate.Hide();
            }
        }

        if (counter != null)
        {
            if (unreadCount > 0)
            {
                counter.text = unreadCount.ToString();
            }
            else
            {
                counter.text = string.Empty;
            }
        }
    }
}
