using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class LineupDataManager : IDataManager
{
    private readonly AppModel _appModel;
    private readonly AppConfig _appConfig;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    private readonly List<PlayerModel> _allPlayers = new List<PlayerModel>();
    private bool _suppressAvailablePlayersUpdate;

    public ReactiveProperty<LineupMod> SelectedDraftMode { get; } = new ReactiveProperty<LineupMod>(LineupMod.Alternate);
    public ReactiveCollection<ToggleButtonModel> DraftModes { get; } = new ReactiveCollection<ToggleButtonModel>();
    private readonly ReactiveCollection<object> _draftModesAsObject = new ReactiveCollection<object>();
    public ReactiveCollection<object> DraftModesAsObject => _draftModesAsObject;

    public ReactiveCollection<ToggleButtonModel> PositionFilters { get; } = new ReactiveCollection<ToggleButtonModel>();
    private readonly ReactiveCollection<object> _positionFiltersAsObject = new ReactiveCollection<object>();
    public ReactiveCollection<object> PositionFiltersAsObject => _positionFiltersAsObject;

    public ReactiveCollection<PlayerModel> PlayersPool { get; } = new ReactiveCollection<PlayerModel>();
    private readonly ReactiveCollection<object> _playersPoolAsObject = new ReactiveCollection<object>();
    public ReactiveCollection<object> PlayersPoolAsObject => _playersPoolAsObject;

    public ReactiveCollection<SquadPlayerModel> SquadGreen { get; } = new ReactiveCollection<SquadPlayerModel>();
    public ReactiveCollection<SquadPlayerModel> SquadRed { get; } = new ReactiveCollection<SquadPlayerModel>();
    private readonly ReactiveCollection<object> _squadBlueAsObject = new ReactiveCollection<object>();
    private readonly ReactiveCollection<object> _squadRedAsObject = new ReactiveCollection<object>();
    public ReactiveCollection<object> SquadBlueAsObject => _squadBlueAsObject;
    public ReactiveCollection<object> SquadRedAsObject => _squadRedAsObject;

    public ReactiveProperty<LineupModel> CurrentLineup { get; } = new ReactiveProperty<LineupModel>(null);

    public ReactiveProperty<string> SearchQuery { get; } = new ReactiveProperty<string>(string.Empty);
    public ReactiveCollection<PlayerModel> AvailablePlayers { get; } = new ReactiveCollection<PlayerModel>();
    private readonly ReactiveCollection<object> _availablePlayersAsObject = new ReactiveCollection<object>();
    public ReactiveCollection<object> AvailablePlayersAsObject => _availablePlayersAsObject;

    public LineupDataManager(AppModel appModel, AppConfig appConfig)
    {
        _appModel = appModel;
        _appConfig = appConfig;

        ConfigImageInitializer.InitializeTeamIcons(appConfig);
        InitializePlayers();
        InitializeDraftModes();
        BindCollections();
        SelectedDraftMode.Subscribe(_ => UpdateDraftModesSelection()).AddTo(_disposables);
        
        SearchQuery.Subscribe(_ => TryUpdateAvailablePlayers()).AddTo(_disposables);
        SquadGreen.ObserveAdd().Subscribe(_ => TryUpdateAvailablePlayers()).AddTo(_disposables);
        SquadGreen.ObserveRemove().Subscribe(_ => TryUpdateAvailablePlayers()).AddTo(_disposables);
        SquadRed.ObserveAdd().Subscribe(_ => TryUpdateAvailablePlayers()).AddTo(_disposables);
        SquadRed.ObserveRemove().Subscribe(_ => TryUpdateAvailablePlayers()).AddTo(_disposables);
        PlayersPool.ObserveAdd().Subscribe(_ => TryUpdateAvailablePlayers()).AddTo(_disposables);
        PlayersPool.ObserveRemove().Subscribe(_ => TryUpdateAvailablePlayers()).AddTo(_disposables);
        
        UpdateAvailablePlayers();
    }

    private void InitializePlayers()
    {
        foreach (var player in _appModel.players)
        {
            if (!string.IsNullOrEmpty(player.avatarPath))
            {
                player.avatar = FileUtils.LoadImageAsSprite(player.avatarPath);
            }
            
            _allPlayers.Add(player);
            PlayersPool.Add(player);
        }
    }

    public void AddPlayer(string name, PlayerPosition position, Sprite avatar, string avatarPath)
    {
        var newId = IdGenerator.GetNextId(_appModel, "Player");
        
        var player = new PlayerModel(newId, name, position);
        var fileName = $"player_{newId}_avatar.png";
        
        if (avatar != null)
        {
            player.avatar = avatar;
            FileUtils.SaveImage(avatar, fileName);
            player.avatarPath = fileName;
        }

        _allPlayers.Add(player);
        PlayersPool.Add(player);
        _appModel.players.Add(player);
        DataManager.Instance.SaveAppModel();
    }

    public void RemovePlayer(int playerId)
    {
        var player = _allPlayers.FirstOrDefault(p => p.id == playerId);
        if (player == null) return;

        _allPlayers.Remove(player);
        PlayersPool.Remove(player);
        _appModel.players.Remove(player);

        RemovePlayerFromTeam(playerId, TeamSide.Green);
        RemovePlayerFromTeam(playerId, TeamSide.Red);
        DataManager.Instance.SaveAppModel();
    }

    public void UpdatePlayer(int playerId, string name, PlayerPosition position, Sprite avatar, string avatarPath)
    {
        var player = _allPlayers.FirstOrDefault(p => p.id == playerId);
        if (player == null) return;

        player.name = name;
        player.position = position;
        
        if (avatar != null)
        {
            player.avatar = avatar;
            var fileName = $"player_{playerId}_avatar.png";
            FileUtils.SaveImage(avatar, fileName);
            player.avatarPath = fileName;
        }

        var poolIndex = PlayersPool.IndexOf(player);
        if (poolIndex >= 0)
        {
            PlayersPool[poolIndex] = player;
        }

        var appIndex = _appModel.players.IndexOf(player);
        if (appIndex >= 0)
        {
            _appModel.players[appIndex] = player;
        }

        UpdateSquadPlayerInfo(playerId, name, position);
        DataManager.Instance.SaveAppModel();
    }

    private void UpdateSquadPlayerInfo(int playerId, string name, PlayerPosition position)
    {
        UpdateSquadPlayerInfoInSquad(SquadGreen, playerId, name, position);
        UpdateSquadPlayerInfoInSquad(SquadRed, playerId, name, position);
    }

    private void UpdateSquadPlayerInfoInSquad(ReactiveCollection<SquadPlayerModel> squad, int playerId, string name, PlayerPosition position)
    {
        for (int i = 0; i < squad.Count; i++)
        {
            var squadPlayer = squad[i];
            if (squadPlayer.playerId == playerId)
            {
                squad[i] = new SquadPlayerModel
                {
                    playerId = squadPlayer.playerId,
                    name = name,
                    position = position,
                    squadNumber = squadPlayer.squadNumber,
                    isCaptain = squadPlayer.isCaptain,
                    teamSide = squadPlayer.teamSide,
                    teamIcon = squadPlayer.teamIcon
                };
            }
        }
    }

    private void InitializeDraftModes()
    {
        foreach (LineupMod mode in Enum.GetValues(typeof(LineupMod)))
        {
            string displayName = mode == LineupMod.Alternate ? "Alternate 1:1" : "Snake 1:2";
            DraftModes.Add(new ToggleButtonModel
            {
                name = displayName,
                selected = mode == SelectedDraftMode.Value
            });
        }
    }

    private void UpdateDraftModesSelection()
    {
        var modes = Enum.GetValues(typeof(LineupMod)).Cast<LineupMod>().ToList();
        for (int i = 0; i < DraftModes.Count && i < modes.Count; i++)
        {
            var mode = modes[i];
            var model = DraftModes[i];
            var newSelected = SelectedDraftMode.Value == mode;

            if (model.selected != newSelected)
            {
                string displayName = mode == LineupMod.Alternate ? "Alternate 1:1" : "Snake 1:2";
                DraftModes[i] = new ToggleButtonModel
                {
                    name = displayName,
                    selected = newSelected
                };
            }
        }
    }

    private void BindCollections()
    {
        BindMirror(DraftModes, _draftModesAsObject);
        BindMirror(PlayersPool, _playersPoolAsObject);
        BindMirror(SquadGreen, _squadBlueAsObject);
        BindMirror(SquadRed, _squadRedAsObject);
        BindMirror(PositionFilters, _positionFiltersAsObject);
        BindMirror(AvailablePlayers, _availablePlayersAsObject);
    }

    private void BindMirror<T>(ReactiveCollection<T> source, ReactiveCollection<object> mirror)
    {
        mirror.Clear();
        foreach (var item in source)
        {
            mirror.Add(item);
        }

        source.ObserveAdd().Subscribe(e => mirror.Insert(e.Index, e.Value)).AddTo(_disposables);
        source.ObserveRemove().Subscribe(e => mirror.RemoveAt(e.Index)).AddTo(_disposables);
        source.ObserveReplace().Subscribe(e => mirror[e.Index] = e.NewValue).AddTo(_disposables);
        source.ObserveMove().Subscribe(e => mirror.Move(e.OldIndex, e.NewIndex)).AddTo(_disposables);
        source.ObserveReset().Subscribe(_ =>
        {
            mirror.Clear();
            foreach (var item in source) mirror.Add(item);
        }).AddTo(_disposables);
    }

    public void SelectDraftMode(LineupMod mode)
    {
        SelectedDraftMode.Value = mode;
    }

    public void SelectPlayerForTeam(int playerId, TeamSide teamSide)
    {
        var player = _allPlayers.FirstOrDefault(p => p.id == playerId);
        if (player == null) return;

        var targetSquad = teamSide == TeamSide.Green ? SquadGreen : SquadRed;
        var otherSquad = teamSide == TeamSide.Green ? SquadRed : SquadGreen;

        var existingInTarget = targetSquad.FirstOrDefault(sp => sp.playerId == playerId);
        if (existingInTarget != null)
        {
            RemovePlayerFromTeam(playerId, teamSide);
            return;
        }

        var existingInOther = otherSquad.FirstOrDefault(sp => sp.playerId == playerId);
        if (existingInOther != null)
        {
            otherSquad.Remove(existingInOther);
        }

        var squadNumber = targetSquad.Count + 1;
        var squadPlayer = new SquadPlayerModel
        {
            playerId = player.id,
            name = player.name,
            position = player.position,
            squadNumber = squadNumber,
            isCaptain = false,
            teamSide = teamSide
        };

        targetSquad.Add(squadPlayer);
    }

    public void RemovePlayerFromTeam(int playerId, TeamSide teamSide)
    {
        var squad = teamSide == TeamSide.Green ? SquadGreen : SquadRed;
        var player = squad.FirstOrDefault(sp => sp.playerId == playerId);
        if (player != null)
        {
            squad.Remove(player);
            RenumberSquad(teamSide);
        }
    }

    private void RenumberSquad(TeamSide teamSide)
    {
        var squad = teamSide == TeamSide.Green ? SquadGreen : SquadRed;
        for (int i = 0; i < squad.Count; i++)
        {
            var player = squad[i];
            squad[i] = new SquadPlayerModel
            {
                playerId = player.playerId,
                name = player.name,
                position = player.position,
                squadNumber = i + 1,
                isCaptain = player.isCaptain,
                teamSide = player.teamSide
            };
        }
    }

    public void ToggleCaptain(int playerId, TeamSide teamSide)
    {
        var squad = teamSide == TeamSide.Green ? SquadGreen : SquadRed;
        var player = squad.FirstOrDefault(sp => sp.playerId == playerId);
        if (player == null) return;

        if (player.isCaptain)
        {
            var index = squad.IndexOf(player);
            squad[index] = new SquadPlayerModel
            {
                playerId = player.playerId,
                name = player.name,
                position = player.position,
                squadNumber = player.squadNumber,
                isCaptain = false,
                teamSide = player.teamSide
            };
        }
        else
        {
            for (int i = 0; i < squad.Count; i++)
            {
                var p = squad[i];
                squad[i] = new SquadPlayerModel
                {
                    playerId = p.playerId,
                    name = p.name,
                    position = p.position,
                    squadNumber = p.squadNumber,
                    isCaptain = p.playerId == playerId,
                    teamSide = p.teamSide
                };
            }
        }
    }

    public void AutoBalance()
    {
        _suppressAvailablePlayersUpdate = true;
        var allSquadPlayers = new List<SquadPlayerModel>();
        allSquadPlayers.AddRange(SquadGreen);
        allSquadPlayers.AddRange(SquadRed);

        var players = allSquadPlayers
            .Select(sp => GetPlayerById(sp.playerId))
            .Where(p => p != null)
            .ToList();

        SquadGreen.Clear();
        SquadRed.Clear();

        var gks = players.Where(p => p.position == PlayerPosition.GK).ToList();
        var others = players.Where(p => p.position != PlayerPosition.GK).ToList();

        int blueIndex = 1;
        int redIndex = 1;

        if (gks.Count >= 2)
        {
            AddPlayerToSquad(gks[0], TeamSide.Green, blueIndex++);
            AddPlayerToSquad(gks[1], TeamSide.Red, redIndex++);
        }
        else if (gks.Count == 1)
        {
            AddPlayerToSquad(gks[0], TeamSide.Green, blueIndex++);
        }

        var shuffled = others.OrderBy(x => UnityEngine.Random.value).ToList();

        if (SelectedDraftMode.Value == LineupMod.Alternate)
        {
            bool isBlueTurn = true;
            foreach (var player in shuffled)
            {
                if (isBlueTurn)
                {
                    AddPlayerToSquad(player, TeamSide.Green, blueIndex++);
                }
                else
                {
                    AddPlayerToSquad(player, TeamSide.Red, redIndex++);
                }
                isBlueTurn = !isBlueTurn;
            }
        }
        else if (SelectedDraftMode.Value == LineupMod.Snake)
        {
            int playerIndex = 0;
            int round = 0;
            while (playerIndex < shuffled.Count)
            {
                bool isEvenRound = (round % 2 == 0);
                int bluePicks = isEvenRound ? 1 : 2;
                int redPicks = isEvenRound ? 2 : 1;

                for (int i = 0; i < bluePicks && playerIndex < shuffled.Count; i++)
                {
                    AddPlayerToSquad(shuffled[playerIndex], TeamSide.Green, blueIndex++);
                    playerIndex++;
                }

                for (int i = 0; i < redPicks && playerIndex < shuffled.Count; i++)
                {
                    AddPlayerToSquad(shuffled[playerIndex], TeamSide.Red, redIndex++);
                    playerIndex++;
                }

                round++;
            }
        }
        _suppressAvailablePlayersUpdate = false;
        UpdateAvailablePlayers();
    }

    private void AddPlayerToSquad(PlayerModel player, TeamSide teamSide, int squadNumber)
    {
        var squad = teamSide == TeamSide.Green ? SquadGreen : SquadRed;
        var squadPlayer = new SquadPlayerModel
        {
            playerId = player.id,
            name = player.name,
            position = player.position,
            squadNumber = squadNumber,
            isCaptain = false,
            teamSide = teamSide,
            teamIcon = GetTeamIcon(teamSide)
        };
        squad.Add(squadPlayer);
    }

    public ReactiveCollection<object> GetSquadPlayersAsObject(TeamSide teamSide)
    {
        return teamSide == TeamSide.Green ? SquadBlueAsObject : SquadRedAsObject;
    }

    public SquadPanelModel BuildSquadPanelModel(TeamSide teamSide)
    {
        var squad = teamSide == TeamSide.Green ? SquadGreen : SquadRed;
        var players = squad.ToList();
        
        return new SquadPanelModel
        {
            teamSide = teamSide,
            players = players,
            squadIcon = GetTeamIcon(teamSide),
            playerCount = players.Count
        };
    }

    private Sprite GetTeamIcon(TeamSide side) 
    {
        var icon = ConfigImageInitializer.GetTeamIcon(side);
        if (icon != null) return icon;
        return _appConfig.teams.Where(t => t.side == side).FirstOrDefault()?.icon;
    }

    public Sprite GetTeamIconForSide(TeamSide side)
    {
        return GetTeamIcon(side);
    }

    public PlayerModel GetPlayerById(int id)
    {
        return _allPlayers.FirstOrDefault(p => p.id == id);
    }

    public void SaveLineup()
    {
        if (CurrentLineup.Value == null)
        {
            CurrentLineup.Value = new LineupModel(null, _appModel);
        }

        var lineup = CurrentLineup.Value;
        lineup.playersBlue = SquadGreen.Select(sp => sp.playerId).ToList();
        lineup.playersRed = SquadRed.Select(sp => sp.playerId).ToList();
        lineup.captainBlue = SquadGreen.FirstOrDefault(sp => sp.isCaptain)?.playerId;
        lineup.captainRed = SquadRed.FirstOrDefault(sp => sp.isCaptain)?.playerId;

        if (!_appModel.lineups.Contains(lineup))
        {
            _appModel.lineups.Add(lineup);
        }

        DataManager.Instance.SaveAppModel();
    }

    private void TryUpdateAvailablePlayers()
    {
        if (_suppressAvailablePlayersUpdate) return;
        UpdateAvailablePlayers();
    }

    private void UpdateAvailablePlayers()
    {
        var addedPlayerIds = new HashSet<int>();
        foreach (var player in SquadGreen)
        {
            addedPlayerIds.Add(player.playerId);
        }
        foreach (var player in SquadRed)
        {
            addedPlayerIds.Add(player.playerId);
        }

        var available = PlayersPool.Where(p => !addedPlayerIds.Contains(p.id)).ToList();

        if (!string.IsNullOrEmpty(SearchQuery.Value))
        {
            var query = SearchQuery.Value.ToLower();
            available = available.Where(p => 
                p.name != null && p.name.ToLower().Contains(query)).ToList();
        }

        AvailablePlayers.Clear();
        foreach (var player in available)
        {
            AvailablePlayers.Add(player);
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}

