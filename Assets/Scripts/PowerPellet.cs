using UnityEngine;

public class PowerPellet : Pellet
{
    public float duration = 8.0f;
    
    private void Awake()
    {
        this.points = 50;
    }
    
    protected override void Eat()
    {
        FindObjectOfType<GameManager>().PowerPelletEaten(this);
    }
}
