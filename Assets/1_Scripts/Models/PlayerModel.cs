using System;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class PlayerModel
{
    public int id;
    public string name;
    public PlayerPosition position;
    [NonSerialized]
    private Sprite _avatarCache;
    
    public Sprite avatar
    {
        get
        {
            if (_avatarCache == null && !string.IsNullOrEmpty(avatarPath))
            {
                _avatarCache = FileUtils.LoadImageAsSprite(avatarPath);
            }
            return _avatarCache;
        }
        set
        {
            _avatarCache = value;
        }
    }
    
    public string avatarPath;

    public PlayerModel() { }

    public PlayerModel(int id, string name, PlayerPosition position)
    {
        this.id = id;
        this.name = name;
        this.position = position;
    }
}

