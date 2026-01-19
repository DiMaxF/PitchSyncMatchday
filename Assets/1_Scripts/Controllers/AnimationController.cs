using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public async UniTask PlayAsync(bool show)
    {
        var allAnimations = GetComponents<IAnimationComponent>();

        if (!allAnimations.Any()) return;

        var animations = allAnimations
            .OrderBy(a => a.Order)
            .GroupBy(a => a.Order)
            .ToList();

        if (!animations.Any()) return;

        var sequence = DOTween.Sequence();

        foreach (var orderGroup in animations)
        {
            bool isFirstInGroup = true;
            foreach (var anim in orderGroup)
            {
                try
                {
                    var tween = show ? anim.AnimateShow() : anim.AnimateHide();

                    if (isFirstInGroup || !anim.IsParallel)
                    {
                        sequence.Append(tween);
                        isFirstInGroup = false;
                    }
                    else
                    {
                        sequence.Join(tween);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Animation error in {anim.GetType().Name} on {gameObject.name}: {ex.Message}");
                }
            }
        }

        await sequence.Play().AsyncWaitForCompletion().AsUniTask();
    }
}