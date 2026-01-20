using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeToDelete : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform content;
    [SerializeField] private float revealWidth = 140f;
    [SerializeField] private float revealThreshold = 60f;
    [SerializeField] private float dismissThreshold = 220f;
    [SerializeField] private float animationSpeed = 12f;

    private Vector2 _startPointerPos;
    private Vector2 _startContentPos;
    private bool _isOpen;
    private CancellationTokenSource _animCts;

    public event Action OnDelete;
    public event Action OnOpened;
    public event Action OnClosed;

    private void Awake()
    {
        if (content == null)
        {
            content = GetComponent<RectTransform>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (content == null) return;
        _startPointerPos = eventData.position;
        _startContentPos = content.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (content == null) return;
        float deltaX = eventData.position.x - _startPointerPos.x;
        float targetX = _startContentPos.x + deltaX;

        targetX = Mathf.Clamp(targetX, -dismissThreshold, 0f);
        content.anchoredPosition = new Vector2(targetX, content.anchoredPosition.y);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (content == null) return;
        float x = content.anchoredPosition.x;

        if (Mathf.Abs(x) >= dismissThreshold)
        {
            TriggerDelete();
            return;
        }

        if (Mathf.Abs(x) >= revealThreshold)
        {
            Open();
        }
        else
        {
            Close();
        }
    }

    public void Open()
    {
        if (content == null) return;
        if (_isOpen) return;
        _isOpen = true;
        StartAnimation(new Vector2(-revealWidth, content.anchoredPosition.y));
        OnOpened?.Invoke();
    }

    public void Close()
    {
        if (content == null) return;
        if (!_isOpen) return;
        _isOpen = false;
        StartAnimation(new Vector2(0f, content.anchoredPosition.y));
        OnClosed?.Invoke();
    }

    private void StartAnimation(Vector2 target)
    {
        _animCts?.Cancel();
        _animCts?.Dispose();
        _animCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        AnimateTo(target, _animCts.Token).Forget();
    }

    private async UniTask AnimateTo(Vector2 target, CancellationToken ct)
    {
        while (Vector2.Distance(content.anchoredPosition, target) > 0.5f)
        {
            ct.ThrowIfCancellationRequested();
            content.anchoredPosition = Vector2.Lerp(
                content.anchoredPosition, target, Time.deltaTime * animationSpeed);
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
        content.anchoredPosition = target;
    }

    private void TriggerDelete()
    {
        OnDelete?.Invoke();
    }
}