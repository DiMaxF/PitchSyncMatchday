using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class NotificationsDataManager : IDataManager
{
    private readonly AppModel _appModel;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    private IDisposable _tick;

    public ReactiveCollection<NotificationModel> Queue { get; } = new ReactiveCollection<NotificationModel>();
    private readonly ReactiveCollection<object> _queueAsObject = new ReactiveCollection<object>();
    public ReactiveCollection<object> QueueAsObject => _queueAsObject;
    public ReactiveProperty<int> UnreadCount { get; } = new ReactiveProperty<int>(0);

    public NotificationsDataManager(AppModel appModel)
    {
        _appModel = appModel;

        if (_appModel.notifications == null)
        {
            _appModel.notifications = new List<NotificationModel>();
        }

        BindQueue();
        LoadFromAppModel();

        DataManager.Profile.NotificationsEnabled
            .Subscribe(enabled =>
            {
                if (enabled)
                {
                    ProcessDueNotifications();
                }
            })
            .AddTo(_disposables);

        _tick = Observable.Interval(TimeSpan.FromSeconds(30))
            .Subscribe(_ => ProcessDueNotifications())
            .AddTo(_disposables);
    }

    private void BindQueue()
    {
        _queueAsObject.Clear();
        foreach (var item in Queue) _queueAsObject.Add(item);

        Queue.ObserveAdd().Subscribe(e =>
        {
            _queueAsObject.Insert(e.Index, e.Value);
            UpdateUnreadCount();
        }).AddTo(_disposables);
        
        Queue.ObserveRemove().Subscribe(e =>
        {
            _queueAsObject.RemoveAt(e.Index);
            UpdateUnreadCount();
        }).AddTo(_disposables);
        
        Queue.ObserveReplace().Subscribe(e =>
        {
            _queueAsObject[e.Index] = e.NewValue;
            UpdateUnreadCount();
        }).AddTo(_disposables);
        
        Queue.ObserveMove().Subscribe(e => _queueAsObject.Move(e.OldIndex, e.NewIndex)).AddTo(_disposables);
        
        Queue.ObserveReset().Subscribe(_ =>
        {
            _queueAsObject.Clear();
            foreach (var item in Queue) _queueAsObject.Add(item);
            UpdateUnreadCount();
        }).AddTo(_disposables);
        
        UpdateUnreadCount();
    }

    private void LoadFromAppModel()
    {
        Queue.Clear();
        var sorted = _appModel.notifications
            .OrderBy(n => n.isRead)
            .ThenByDescending(n => TryParseDateTime(n.createdAtIso, out var dt) ? dt : DateTime.MinValue)
            .ToList();
        
        foreach (var n in sorted)
        {
            Queue.Add(n);
        }
        UpdateUnreadCount();
    }

    public void ScheduleBookingNotifications(BookingModel booking)
    {
        if (booking == null) return;
        if (string.IsNullOrEmpty(booking.dateTimeIso)) return;

        if (!TryParseDateTime(booking.dateTimeIso, out var bookingTime)) return;
        var stadium = DataManager.PitchFinder.GetStadiumById(booking.stadiumId);
        var stadiumName = stadium != null ? stadium.name : "Unknown Pitch";

        var matchSoonTime = bookingTime.AddMinutes(-30);
        if (matchSoonTime > DateTime.Now)
        {
            EnqueueIfMissing(NotificationType.MatchSoon, matchSoonTime,
                $"Матч в {stadiumName} через 30 минут. Не забудьте QR и lineup!",
                booking.id, null);
        }

        var checkInTime = bookingTime.AddMinutes(-15);
        if (checkInTime > DateTime.Now)
        {
            EnqueueIfMissing(NotificationType.CheckIn, checkInTime,
                "Пора на поле! Покажите QR на входе.",
                booking.id, null);
        }
    }

    public void ScheduleMatchFinished(MatchModel match)
    {
        if (match == null) return;
        var scoreText = $"{match.scoreBlue}:{match.scoreOrange}";
        var message = $"Матч завершён! Счёт {scoreText}. Проверьте wallet и notes.";
        EnqueueIfMissing(NotificationType.MatchFinished, DateTime.Now, message,
            null, match.id);
    }

    public void ScheduleWalletRemindersAfterFinish(MatchModel match)
    {
        if (match == null) return;

        var remaining = GetWalletRemaining();
        if (remaining <= 0f) return;

        var endTime = DateTime.Now;
        if (!string.IsNullOrEmpty(match.endTimeIso) && TryParseDateTime(match.endTimeIso, out var parsedEnd))
        {
            endTime = parsedEnd;
        }

        var nextEvening = GetNextEvening(endTime);
        EnqueueIfMissing(NotificationType.WalletRemainingDaily, nextEvening,
            string.Empty, null, match.id);

        var unpaidTime = endTime.AddHours(24);
        EnqueueIfMissing(NotificationType.WalletUnpaid24h, unpaidTime,
            string.Empty, null, match.id);
    }

    public void ProcessDueNotifications()
    {
        if (!DataManager.Profile.NotificationsEnabled.Value) return;

        var now = DateTime.Now;
        var due = Queue.Where(n => !n.delivered && TryParseDateTime(n.scheduledAtIso, out var dt) && dt <= now).ToList();

        foreach (var notification in due)
        {
            if (!ShouldTrigger(notification))
            {
                MarkDelivered(notification);
                continue;
            }

            notification.message = BuildMessage(notification);
            TriggerNotification(notification);
            MarkDelivered(notification);

            if (notification.type == NotificationType.WalletRemainingDaily.ToString())
            {
                var remaining = GetWalletRemaining();
                if (remaining > 0f)
                {
                    var nextDay = DateTime.Now.Date.AddDays(1).AddHours(20);
                    EnqueueIfMissing(NotificationType.WalletRemainingDaily, nextDay,
                        string.Empty, null, notification.matchId);
                }
            }
        }
    }

    private bool ShouldTrigger(NotificationModel notification)
    {
        if (notification.type == NotificationType.WalletRemainingDaily.ToString() ||
            notification.type == NotificationType.WalletUnpaid24h.ToString())
        {
            return GetWalletRemaining() > 0f;
        }

        return true;
    }

    private string BuildMessage(NotificationModel notification)
    {
        if (notification.type == NotificationType.WalletRemainingDaily.ToString())
        {
            var remaining = GetWalletRemaining();
            return $"В wallet матча осталось ${remaining:F0}. Кто-то ещё не скинулся?";
        }

        if (notification.type == NotificationType.WalletUnpaid24h.ToString())
        {
            var remaining = GetWalletRemaining();
            return $"Не забудьте собрать ${remaining:F0} с участников.";
        }

        return notification.message ?? string.Empty;
    }

    private void TriggerNotification(NotificationModel notification)
    {
        Debug.Log($"[Notification] {notification.message}");
        DataManager.Instance.SaveAppModel();
    }

    public void MarkRead(int notificationId)
    {
        var notification = Queue.FirstOrDefault(n => n.id == notificationId);
        if (notification == null) return;
        if (notification.isRead) return;

        notification.isRead = true;
        ReplaceInQueue(notification);
        SortQueue();
        DataManager.Instance.SaveAppModel();
    }

    public void MarkAllRead()
    {
        bool changed = false;
        for (int i = 0; i < Queue.Count; i++)
        {
            var notification = Queue[i];
            if (!notification.isRead)
            {
                notification.isRead = true;
                Queue[i] = notification;
                changed = true;
            }
        }

        if (changed)
        {
            SortQueue();
            DataManager.Instance.SaveAppModel();
        }
    }

    public void DeleteNotification(int notificationId)
    {
        var notification = Queue.FirstOrDefault(n => n.id == notificationId);
        if (notification == null) return;

        Queue.Remove(notification);
        _appModel.notifications.RemoveAll(n => n.id == notificationId);
        DataManager.Instance.SaveAppModel();
    }

    public void DeleteAllNotifications()
    {
        Queue.Clear();
        _appModel.notifications.Clear();
        DataManager.Instance.SaveAppModel();
    }

    private void EnqueueIfMissing(NotificationType type, DateTime scheduledAt, string message, int? bookingId, int? matchId)
    {
        var scheduledAtIso = scheduledAt.ToString("o");
        if (HasNotification(type, scheduledAtIso, bookingId, matchId)) return;

        var model = new NotificationModel
        {
            id = IdGenerator.GetNextId(_appModel, "Notification"),
            type = type.ToString(),
            message = message,
            scheduledAtIso = scheduledAtIso,
            createdAtIso = DateTime.Now.ToString("o"),
            delivered = false,
            bookingId = bookingId,
            matchId = matchId
        };

        Queue.Add(model);
        _appModel.notifications.Add(model);
        SortQueue();
        DataManager.Instance.SaveAppModel();
    }

    private bool HasNotification(NotificationType type, string scheduledAtIso, int? bookingId, int? matchId)
    {
        return _appModel.notifications.Any(n =>
            n.type == type.ToString() &&
            n.scheduledAtIso == scheduledAtIso &&
            n.bookingId == bookingId &&
            n.matchId == matchId &&
            !n.delivered);
    }

    private void MarkDelivered(NotificationModel notification)
    {
        notification.delivered = true;
        ReplaceInQueue(notification);
        DataManager.Instance.SaveAppModel();
    }

    private void ReplaceInQueue(NotificationModel notification)
    {
        for (int i = 0; i < Queue.Count; i++)
        {
            if (Queue[i].id == notification.id)
            {
                Queue[i] = notification;
                break;
            }
        }
    }

    private bool TryParseDateTime(string iso, out DateTime dateTime)
    {
        return DateTime.TryParse(iso, out dateTime);
    }

    private float GetWalletRemaining()
    {
        var wallet = _appModel.wallet;
        if (wallet == null) return 0f;
        return Mathf.Max(0f, wallet.totalCost - wallet.totalPaid);
    }

    private DateTime GetNextEvening(DateTime fromTime)
    {
        var evening = fromTime.Date.AddHours(20);
        if (fromTime <= evening)
        {
            return evening;
        }
        return fromTime.Date.AddDays(1).AddHours(20);
    }

    private void SortQueue()
    {
        var sorted = Queue
            .OrderBy(n => n.isRead)
            .ThenByDescending(n => TryParseDateTime(n.createdAtIso, out var dt) ? dt : DateTime.MinValue)
            .ToList();

        Queue.Clear();
        foreach (var n in sorted)
        {
            Queue.Add(n);
        }
        UpdateUnreadCount();
    }

    private void UpdateUnreadCount()
    {
        var count = Queue.Count(n => !n.isRead);
        UnreadCount.Value = count;
    }

    public void Dispose()
    {
        _tick?.Dispose();
        _disposables.Dispose();
    }
}

