using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using Unity.Mathematics.Geometry;
using UnityEngine.InputSystem;

public class TileHoverSystem : MonoBehaviour
{
    public List<Camera> Camera;
    public Tilemap tilemap;
    public TextMeshPro textMesh;

    void Update()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = new(0,0,0);
        
        foreach (var cam in Camera)
        {
            if (cam.gameObject.activeSelf)
                mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, Mathf.Abs(cam.transform.position.z)));
        }

        Vector3Int cell = tilemap.WorldToCell(mouseWorld);

        TileBase tile = tilemap.GetTile(cell);
        InfoTile infoTile = tile as InfoTile;

        if (infoTile != null)
        {
            Vector3 pos = tilemap.GetCellCenterWorld(cell);

            textMesh.gameObject.SetActive(true);
            textMesh.transform.position = pos + Vector3.up * 2.4f;

            var achievement = infoTile.achievement;
            if (achievement != null)
            {
                var progress = AchievementManager.Instance != null ? AchievementManager.Instance.GetProgress(achievement.id) : null;
                string titleText = infoTile.hackName + "\n \n";
                string progressText = progress != null ? $"{progress.currentProgress}/{achievement.targetProgress}" : $"0/{achievement.targetProgress}";
                textMesh.text = titleText + achievement.description + "\n " + progressText;
            }
            else
            {
                textMesh.text = string.Empty;
            }        }
        else
        {
            textMesh.gameObject.SetActive(false);
        }
    }
}