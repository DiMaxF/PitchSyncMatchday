using Cysharp.Threading.Tasks;
using System;
using UniRx;
using UnityEngine;

public abstract class UIView : MonoBehaviour
{
    public bool IsActive => gameObject.activeSelf;

    private CompositeDisposable _disposables = new CompositeDisposable();
    private bool _isSubscribed = false;
    private AnimationController _animController;

    protected virtual void Awake()
    {
        _animController = GetComponent<AnimationController>();

        if (!_isSubscribed)
        {
            _isSubscribed = true;
            Subscribe();
        }
    }

    protected virtual void OnEnable()
    {
        UIManager.RegisterView(this);

        if (!_isSubscribed)
        {
            _isSubscribed = true;
            Subscribe();
        }
    }

    protected virtual void OnDisable()
    {
    }

    protected virtual void OnDestroy()
    {
        UIManager.UnregisterView(this);
        _disposables.Clear();
        _disposables.Dispose();
        _isSubscribed = false;
    }

    public virtual void Init()
    {
        UpdateUI();
    }

    public virtual void UpdateUI() { }

    protected virtual void Subscribe() { }

    protected IDisposable AddToDispose(IDisposable disposable)
    {
        _disposables.Add(disposable);
        return disposable;
    }

    public virtual async UniTask ShowAsync()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (_animController != null)
            await _animController.PlayAsync(true);
    }

    public virtual async UniTask HideAsync()
    {
        if (_animController != null)
            await _animController.PlayAsync(false);
    }

    public virtual void Show() => ShowAsync().Forget();
    public virtual void Hide() => HideAsync().Forget();
}