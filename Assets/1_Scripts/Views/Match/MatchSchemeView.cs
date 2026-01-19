using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class MatchSchemeView : UIView<LineupModel>
{
    [SerializeField] private RectTransform pitchRect;
    [SerializeField] private Transform playersContainer;
    [SerializeField] private PlayerIcon playerPrefab;

    [SerializeField] private float minIconSize = 40f;
    [SerializeField] private float maxIconSize = 90f;

    [SerializeField] private float sideMargin = 0.08f;
    [SerializeField] private float gkY = 0.08f;
    [SerializeField] private float dfY = 0.28f;
    [SerializeField] private float mfY = 0.55f;
    [SerializeField] private float fwY = 0.80f;

    private readonly List<PlayerIcon> _icons = new List<PlayerIcon>();
    private LineupDataManager Lineup => DataManager.Lineup;

    protected override void Subscribe()
    {
        base.Subscribe();
        DataProperty
            .Where(m => m != null)
            .Subscribe(_ => UpdateScheme())
            .AddTo(this);
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        UpdateScheme();
    }

    private void UpdateScheme()
    {
        var lineup = DataProperty.Value;
        if (lineup == null || pitchRect == null || playersContainer == null || playerPrefab == null) return;

        ClearIcons();

        var blue = BuildSquadPlayers(lineup.playersBlue, TeamSide.Green, lineup.captainBlue);
        var red = BuildSquadPlayers(lineup.playersRed, TeamSide.Red, lineup.captainRed);

        int maxCount = Mathf.Max(1, Mathf.Max(blue.Count, red.Count));
        float iconSize = ComputeIconSize(maxCount);

        LayoutTeam(blue, false, iconSize);
        LayoutTeam(red, true, iconSize);
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

    private void LayoutTeam(List<SquadPlayerModel> team, bool isTopTeam, float iconSize)
    {
        if (team.Count == 0) return;

        var gk = team.Where(p => p.position == PlayerPosition.GK).ToList();
        var df = team.Where(p => p.position == PlayerPosition.DF).ToList();
        var mf = team.Where(p => p.position == PlayerPosition.MF).ToList();
        var fw = team.Where(p => p.position == PlayerPosition.FW).ToList();

        PlaceLine(gk, isTopTeam, gkY, iconSize);
        PlaceLine(df, isTopTeam, dfY, iconSize);
        PlaceLine(mf, isTopTeam, mfY, iconSize);
        PlaceLine(fw, isTopTeam, fwY, iconSize);
    }

    private void PlaceLine(List<SquadPlayerModel> line, bool isTopTeam, float yNorm, float iconSize)
    {
        if (line.Count == 0) return;

        float y = isTopTeam ? 1f - yNorm : yNorm;
        var xs = GetLineXs(line.Count);

        for (int i = 0; i < line.Count; i++)
        {
            var model = line[i];
            var icon = SpawnIcon(model);
            var anchored = NormalizedToAnchored(xs[i], y);
            icon.ApplyLayout(anchored, iconSize);
        }
    }

    private List<float> GetLineXs(int count)
    {
        var xs = new List<float>(count);
        if (count == 1)
        {
            xs.Add(0.5f);
            return xs;
        }

        float left = sideMargin;
        float right = 1f - sideMargin;
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / (count - 1);
            xs.Add(Mathf.Lerp(left, right, t));
        }
        return xs;
    }

    private Vector2 NormalizedToAnchored(float xNorm, float yNorm)
    {
        var rect = pitchRect.rect;
        float x = (xNorm - 0.5f) * rect.width;
        float y = (yNorm - 0.5f) * rect.height;
        return new Vector2(x, y);
    }

    private float ComputeIconSize(int maxPlayersOneTeam)
    {
        float t = Mathf.InverseLerp(1f, 11f, maxPlayersOneTeam);
        return Mathf.Lerp(maxIconSize, minIconSize, t);
    }

    private PlayerIcon SpawnIcon(SquadPlayerModel model)
    {
        var icon = Instantiate(playerPrefab, playersContainer);
        icon.Init(model);
        _icons.Add(icon);
        return icon;
    }

    private void ClearIcons()
    {
        foreach (var icon in _icons)
        {
            if (icon != null) Destroy(icon.gameObject);
        }
        _icons.Clear();
    }
}
