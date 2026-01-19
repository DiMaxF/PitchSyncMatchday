using System;
using UnityEngine;

[Serializable]
public class MatchEvent
{
    public int minute;
    public int elapsedSeconds;
    public string type;
    public TeamSide team;
    public string description;
    public int? playerId;

    public MatchEvent() { }

    public MatchEvent(int elapsedSeconds, MatchEventType eventType, TeamSide team, int? playerId = null, string desc = "")
    {
        this.elapsedSeconds = elapsedSeconds;
        this.minute = elapsedSeconds / 60;
        this.type = eventType.ToString();
        this.team = team;
        this.playerId = playerId;
        this.description = desc;
    }
}