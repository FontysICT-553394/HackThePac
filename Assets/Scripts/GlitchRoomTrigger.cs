using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GlitchRoomTrigger : MonoBehaviour
{
    [SerializeField] private GlitchEffect glitchEffect;

    private void Awake()
    {
        if (glitchEffect == null)
            glitchEffect = FindObjectOfType<GlitchEffect>();

        var collider2d = GetComponent<Collider2D>();
        if (collider2d != null)
            collider2d.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        glitchEffect?.SetPacmanInGlitchRoom(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        glitchEffect?.SetPacmanInGlitchRoom(false);
    }
}