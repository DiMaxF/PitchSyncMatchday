using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private const bool ExcludeChildViews = true;

    public async UniTask PlayAsync(bool show)
    {
        var allAnimations = GetComponentsInChildren<IAnimationComponent>(true);

        if (!allAnimations.Any()) return;

        var filteredAnimations = allAnimations
            .Where(anim =>
            {
                var go = (anim as Component)?.gameObject;
                return go != null && (!ExcludeChildViews || !HasUIViewComponent(go) || go == gameObject);
            })
            .ToList();

        if (!filteredAnimations.Any()) return;

        var animationsByGameObject = filteredAnimations
            .GroupBy(a => (a as Component)?.gameObject)
            .Where(g => g.Key != null)
            .ToList();

        var tasks = new List<UniTask>();

        foreach (var group in animationsByGameObject)
        {
            var animations = group
                .OrderBy(a => a.Order)
                .GroupBy(a => a.Order)
                .ToList();

            if (!animations.Any()) continue;

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
                        Debug.LogError($"Animation error in {anim.GetType().Name} on {group.Key.name}: {ex.Message}");
                    }
                }
            }

            tasks.Add(sequence.Play().AsyncWaitForCompletion().AsUniTask());
        }

        if (tasks.Any())
        {
            await UniTask.WhenAll(tasks);
        }
    }

    private static bool HasUIViewComponent(GameObject obj)
    {
        if (obj == null) return false;

        var behaviours = obj.GetComponents<MonoBehaviour>();
        return behaviours.Any(mb =>
        {
            var type = mb.GetType();
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(UIView<>);
        });
    }
}