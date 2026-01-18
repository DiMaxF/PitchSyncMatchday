using UnityEngine;
using DG.Tweening;
using System.Linq;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public static class AnimationPlayer
{
    public static async UniTask PlayAnimationsAsync(GameObject target, bool show = true)
    {
        if (target == null) return;

        var allAnimations = target.GetComponentsInChildren<IAnimationComponent>(true);
        if (allAnimations.Length == 0) return;

        var filteredAnimations = allAnimations
     .Where(anim =>
     {
         var go = (anim as Component)?.gameObject;
         return go != null && (!HasUIViewComponent(go) || go == target);
     })
     .ToList();

        if (filteredAnimations.Count == 0) return;

        var animationsByGameObject = filteredAnimations
            .GroupBy(a => (a as Component)?.gameObject)
            .Where(g => g.Key != null) // �� ������ ������ ����������� null
            .ToList();

        var tasks = new List<UniTask>();

        foreach (var group in animationsByGameObject)
        {
            var gameObject = group.Key;
            var animations = group
                .OrderBy(a => a.Order)
                .GroupBy(a => a.Order)
                .ToList();

            if (animations.Count == 0) continue;

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
                    catch (Exception ex)
                    {
                        Debug.LogError($"Animation error in {anim.GetType().Name} on {gameObject.name}: {ex.Message}");
                    }
                }
            }

            tasks.Add(sequence.AsyncWaitForCompletion().AsUniTask());
        }

        if (tasks.Count > 0)
        {
            await UniTask.WhenAll(tasks);
        }
    }

    private static bool HasUIViewComponent(GameObject obj)
    {
        if (obj == null) return false;

        var behaviours = obj.GetComponents<MonoBehaviour>();
        foreach (var mb in behaviours)
        {
            var type = mb.GetType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(UIView<>))
            {
                return true;
            }
        }

        return false;
    }
}
