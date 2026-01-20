using System;

[Serializable]
public class NotificationModel
{
    public int id;
    public string type;
    public string message;
    public string scheduledAtIso;
    public string createdAtIso;
    public bool delivered;
    public bool isRead;

    public int? bookingId;
    public int? matchId;
}

