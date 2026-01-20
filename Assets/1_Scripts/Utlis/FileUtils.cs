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

    public static Sprite ProcessAvatarImage(Texture2D sourceTexture, int maxSize = 500)
    {
        if (sourceTexture == null) return null;

        int size = Mathf.Min(sourceTexture.width, sourceTexture.height);
        int x = (sourceTexture.width - size) / 2;
        int y = (sourceTexture.height - size) / 2;

        Texture2D croppedTexture = new Texture2D(size, size);
        Color[] pixels = sourceTexture.GetPixels(x, y, size, size);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        Texture2D resizedTexture = croppedTexture;
        if (size > maxSize)
        {
            resizedTexture = ResizeTexture(croppedTexture, maxSize, maxSize);
            UnityEngine.Object.Destroy(croppedTexture);
        }

        Sprite sprite = Sprite.Create(resizedTexture, new Rect(0, 0, resizedTexture.width, resizedTexture.height), new Vector2(0.5f, 0.5f));
        return sprite;
    }

    private static Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        
        Texture2D result = new Texture2D(targetWidth, targetHeight);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();
        
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        
        return result;
    }

    public static void SaveImage(Sprite sprite, string fileName, int maxSize = 500)
    {
        if (sprite == null)
        {
            Debug.LogError("Cannot save null sprite");
            return;
        }

        try
        {
            Texture2D sourceTexture = sprite.texture;
            Rect rect = sprite.textureRect;
            
            RenderTexture fullTexture = RenderTexture.GetTemporary(
                sourceTexture.width, 
                sourceTexture.height, 
                0, 
                RenderTextureFormat.Default, 
                RenderTextureReadWrite.Linear);
            
            Graphics.Blit(sourceTexture, fullTexture);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = fullTexture;
            
            Texture2D readableTexture = new Texture2D((int)rect.width, (int)rect.height);
            int yCoord = sourceTexture.height - (int)rect.y - (int)rect.height;
            readableTexture.ReadPixels(new Rect(rect.x, yCoord, rect.width, rect.height), 0, 0);
            readableTexture.Apply();
            
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(fullTexture);
            
            Texture2D finalTexture = readableTexture;
            if (readableTexture.width > maxSize || readableTexture.height > maxSize)
            {
                int newWidth = readableTexture.width;
                int newHeight = readableTexture.height;
                
                if (newWidth > newHeight)
                {
                    newHeight = (int)(newHeight * ((float)maxSize / newWidth));
                    newWidth = maxSize;
                }
                else
                {
                    newWidth = (int)(newWidth * ((float)maxSize / newHeight));
                    newHeight = maxSize;
                }
                
                finalTexture = ResizeTexture(readableTexture, newWidth, newHeight);
                UnityEngine.Object.Destroy(readableTexture);
            }
            
            byte[] bytes = finalTexture.EncodeToPNG();
            string filePath = GetFilePath(fileName);
            File.WriteAllBytes(filePath, bytes);
            
            UnityEngine.Object.Destroy(finalTexture);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save image {fileName}: {ex.Message}");
        }
    }
}



