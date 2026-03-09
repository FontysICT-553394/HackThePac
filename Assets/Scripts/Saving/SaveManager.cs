using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string SaveDirectory => Path.Combine(Application.persistentDataPath, "saves");

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Saves data of type T to a JSON file.
    /// </summary>
    public void Save<T>(string fileName, T data)
    {
        string path = GetFilePath(fileName);
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
            Debug.Log($"[SaveManager] Saved to {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to save {fileName}: {e.Message}");
        }
    }

    /// <summary>
    /// Loads data of type T from a JSON file. Returns default if not found.
    /// </summary>
    public T Load<T>(string fileName) where T : new()
    {
        string path = GetFilePath(fileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveManager] File not found: {path}");
            return new T();
        }

        try
        {
            string json = File.ReadAllText(path);
            T data = JsonUtility.FromJson<T>(json);
            Debug.Log($"[SaveManager] Loaded from {path}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to load {fileName}: {e.Message}");
            return new T();
        }
    }

    /// <summary>
    /// Checks whether a save file exists.
    /// </summary>
    public bool DoesSaveExists(string fileName)
    {
        return File.Exists(GetFilePath(fileName));
    }

    /// <summary>
    /// Deletes a save file if it exists.
    /// </summary>
    public void Delete(string fileName)
    {
        string path = GetFilePath(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[SaveManager] Deleted {path}");
        }
    }

    private string GetFilePath(string fileName)
    {
        if (!fileName.EndsWith(".json"))
            fileName += ".json";
        return Path.Combine(SaveDirectory, fileName);
    }
}