using System;
using UnityEngine;

[Serializable]
public class MatchEvent
{
    public int minute;
    public string type;
    public TeamSide team;
    public string description;

    public MatchEvent(int minute, string type, TeamSide team, string desc = "")
    {
        this.minute = minute;
        this.type = type;
        this.team = team;
        this.description = desc;
    }
}