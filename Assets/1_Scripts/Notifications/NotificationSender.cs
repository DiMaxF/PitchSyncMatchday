using System;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using Unity.Notifications.Android;
#elif UNITY_IOS && !UNITY_EDITOR
using Unity.Notifications.iOS;
#endif

public class NotificationSender : MonoBehaviour
{
    public static NotificationSender Instance { get; private set; }

    private const string ANDROID_CHANNEL_ID = "default_channel";
    private const string ANDROID_CHANNEL_NAME = "Default Channel";
    private const string ANDROID_CHANNEL_DESCRIPTION = "Default notifications channel";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    private void Initialize()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        InitializeAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
        InitializeIOS();
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void InitializeAndroid()
    {
        var channel = new AndroidNotificationChannel
        {
            Id = ANDROID_CHANNEL_ID,
            Name = ANDROID_CHANNEL_NAME,
            Description = ANDROID_CHANNEL_DESCRIPTION,
            Importance = Importance.Default
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }
#endif

#if UNITY_IOS && !UNITY_EDITOR
    private void InitializeIOS()
    {
    }
#endif

    public void SendNotification(string title, string message, int notificationId)
    {
        if (!NotificationPermissionManager.Instance?.HasPermission() ?? true)
        {
            new Log("Notification permission not granted, skipping notification", "NotificationSender");
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        SendAndroidNotification(title, message, notificationId);
#elif UNITY_IOS && !UNITY_EDITOR
        SendIOSNotification(title, message, notificationId);
#else
        new Log($"{title}: {message}", "Notification");
#endif
    }

    public void ScheduleNotification(string title, string message, DateTime scheduledTime, int notificationId)
    {
        if (!NotificationPermissionManager.Instance?.HasPermission() ?? true)
        {
            new Log("Notification permission not granted, skipping scheduled notification", "NotificationSender");
            return;
        }

        if (scheduledTime <= DateTime.Now)
        {
            SendNotification(title, message, notificationId);
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        ScheduleAndroidNotification(title, message, scheduledTime, notificationId);
#elif UNITY_IOS && !UNITY_EDITOR
        ScheduleIOSNotification(title, message, scheduledTime, notificationId);
#else
        new Log($"{title}: {message} at {scheduledTime}", "Scheduled Notification");
#endif
    }

    public void CancelNotification(int notificationId)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidNotificationCenter.CancelNotification(notificationId);
#elif UNITY_IOS && !UNITY_EDITOR
        iOSNotificationCenter.RemoveScheduledNotification(notificationId.ToString());
        iOSNotificationCenter.RemoveDeliveredNotification(notificationId.ToString());
#else
        new Log($"ID: {notificationId}", "Cancel Notification");
#endif
    }

    public void CancelAllNotifications()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidNotificationCenter.CancelAllNotifications();
#elif UNITY_IOS && !UNITY_EDITOR
        iOSNotificationCenter.RemoveAllScheduledNotifications();
        iOSNotificationCenter.RemoveAllDeliveredNotifications();
#else
        new Log("All notifications cancelled", "Cancel All Notifications");
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void SendAndroidNotification(string title, string message, int notificationId)
    {
        var notification = new AndroidNotification
        {
            Title = title,
            Text = message,
            SmallIcon = "icon_small",
            LargeIcon = "icon_large",
            FireTime = DateTime.Now
        };

        AndroidNotificationCenter.SendNotificationWithExplicitID(notification, ANDROID_CHANNEL_ID, notificationId);
    }

    private void ScheduleAndroidNotification(string title, string message, DateTime scheduledTime, int notificationId)
    {
        var notification = new AndroidNotification
        {
            Title = title,
            Text = message,
            SmallIcon = "icon_small",
            LargeIcon = "icon_large",
            FireTime = scheduledTime
        };

        AndroidNotificationCenter.SendNotificationWithExplicitID(notification, ANDROID_CHANNEL_ID, notificationId);
    }
#endif

#if UNITY_IOS && !UNITY_EDITOR
    private void SendIOSNotification(string title, string message, int notificationId)
    {
        var timeTrigger = new iOSNotificationTimeIntervalTrigger
        {
            TimeInterval = TimeSpan.FromSeconds(1),
            Repeats = false
        };

        var notification = new iOSNotification
        {
            Identifier = notificationId.ToString(),
            Title = title,
            Body = message,
            Trigger = timeTrigger
        };

        iOSNotificationCenter.ScheduleNotification(notification);
    }

    private void ScheduleIOSNotification(string title, string message, DateTime scheduledTime, int notificationId)
    {
        if (scheduledTime <= DateTime.Now)
        {
            SendIOSNotification(title, message, notificationId);
            return;
        }

        var calendarTrigger = new iOSNotificationCalendarTrigger
        {
            Year = scheduledTime.Year,
            Month = scheduledTime.Month,
            Day = scheduledTime.Day,
            Hour = scheduledTime.Hour,
            Minute = scheduledTime.Minute,
            Second = scheduledTime.Second,
            Repeats = false
        };

        var notification = new iOSNotification
        {
            Identifier = notificationId.ToString(),
            Title = title,
            Body = message,
            Trigger = calendarTrigger
        };

        iOSNotificationCenter.ScheduleNotification(notification);
    }
#endif
}

