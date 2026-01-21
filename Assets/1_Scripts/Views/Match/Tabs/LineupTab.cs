using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class LineupTab : UIView
{
    [SerializeField] private MatchSchemeView schemeView;
    [SerializeField] private Button shareLineupImage;
    [SerializeField] private SquadPanel redSquad;
    [SerializeField] private SquadPanel greenSquad;
    [SerializeField] private Button createLineupButton;
    [SerializeField] private GameObject lineupEmptyState;
    [SerializeField] private Button shareLineUp;

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
                    SyncLineupToSquads(lineup);
                    UpdateSquadPanels();
                }
                UpdateEmptyState();
            }));
        }

        if (createLineupButton != null)
        {
            createLineupButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    MatchCenter.RequestLineupForCurrentMatch();
                    DataManager.Navigation.SelectScreen(Screens.LineupScreen);
                })
                .AddTo(this);
        }

        if (greenSquad != null)
        {
            AddToDispose(Lineup.SquadGreen.ObserveAdd().Subscribe(_ => UpdateSquadPanels()));
            AddToDispose(Lineup.SquadGreen.ObserveRemove().Subscribe(_ => UpdateSquadPanels()));
            AddToDispose(Lineup.SquadGreen.ObserveReplace().Subscribe(_ => UpdateSquadPanels()));
            AddToDispose(Lineup.SquadGreen.ObserveReset().Subscribe(_ => UpdateSquadPanels()));
        }

        if (redSquad != null)
        {
            AddToDispose(Lineup.SquadRed.ObserveAdd().Subscribe(_ => UpdateSquadPanels()));
            AddToDispose(Lineup.SquadRed.ObserveRemove().Subscribe(_ => UpdateSquadPanels()));
            AddToDispose(Lineup.SquadRed.ObserveReplace().Subscribe(_ => UpdateSquadPanels()));
            AddToDispose(Lineup.SquadRed.ObserveReset().Subscribe(_ => UpdateSquadPanels()));
        }

        if (shareLineUp != null)
        {
            AddToDispose(shareLineUp.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (schemeView != null && MatchCenter.CurrentLineup.Value != null)
                    {
                        ShareLineupImage();
                    }
                }));
        }
    }

    private void ShareLineupImage()
    {
        if (schemeView == null) return;

        schemeView.CaptureAsImage(texture =>
        {
            if (texture == null)
            {
                new Error("Failed to capture lineup image", "LineupTab");
                return;
            }

            string fileName = "lineup_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
            string filePath = Path.Combine(Application.temporaryCachePath, fileName);

            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, pngData);

            SaveLineupImageToProjectFiles(fileName, pngData);

            Destroy(texture);

            new NativeShare()
                .AddFile(filePath, "image/png")
                .SetSubject("Lineup")
                .SetText("Match Lineup")
                .Share();
        });
    }

    private void SaveLineupImageToProjectFiles(string fileName, byte[] pngData)
    {
        var persistentDir = Path.Combine(Application.persistentDataPath, "lineups");
        Directory.CreateDirectory(persistentDir);
        File.WriteAllBytes(Path.Combine(persistentDir, fileName), pngData);

#if UNITY_EDITOR
        var projectDir = Path.Combine(Application.dataPath, "Lineups");
        Directory.CreateDirectory(projectDir);
        File.WriteAllBytes(Path.Combine(projectDir, fileName), pngData);
#endif
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        var lineup = MatchCenter.CurrentLineup.Value;
        if (lineup != null)
        {
            SyncLineupToSquads(lineup);
            UpdateSquadPanels();
        }
        UpdateEmptyState();
    }

    private void UpdateEmptyState()
    {
        bool hasLineup = MatchCenter.CurrentLineup.Value != null;

        if (schemeView != null)
        {
            schemeView.gameObject.SetActive(hasLineup);
        }
        if (greenSquad != null)
        {
            greenSquad.gameObject.SetActive(hasLineup);
        }
        if (redSquad != null)
        {
            redSquad.gameObject.SetActive(hasLineup);
        }
        if (createLineupButton != null)
        {
            createLineupButton.gameObject.SetActive(!hasLineup);
        }
        if (lineupEmptyState != null)
        {
            lineupEmptyState.SetActive(!hasLineup);
        }
    }

    private void SyncLineupToSquads(LineupModel lineup)
    {
        Lineup.SquadGreen.Clear();
        Lineup.SquadRed.Clear();

        int greenIndex = 1;
        foreach (var playerId in lineup.playersBlue)
        {
            var player = Lineup.GetPlayerById(playerId);
            if (player == null) continue;

            var squadPlayer = new SquadPlayerModel
            {
                playerId = player.id,
                name = player.name,
                position = player.position,
                squadNumber = greenIndex++,
                isCaptain = lineup.captainBlue.HasValue && lineup.captainBlue.Value == player.id,
                teamSide = TeamSide.Green,
                teamIcon = Lineup.GetTeamIconForSide(TeamSide.Green)
            };
            Lineup.SquadGreen.Add(squadPlayer);
        }

        int redIndex = 1;
        foreach (var playerId in lineup.playersRed)
        {
            var player = Lineup.GetPlayerById(playerId);
            if (player == null) continue;

            var squadPlayer = new SquadPlayerModel
            {
                playerId = player.id,
                name = player.name,
                position = player.position,
                squadNumber = redIndex++,
                isCaptain = lineup.captainRed.HasValue && lineup.captainRed.Value == player.id,
                teamSide = TeamSide.Red,
                teamIcon = Lineup.GetTeamIconForSide(TeamSide.Red)
            };
            Lineup.SquadRed.Add(squadPlayer);
        }
    }

    private void UpdateSquadPanels()
    {
        if (greenSquad != null)
        {
            var panelModel = Lineup.BuildSquadPanelModel(TeamSide.Green);
            greenSquad.Init(panelModel);
            
            if (greenSquad.playersList != null)
            {
                greenSquad.playersList.Init(Lineup.SquadBlueAsObject);
            }
        }

        if (redSquad != null)
        {
            var panelModel = Lineup.BuildSquadPanelModel(TeamSide.Red);
            redSquad.Init(panelModel);
            
            if (redSquad.playersList != null)
            {
                redSquad.playersList.Init(Lineup.SquadRedAsObject);
            }
        }
    }
}
