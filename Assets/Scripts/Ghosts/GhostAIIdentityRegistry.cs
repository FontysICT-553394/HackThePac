using UnityEngine;

public class GhostAIIdentityRegistry : MonoBehaviour
{
    public GhostMovement BlinkyMovement { get; private set; }

    public void RegisterBlinky(GhostMovement blinky)
    {
        BlinkyMovement = blinky;
    }
}