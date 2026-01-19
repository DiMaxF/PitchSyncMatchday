using UnityEngine;

[CreateAssetMenu(fileName = "TeamConfig", menuName = "Scriptable Objects/TeamConfig")]
public class TeamConfig : ScriptableObject
{
    public TeamSide side;
    public Sprite icon;
}
