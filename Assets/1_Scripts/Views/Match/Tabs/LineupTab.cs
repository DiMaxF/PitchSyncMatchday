using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class LineupTab : UIView
{
    [SerializeField] private MatchSchemeView schemeView;
    [SerializeField] private Button shareLineupImage;
    [SerializeField] private SquadPanel redSquad;
    [SerializeField] private SquadPanel greenSquad;

    private MatchCenterDataManager MatchCenter => DataManager.MatchCenter;
    private LineupDataManager Lineup => DataManager.Lineup;

    protected override void Subscribe()
    {
        base.Subscribe();
        
        if (schemeView != null)
        {
            AddToDispose(MatchCenter.CurrentLineup.Subscribe(lineup =>
            {
                if (lineup != null)
                {
                    schemeView.Init(lineup);
                    UpdateSquadPanels(lineup);
                }
            }));
        }
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        var lineup = MatchCenter.CurrentLineup.Value;
        if (lineup != null)
        {
            UpdateSquadPanels(lineup);
        }
    }

    private void UpdateSquadPanels(LineupModel lineup)
    {
        if (greenSquad != null)
        {
            var greenPlayers = BuildSquadPlayers(lineup.playersBlue, TeamSide.Green, lineup.captainBlue);
            var greenModel = new SquadPanelModel
            {
                teamSide = TeamSide.Green,
                players = greenPlayers,
                squadIcon = Lineup.GetTeamIconForSide(TeamSide.Green),
                playerCount = greenPlayers.Count
            };
            greenSquad.Init(greenModel);
            
            if (greenSquad.playersList != null)
            {
                var greenAsObject = greenPlayers.Cast<object>().ToList();
                greenSquad.playersList.Init(greenAsObject);
            }
        }

        if (redSquad != null)
        {
            var redPlayers = BuildSquadPlayers(lineup.playersRed, TeamSide.Red, lineup.captainRed);
            var redModel = new SquadPanelModel
            {
                teamSide = TeamSide.Red,
                players = redPlayers,
                squadIcon = Lineup.GetTeamIconForSide(TeamSide.Red),
                playerCount = redPlayers.Count
            };
            redSquad.Init(redModel);
            
            if (redSquad.playersList != null)
            {
                var redAsObject = redPlayers.Cast<object>().ToList();
                redSquad.playersList.Init(redAsObject);
            }
        }
    }

    private List<SquadPlayerModel> BuildSquadPlayers(List<int> ids, TeamSide side, int? captainId)
    {
        var result = new List<SquadPlayerModel>();
        int idx = 1;

        foreach (var id in ids)
        {
            var player = Lineup.GetPlayerById(id);
            if (player == null) continue;

            result.Add(new SquadPlayerModel
            {
                playerId = player.id,
                name = player.name,
                position = player.position,
                squadNumber = idx++,
                isCaptain = captainId.HasValue && captainId.Value == player.id,
                teamSide = side,
                teamIcon = Lineup.GetTeamIconForSide(side)
            });
        }

        return result;
    }
}
