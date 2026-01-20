public class NotificationAction
{
    public int notificationId;
    public bool isDelete;

    public NotificationAction(int id, bool delete)
    {
        notificationId = id;
        isDelete = delete;
    }
}

