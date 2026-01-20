using System;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using Unity.Notifications.Android;
#elif UNITY_IOS && !UNITY_EDITOR
using Unity.Notifications.iOS;
#endif

public class NotificationPermissionManager : MonoBehaviour
{
    public static NotificationPermissionManager Instance { get; private set; }

    public event Action<bool> OnPermissionResult;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        RequestPermission();
    }

    public void RequestPermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        RequestAndroidPermission();
#elif UNITY_IOS && !UNITY_EDITOR
        RequestIOSPermission();
#else
        OnPermissionResult?.Invoke(true);
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void RequestAndroidPermission()
    {
        AndroidNotificationCenter.RegisterNotificationChannel(new AndroidNotificationChannel
        {
            Id = "default_channel",
            Name = "Default Channel",
            Description = "Default notifications channel",
            Importance = Importance.Default
        });

        if (UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            OnPermissionResult?.Invoke(true);
        }
        else
        {
            UnityEngine.Android.Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
            StartCoroutine(CheckAndroidPermissionCoroutine());
        }
    }

    private System.Collections.IEnumerator CheckAndroidPermissionCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        bool granted = UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS");
        OnPermissionResult?.Invoke(granted);
    }
#endif

#if UNITY_IOS && !UNITY_EDITOR
    private void RequestIOSPermission()
    {
        StartCoroutine(RequestIOSPermissionCoroutine());
    }

    private System.Collections.IEnumerator RequestIOSPermissionCoroutine()
    {
        using (var request = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound, true))
        {
            while (!request.IsFinished)
            {
                yield return null;
            }

            bool granted = request.Granted;
            OnPermissionResult?.Invoke(granted);
        }
    }
#endif

    public bool HasPermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS");
#elif UNITY_IOS && !UNITY_EDITOR
        return iOSNotificationCenter.GetNotificationSettings().AuthorizationStatus == AuthorizationStatus.Authorized;
#else
        return true;
#endif
    }
}

