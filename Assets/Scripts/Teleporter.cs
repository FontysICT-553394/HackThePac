using System;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("Teleporter Settings")]
    [SerializeField] private Transform targetTeleportLocation;
    [SerializeField] private float teleportOffset = -0.6f;
    [SerializeField] private float teleportCooldown = 0.1f;

    [Header("Secret Teleporter Settings")]
    [SerializeField] private bool isSecretTeleporter = false;
    [SerializeField] private int secretTeleportAfter = 3;
    [SerializeField] private Transform secretTeleportLocation;
    [SerializeField] private float secretTeleportTimeWindow = 1f;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject secretCamera;
    
    private float _lastTeleportTime = -Mathf.Infinity;
    private int secretTeleportCounter = 0;
    
    private void Start()
    {
        _lastTeleportTime = Time.time;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time - _lastTeleportTime < teleportCooldown)
            return;

        if (TrySecretTeleport(other.transform))
            return;

        var otherTeleporter = targetTeleportLocation.GetComponent<Teleporter>();
        if (otherTeleporter != null)
            otherTeleporter._lastTeleportTime = Time.time;

        Teleport(other.transform, targetTeleportLocation);

        _lastTeleportTime = Time.time;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time - _lastTeleportTime < teleportCooldown)
            return;

        if (TrySecretTeleport(other.transform))
            return;

        var otherTeleporter = targetTeleportLocation.GetComponent<Teleporter>();
        if (otherTeleporter != null)
            otherTeleporter._lastTeleportTime = Time.time;

        Teleport(other.transform, targetTeleportLocation);

        _lastTeleportTime = Time.time;
    }

    private void Teleport(Transform entity, Transform location)
    {
        entity.transform.position = new Vector3(location.position.x, location.position.y, entity.transform.position.z) 
                                    + (Vector3)(Vector2) location.right * teleportOffset;
    }
    
    private bool TrySecretTeleport(Transform entity)
    {
        if (!isSecretTeleporter)
            return false;

        if (Time.time - _lastTeleportTime < secretTeleportTimeWindow)
        {
            secretTeleportCounter++;
        }
        else
        {
            secretTeleportCounter = 1;
        }

        if (secretTeleportCounter >= secretTeleportAfter)
        {
            Teleport(entity, secretTeleportLocation);
            mainCamera.SetActive(false);
            secretCamera.SetActive(true);

            _lastTeleportTime = Time.time;
            secretTeleportCounter = 0;
            return true;
        }

        return false;
    }
}
