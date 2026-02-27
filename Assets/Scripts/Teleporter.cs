using System;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private Transform targetTeleportLocation;
    [SerializeField] private float teleportOffset = 8f;
    [SerializeField] private float teleportCooldown = 0.3f;

    private float _lastTeleportTime = -Mathf.Infinity;

    private void OnTriggerEnter2D(Collider2D other) 
    { 
        if (Time.time - _lastTeleportTime < teleportCooldown) 
            return;
        
        var otherTeleporter = targetTeleportLocation.GetComponent<Teleporter>();
        if (otherTeleporter != null) 
            otherTeleporter._lastTeleportTime = Time.time;

        other.transform.position = new Vector3(targetTeleportLocation.position.x, other.transform.position.y, other.transform.position.z) 
                                   + (Vector3)(Vector2)targetTeleportLocation.up * 0.2f;

        _lastTeleportTime = Time.time;
    }
    
    private void OnTriggerStay2D(Collider2D other) 
    { 
        if (Time.time - _lastTeleportTime < teleportCooldown) 
            return;
    
        var otherTeleporter = targetTeleportLocation.GetComponent<Teleporter>();
        if (otherTeleporter != null) 
            otherTeleporter._lastTeleportTime = Time.time;

        other.transform.position = new Vector3(targetTeleportLocation.position.x, targetTeleportLocation.position.y, other.transform.position.z) 
                                   + (Vector3)(Vector2)targetTeleportLocation.up * 0.2f;

        _lastTeleportTime = Time.time;
    }
    
    
}
