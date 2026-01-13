using UnityEngine;

public class Log 
{
    public Log(string message, string tag = "DEBUG") 
    {
        Debug.Log($"[{tag}] {message}");
    }
}


public class Error
{
    public Error(string message, string tag = "DEBUG")
    {
        string coloredTag = $"<color=#{ColorUtility.ToHtmlStringRGB(Color.red)}>[{tag}]</color>";
        Debug.Log($"[{coloredTag}] {message}");
    }
}
