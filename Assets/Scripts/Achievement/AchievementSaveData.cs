using System;
using System.Collections.Generic;

[Serializable]
public class AchievementSaveData
{
    public List<AchievementEntry> entries = new List<AchievementEntry>();
}

[Serializable]
public class AchievementEntry
{
    public string id;
    public int currentProgress;
    public bool isCompleted;
}