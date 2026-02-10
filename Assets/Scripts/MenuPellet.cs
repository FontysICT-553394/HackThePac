using UnityEngine;

public class MenuPellet : Pellet
{
    protected override void Eat()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pacman"))
        {
            Eat();
        }
    }
}