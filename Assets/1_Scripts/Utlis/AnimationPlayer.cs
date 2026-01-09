using UnityEngine;
using DG.Tweening;
using System.Linq;
using Cysharp.Threading.Tasks;
using System;

public static class AnimationPlayer
{
    public static async UniTask PlayAnimationsAsync(GameObject target, bool show = true)
    {
        if (target == null) return;

        var animations = target.GetComponentsInChildren<IAnimationComponent>(true)
            .OrderBy(a => a.Order)
            .GroupBy(a => a.Order)
            .ToList();

        if (animations.Count == 0) return;

        var controller = new DOTweenAnimationController();
        controller.StartAnimation();
        var sequence = controller.GetSequence();

        foreach (var group in animations)
        {
            bool isFirstInGroup = true;
            foreach (var anim in group)
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
                catch (Exception ex)
                {
                    Debug.LogError($"Animation error in {anim.GetType().Name} on {target.name}: {ex.Message}");
                }
            }
        }

        await sequence.AsyncWaitForCompletion();
        controller.StopAnimation();
    }


    

}