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

    public LineupModel(int? matchId, AppModel appModel)
    {
        id = IdGenerator.GetNextId(appModel, "Lineup");
        this.matchId = matchId;
    }
}