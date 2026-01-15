using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static readonly Dictionary<UIView, Subject<object>> _viewSubjects = new Dictionary<UIView, Subject<object>>();
    private static readonly List<UIView> _persistentViews = new List<UIView>();
    private static readonly CompositeDisposable _globalDisposables = new CompositeDisposable();

    public static void RegisterView<TView>(TView view, bool persistent = false) where TView : UIView
    {
        if (!_viewSubjects.ContainsKey(view))
        {
            _viewSubjects[view] = new Subject<object>();
        }

        if (persistent && !_persistentViews.Contains(view))
        {
            _persistentViews.Add(view);
        }
    }

    public static IDisposable SubscribeToView<TView, TData>(TView view, Action<TData> handler, bool persistent = false)
        where TView : UIView
    {
        if (!_viewSubjects.TryGetValue(view, out var subject))
        {
            subject = new Subject<object>();
            _viewSubjects[view] = subject;
        }

        var subscription = subject
            .OfType<object, TData>()
            .Subscribe(handler);

        if (persistent)
        {
            _globalDisposables.Add(subscription);
        }

        return subscription;
    }

    public static void TriggerAction<T>(UIView view, T data)
    {
        if (_viewSubjects.TryGetValue(view, out var subject))
        {
            subject.OnNext(data);
        }
    }

    public static IDisposable ForwardEventsFrom(UIView sourceView, UIView targetView, bool persistent = false)
    {
        if (!_viewSubjects.TryGetValue(sourceView, out var sourceSubject))
        {
            sourceSubject = new Subject<object>();
            _viewSubjects[sourceView] = sourceSubject;
        }

        var subscription = sourceSubject
            .Subscribe(data =>
            {
                if (_viewSubjects.TryGetValue(targetView, out var targetSubject))
                {
                    targetSubject.OnNext(data);
                }
            });

        if (persistent)
        {
            _globalDisposables.Add(subscription);
        }

        return subscription;
    }

    public static TView GetView<TView>() where TView : UIView
    {
        return _persistentViews.OfType<TView>().FirstOrDefault() ??
               FindObjectsOfType<TView>(true).FirstOrDefault(v => _viewSubjects.ContainsKey(v));
    }

    public static void UnregisterView(UIView view)
    {
        if (_viewSubjects.TryGetValue(view, out var subject))
        {
            subject.OnCompleted();
            subject.Dispose();
            _viewSubjects.Remove(view);
        }
        _persistentViews.Remove(view);
    }

    public static void ClearNonPersistent()
    {
        var viewsToRemove = new List<UIView>();
        foreach (var kvp in _viewSubjects.Keys)
        {
            if (!_persistentViews.Contains(kvp))
            {
                viewsToRemove.Add(kvp);
            }
        }

        foreach (var view in viewsToRemove)
        {
            UnregisterView(view);
        }
    }
}