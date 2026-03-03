using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class Pellet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private string pacmanTag = "Player";

    private bool eaten;

    private void Reset()
    {
        // Zorg dat je pellet collider een trigger is
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (eaten) return;
        if (!other.CompareTag(pacmanTag)) return;

        eaten = true;

        // 1) notify ghost release system
        if (GhostHouseController.Instance != null)
            GhostHouseController.Instance.OnDotEaten();

        // 3) destroy pellet
        Destroy(gameObject);
    }
}
