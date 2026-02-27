using UnityEngine;
using UnityEngine.Tilemaps;

public class PacManAI : MonoBehaviour
{
    
    private GameObject _blinky, _pinky, _inky, _clyde;

    private Tilemap _pelletTilemap;
    private Tilemap _powerPelletTilemap;
    private Vector2 _currentDirection = Vector2.zero;
    private Vector2 _queuedDirection = Vector2.zero;
    private Rigidbody2D _rb;
    
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        _blinky = GameObject.Find("blinky(Clone)");
        _pinky = GameObject.Find("pinky(Clone)");
        _inky = GameObject.Find("inky(Clone)");
        _clyde = GameObject.Find("clyde(Clone)");

        _pelletTilemap = GameObject.Find("pelletsTilemap").GetComponent<Tilemap>();
        _powerPelletTilemap = GameObject.Find("powerpelletsTilemap").GetComponent<Tilemap>();
    }
   
}
