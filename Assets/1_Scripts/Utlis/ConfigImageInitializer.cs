using System.Collections.Generic;
using UnityEngine;

public static class ConfigImageInitializer
{
    private static Dictionary<string, string> _teamIconPaths = new Dictionary<string, string>();

    public static void InitializeTeamIcons(AppConfig config)
    {
        if (config == null || config.teams == null) return;

        foreach (var teamConfig in config.teams)
        {
            if (teamConfig.icon != null)
            {
                string key = teamConfig.side.ToString();
                if (!_teamIconPaths.ContainsKey(key))
                {
                    string fileName = $"team_{key.ToLower()}_icon.png";
                    if (!FileUtils.FileExists(fileName))
                    {
                        FileUtils.SaveImage(teamConfig.icon, fileName);
                    }
                    _teamIconPaths[key] = fileName;
                }
            }
        }
    }

    public static Sprite GetTeamIcon(TeamSide side)
    {
        string key = side.ToString();
        if (_teamIconPaths.TryGetValue(key, out string path))
        {
            return FileUtils.LoadImageAsSprite(path);
        }
        return null;
    }

    public static string GetTeamIconPath(TeamSide side)
    {
        string key = side.ToString();
        return _teamIconPaths.TryGetValue(key, out string path) ? path : null;
    }
}

