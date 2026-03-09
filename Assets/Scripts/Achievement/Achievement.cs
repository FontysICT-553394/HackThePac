using UnityEngine;

[CreateAssetMenu(menuName = "Achievements/Create Achievement")]
public class Achievement : ScriptableObject
{
    public string id;
    public string title;
    [TextArea] public string description;
    public Sprite icon;

    [Tooltip("Target value to complete the achievement (e.g. 10 kills, 5 levels)")]
    public int targetProgress = 1;
}