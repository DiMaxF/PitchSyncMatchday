using System;
using System.IO;
using UnityEngine;

public static class FileUtils
{
    private static string GetDataPath()
    {
        return Application.persistentDataPath;
    }

    public static string GetFilePath(string fileName)
    {
        return Path.Combine(GetDataPath(), fileName);
    }

    public static bool FileExists(string fileName)
    {
        return File.Exists(GetFilePath(fileName));
    }

    public static void SaveJson<T>(T data, string fileName)
    {
        try
        {
            string filePath = GetFilePath(fileName);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(filePath, json);
            new Log($"{filePath} saved", "FileUtils");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save JSON file {fileName}: {ex.Message}");
        }
    }

    public static T LoadJson<T>(string fileName) where T : new()
    {
        try
        {
            string filePath = GetFilePath(fileName);
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonUtility.FromJson<T>(json);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load JSON file {fileName}: {ex.Message}");
        }
        return new T();
    }

    public static void SaveImage(Sprite sprite, string fileName)
    {
        if (sprite == null)
        {
            Debug.LogError("Cannot save null sprite");
            return;
        }

        try
        {
            Texture2D texture = sprite.texture;
            Rect rect = sprite.textureRect;
            Texture2D croppedTexture = new Texture2D((int)rect.width, (int)rect.height);

            Color[] pixels = texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();

            byte[] bytes = croppedTexture.EncodeToPNG();
            string filePath = GetFilePath(fileName);
            File.WriteAllBytes(filePath, bytes);

            UnityEngine.Object.Destroy(croppedTexture);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save image {fileName}: {ex.Message}");
        }
    }

    public static void SaveImage(Texture2D texture, string fileName)
    {
        if (texture == null)
        {
            Debug.LogError("Cannot save null texture");
            return;
        }

        try
        {
            byte[] bytes = texture.EncodeToPNG();
            string filePath = GetFilePath(fileName);
            File.WriteAllBytes(filePath, bytes);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save image {fileName}: {ex.Message}");
        }
    }

    public static Sprite LoadImageAsSprite(string fileName)
    {
        try
        {
            string filePath = GetFilePath(fileName);
            if (File.Exists(filePath))
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(bytes);

                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                return sprite;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load image {fileName}: {ex.Message}");
        }
        return null;
    }

    public static Texture2D LoadImageAsTexture(string fileName)
    {
        try
        {
            string filePath = GetFilePath(fileName);
            if (File.Exists(filePath))
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(bytes);
                return texture;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load image {fileName}: {ex.Message}");
        }
        return null;
    }

    public static void DeleteFile(string fileName)
    {
        try
        {
            string filePath = GetFilePath(fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to delete file {fileName}: {ex.Message}");
        }
    }
}



