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

    public bool isHackForPacMan = false;
    public bool isHackForGhost = false;
    // These variables store runtime state. Since this is a ScriptableObject,
    // this state is shared by all tiles using this specific asset file.
    public bool isHackLocked = true;
    public bool isHackEnabled = false;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);

        if (achievement == null)
        {
            tileData.sprite = hackLockedSprite;
            return;
        }

        // We removed the '_initialized' latch. 
        // Now it checks the AchievementManager every time the tile data is requested at runtime.
        // This ensures that if the Tilemap renders BEFORE AchievementManager initializes, 
        // it will correct itself on the next refresh instead of getting stuck in a locked state.
        if (Application.isPlaying && AchievementManager.Instance != null)
        {
            bool completed = AchievementManager.Instance.IsCompleted(achievement.id);

            if (completed)
            {
                isHackLocked = false;
            }
            else
            {
                isHackLocked = true;
                // If the hack is locked, force it to be disabled
                isHackEnabled = false;
            }
        }

        if (isHackLocked)
            tileData.sprite = hackLockedSprite;
        else if (isHackEnabled)
            tileData.sprite = hackEnabledSprite;
        else
            tileData.sprite = hackDisabledSprite;
    }

    // Ensures state is reset when the object is loaded (e.g. entering Play Mode with Domain Reload)
    private void OnEnable()
    {
        isHackLocked = true;
        isHackEnabled = false;
    }
}
