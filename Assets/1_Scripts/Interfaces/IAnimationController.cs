
public interface IAnimationController
{
    void StartAnimation();
    void StopAnimation();
    bool HasActiveAnimation { get; }
}