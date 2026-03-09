using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("All achievement definitions (assign in Inspector)")]
    [SerializeField] private List<Achievement> allAchievements = new List<Achievement>();

    private const string SaveFileName = "achievements";

    private Dictionary<string, AchievementEntry> _progressMap = new Dictionary<string, AchievementEntry>();

    /// <summary>
    /// Fired when an achievement is fully completed. Passes the Achievement.
    /// </summary>
    public event Action<Achievement> OnAchievementUnlocked;

    /// <summary>
    /// Fired when any achievement progress changes. Passes the definition and current entry.
    /// </summary>
    public event Action<Achievement, AchievementEntry> OnAchievementProgressChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        LoadProgress();
    }

    /// <summary>
    /// Add progress to an achievement by its id. Automatically saves and fires events.
    /// </summary>
    public void AddProgress(string achievementId, int amount = 1)
    {
        var definition = allAchievements.FirstOrDefault(a => a.id == achievementId);
        if (definition == null)
        {
            Debug.LogWarning($"[AchievementManager] Unknown achievement id: {achievementId}");
            return;
        }

        if (!_progressMap.TryGetValue(achievementId, out var entry))
        {
            entry = new AchievementEntry { id = achievementId, currentProgress = 0, isCompleted = false };
            _progressMap[achievementId] = entry;
        }

        if (entry.isCompleted)
            return;

        entry.currentProgress = Mathf.Min(entry.currentProgress + amount, definition.targetProgress);
        OnAchievementProgressChanged?.Invoke(definition, entry);

        if (entry.currentProgress >= definition.targetProgress)
        {
            entry.isCompleted = true;
            Debug.Log($"[AchievementManager] Achievement unlocked: {definition.title}");
            OnAchievementUnlocked?.Invoke(definition);
        }

        SaveProgress();
    }

    /// <summary>
    /// Returns the progress entry for a given achievement id, or null if none tracked yet.
    /// </summary>
    public AchievementEntry GetProgress(string achievementId)
    {
        _progressMap.TryGetValue(achievementId, out var entry);
        return entry;
    }

    /// <summary>
    /// Returns true if the achievement is completed.
    /// </summary>
    public bool IsCompleted(string achievementId)
    {
        return _progressMap.TryGetValue(achievementId, out var entry) && entry.isCompleted;
    }

    /// <summary>
    /// Returns all achievement definitions paired with their current progress.
    /// </summary>
    public List<(Achievement definition, AchievementEntry entry)> GetAllStatus()
    {
        var result = new List<(Achievement, AchievementEntry)>();
        foreach (var def in allAchievements)
        {
            _progressMap.TryGetValue(def.id, out var entry);
            entry ??= new AchievementEntry { id = def.id, currentProgress = 0, isCompleted = false };
            result.Add((def, entry));
        }
        return result;
    }

    /// <summary>
    /// Resets all achievement progress and deletes the save file.
    /// </summary>
    public void ResetAll()
    {
        _progressMap.Clear();
        SaveManager.Instance.Delete(SaveFileName);
        Debug.Log("[AchievementManager] All achievement progress reset.");
    }

    private void SaveProgress()
    {
        var data = new AchievementSaveData
        {
            entries = _progressMap.Values.ToList()
        };
        SaveManager.Instance.Save(SaveFileName, data);
    }

    private void LoadProgress()
    {
        var data = SaveManager.Instance.Load<AchievementSaveData>(SaveFileName);
        _progressMap.Clear();
        foreach (var entry in data.entries)
        {
            _progressMap[entry.id] = entry;
        }
        Debug.Log($"[AchievementManager] Loaded {_progressMap.Count} achievement entries.");
    }
}