using UnityEngine;

public interface IGhost
{
    GhostState CurrentState { get; }
    
    void Move();
    void SetTarget(Vector3 target);
    void EnterChaseMode();
    void EnterScatterMode();
    void EnterFrightenedMode();
    void EnterEatenMode();
    void ResetToHome();
}
