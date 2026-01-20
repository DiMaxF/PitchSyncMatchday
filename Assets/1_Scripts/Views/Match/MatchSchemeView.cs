using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class MatchSchemeView : UIView<LineupModel>
{
    [SerializeField] private RectTransform pitchRect;
    [SerializeField] private Transform playersContainer;
    [SerializeField] private PlayerIcon playerPrefab;

    [SerializeField] private float minIconSize = 35f;
    [SerializeField] private float maxIconSize = 85f;

    [SerializeField] private float sideMargin = 0.15f;
    [SerializeField] private float gkY = 0.07f;
    [SerializeField] private float dfY = 0.20f;
    [SerializeField] private float mfY = 0.34f;
    [SerializeField] private float fwY = 0.47f;

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

        var xs = GetLineXs(line.Count);
        float arcStrength = 0.05f; // Сила изгиба линии (чем больше, тем сильнее дуга)

        for (int i = 0; i < line.Count; i++)
        {
            var model = line[i];
            float xNorm = xs[i];
            
            // Расчет смещения по Y (эффект дуги)
            // Центральные игроки чуть "впереди", крайние чуть "сзади"
            float yOffset = 0f;
            if (line.Count >= 3)
            {
                float distFromCenter = Mathf.Abs(xNorm - 0.5f) * 2f; // 0 (центр) -> 1 (край)
                // yOffset отрицательный (назад к своим воротам)
                yOffset = -(distFromCenter * distFromCenter) * arcStrength;
            }

            float finalYNorm = yNorm + yOffset;
            float y = isTopTeam ? 1f - finalYNorm : finalYNorm;

            var icon = SpawnIcon(model);
            var anchored = NormalizedToAnchored(xNorm, y);
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

        // Адаптивная ширина: если игроков мало, они стоят ближе к центру
        // Если много - ближе к краям
        // 2 игрока -> отступы большие
        // 4+ игрока -> отступы минимальные (sideMargin)
        float tWidth = Mathf.Clamp01((count - 2) / 3f); 
        float currentMargin = Mathf.Lerp(0.25f, sideMargin, tWidth);

        float left = currentMargin;
        float right = 1f - currentMargin;
        
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
