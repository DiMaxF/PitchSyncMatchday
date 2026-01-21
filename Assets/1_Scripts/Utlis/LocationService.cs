using System;
using System.Collections;
using UnityEngine;

public class LocationService : MonoBehaviour
{
    public static LocationService Instance { get; private set; }

    private const float UPDATE_INTERVAL = 30f;
    private const float DESIRED_ACCURACY = 10f;
    private const float UPDATE_DISTANCE = 10f;

    private Coroutine _updateCoroutine;
    private bool _isUpdating = false;

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
        StartCoroutine(InitializeAfterDataManager());
    }

    private IEnumerator InitializeAfterDataManager()
    {
        while (DataManager.Instance == null || DataManager.Profile == null)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        if (DataManager.Profile.LocationPermissionGranted.Value)
        {
            StartLocationUpdates();
        }
        else
        {
            RequestLocationPermission();
        }
    }

    public void Initialize()
    {
        if (DataManager.Instance != null && DataManager.Profile != null)
        {
            if (DataManager.Profile.LocationPermissionGranted.Value)
            {
                StartLocationUpdates();
            }
            else
            {
                RequestLocationPermission();
            }
        }
    }

    public void RequestLocationPermission()
    {
        StartCoroutine(RequestLocationPermissionCoroutine());
    }

    private IEnumerator RequestLocationPermissionCoroutine()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
            yield return new WaitForSeconds(0.5f);
            
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
            {
                new Log("Location permission denied by user", "LocationService");
                if (DataManager.Instance != null && DataManager.Profile != null)
                {
                    DataManager.Profile.LocationPermissionGranted.Value = false;
                }
                yield break;
            }
        }
#endif

        if (!Input.location.isEnabledByUser)
        {
            new Log("Location service is not enabled by user", "LocationService");
            if (DataManager.Instance != null && DataManager.Profile != null)
            {
                DataManager.Profile.LocationPermissionGranted.Value = false;
            }
            yield break;
        }

        Input.location.Start(DESIRED_ACCURACY, UPDATE_DISTANCE);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            new Log("Location service initialization timeout", "LocationService");
            if (DataManager.Instance != null && DataManager.Profile != null)
            {
                DataManager.Profile.LocationPermissionGranted.Value = false;
            }
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            new Error("Unable to determine device location", "LocationService");
            if (DataManager.Instance != null && DataManager.Profile != null)
            {
                DataManager.Profile.LocationPermissionGranted.Value = false;
            }
            yield break;
        }

        if (DataManager.Instance != null && DataManager.Profile != null)
        {
            DataManager.Profile.LocationPermissionGranted.Value = true;
            UpdateLocation();
            StartLocationUpdates();
        }
    }

    private void StartLocationUpdates()
    {
        if (_isUpdating) return;

        _isUpdating = true;
        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
        }
        _updateCoroutine = StartCoroutine(UpdateLocationCoroutine());
    }

    private void StopLocationUpdates()
    {
        _isUpdating = false;
        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
            _updateCoroutine = null;
        }
        if (Input.location.isEnabledByUser)
        {
            Input.location.Stop();
        }
    }

    private IEnumerator UpdateLocationCoroutine()
    {
        while (_isUpdating)
        {
            if (Input.location.isEnabledByUser && Input.location.status == LocationServiceStatus.Running)
            {
                UpdateLocation();
            }
            yield return new WaitForSeconds(UPDATE_INTERVAL);
        }
    }

    private void UpdateLocation()
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            float latitude = Input.location.lastData.latitude;
            float longitude = Input.location.lastData.longitude;

            if (DataManager.Instance != null && DataManager.Profile != null)
            {
                DataManager.Profile.UserLatitude.Value = latitude;
                DataManager.Profile.UserLongitude.Value = longitude;
                DataManager.Instance.SaveAppModel();
            }
        }
    }

    public bool HasLocationPermission()
    {
        return Input.location.isEnabledByUser && 
               (Input.location.status == LocationServiceStatus.Running || 
                Input.location.status == LocationServiceStatus.Initializing);
    }

    public Vector2 GetCurrentLocation()
    {
        if (HasLocationPermission() && Input.location.status == LocationServiceStatus.Running)
        {
            return new Vector2(Input.location.lastData.latitude, Input.location.lastData.longitude);
        }
        
        if (DataManager.Instance != null && DataManager.Profile != null)
        {
            return new Vector2(DataManager.Profile.UserLatitude.Value, DataManager.Profile.UserLongitude.Value);
        }

        return Vector2.zero;
    }

    private void OnDestroy()
    {
        StopLocationUpdates();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            StopLocationUpdates();
        }
        else
        {
            if (DataManager.Instance != null && DataManager.Profile != null && 
                DataManager.Profile.LocationPermissionGranted.Value)
            {
                StartLocationUpdates();
            }
        }
    }

    public static float CalculateDistance(float lat1, float lon1, float lat2, float lon2)
    {
        const float R = 6371f;

        float dLat = (lat2 - lat1) * Mathf.Deg2Rad;
        float dLon = (lon2 - lon1) * Mathf.Deg2Rad;

        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                  Mathf.Cos(lat1 * Mathf.Deg2Rad) * Mathf.Cos(lat2 * Mathf.Deg2Rad) *
                  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);

        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        float distance = R * c;

        return distance;
    }

    public static float CalculateDistance(Vector2 location1, Vector2 location2)
    {
        return CalculateDistance(location1.x, location1.y, location2.x, location2.y);
    }
}

