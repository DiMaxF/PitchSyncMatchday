using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationConfig", menuName = "ScriptableObjects/AnimationConfig")]
public class AnimationConfig : ScriptableObject
{
    public float Duration = 0.25f;
    public float Delay = 0.1f;
    public Ease Ease = Ease.OutQuad;
    
}