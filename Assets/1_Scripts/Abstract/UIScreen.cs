using Cysharp.Threading.Tasks;
using System;
using UniRx;
using UnityEngine;

public abstract class UIScreen : MonoBehaviour
{
    public Screens ScreenType;

    protected ScreensManager ScreenManager;

    private CompositeDisposable _disposables = new CompositeDisposable();
    private AnimationController _animController;

    private void Awake()
    {
        _animController = GetComponent<AnimationController>();
    }

    public void Init(ScreensManager screenManager)
    {
        ScreenManager = screenManager;
    }

    protected virtual void OnEnable()
    {
        SubscribeToData();
    }

    protected virtual void OnDisable()
    {
        _disposables.Clear();
    }

    protected virtual void OnDestroy()
    {
        _disposables.Dispose();
    }

    protected virtual void SubscribeToData() { }

    protected IDisposable AddToDispose(IDisposable disposable)
    {
        _disposables.Add(disposable);
        return disposable;
    }

    public virtual async UniTask ShowAsync()
    {
        gameObject.SetActive(true);

        if (_animController != null)
            await _animController.PlayAsync(true);

        RefreshViews();
    }

    public virtual async UniTask HideAsync()
    {
        if (_animController != null)
            await _animController.PlayAsync(false);

        gameObject.SetActive(false);
    }

    public virtual void Show() => ShowAsync().Forget();
    public virtual void Hide() => HideAsync().Forget();

    protected virtual void RefreshViews()
    {
    }
}