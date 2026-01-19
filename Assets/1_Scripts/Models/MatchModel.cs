using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MatchModel
{
    public int id;
    public int? bookingId;
    public MatchStatus status = MatchStatus.Upcoming;
    public int scoreBlue;
    public int scoreOrange;
    public string startTimeIso;
    public string endTimeIso;
    public string notes = "";
    public List<MatchEvent> events = new List<MatchEvent>();
    public int? lineupId;
    public string pitchName = "";
    public PitchSize pitchSize = PitchSize.Size7x7;
    public int totalDurationMinutes = 60;
    public int elapsedSeconds = 0;

    public MatchModel() { }

    public MatchModel(int? bookingId = null)
    {
        id = GenerateId();
        this.bookingId = bookingId;
    }

    private static int GenerateId() => (int)(DateTime.UtcNow.Ticks & 0xFFFFFFFF);
}