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
            textMesh.transform.position = pos + Vector3.up * 1.8f;
            textMesh.text = infoTile.achievement.description + "\n " + AchievementManager.Instance.GetProgress(infoTile.achievement.id) + "/" + infoTile.achievement.targetProgress;
        }
        else
        {
            textMesh.gameObject.SetActive(false);
        }
    }
}