using UnityEngine;

public class Blinky : MonoBehaviour, IGhost
{
    public GhostState CurrentState { get; }
    public Transform pacmanTransform { get; }
    public Transform homeTransform { get; }

    public void Move()
    {
        throw new System.NotImplementedException();
    }

    public void SetTarget(Vector3 target)
    {
        throw new System.NotImplementedException();
    }

    public void EnterChaseMode()
    {
        throw new System.NotImplementedException();
    }

    public void EnterScatterMode()
    {
        throw new System.NotImplementedException();
    }

    public void EnterFrightenedMode()
    {
        throw new System.NotImplementedException();
    }

    public void EnterEatenMode()
    {
        throw new System.NotImplementedException();
    }

    public void ResetToHome()
    {
        throw new System.NotImplementedException();
    }
}
