using System;
using UnityEngine;

[Serializable]
public class PlayerModel
{
    public int id;
    public string name;
    public PlayerPosition position;
    public Sprite avatar;

    public PlayerModel() { }

    public PlayerModel(int id, string name, PlayerPosition position)
    {
        this.id = id;
        this.name = name;
        this.position = position;
    }

    private static int GenerateId() => (int)(DateTime.UtcNow.Ticks & 0xFFFFFFFF);
}

