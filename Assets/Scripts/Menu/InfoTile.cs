// Assets/Scripts/Menu/InfoTile.cs
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/Info Tile")]
public class InfoTile : Tile
{
    public Achievement achievement;

    public string hackName;
    public Sprite hackLockedSprite;
    public Sprite hackDisabledSprite;
    public Sprite hackEnabledSprite;

    public bool isHackLocked = true;
    public bool isHackEnabled = false;

    // Avoid caching tileData/tilemap/position; GetTileData is called often.
    private bool _initialized;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);

        if (achievement == null)
        {
            tileData.sprite = hackLockedSprite;
            return;
        }

        // One-time init of runtime state per tile instance.
        if (!_initialized)
        {
            // Interpret Achievement completion however you intend.
            // This keeps behavior similar to your original intent without caching tileData.
            bool completed = AchievementManager.Instance != null &&
                             AchievementManager.Instance.IsCompleted(achievement.id);

            if (completed)
            {
                isHackLocked = false;
                isHackEnabled = false;
            }
            else
            {
                isHackLocked = true;
                isHackEnabled = false;
            }

            _initialized = true;
        }

        if (isHackLocked)
            tileData.sprite = hackLockedSprite;
        else if (isHackEnabled)
            tileData.sprite = hackEnabledSprite;
        else
            tileData.sprite = hackDisabledSprite;
    }

    // Optional: call this when starting a new game/scene if you need re-init.
    public void ResetRuntimeState()
    {
        _initialized = false;
    }
}