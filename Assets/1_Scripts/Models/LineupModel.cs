using System;
using System.Collections.Generic;

[Serializable]
public class LineupModel
{
    public int id;
    public int? matchId;
    public List<int> playersBlue = new List<int>();
    public List<int> playersRed = new List<int>();
    public int? captainBlue;
    public int? captainRed;

    public LineupModel() { }

    public LineupModel(int? matchId = null)
    {
        id = GenerateId();
        this.matchId = matchId;
    }

    private static int GenerateId() => (int)(DateTime.UtcNow.Ticks & 0xFFFFFFFF);
}