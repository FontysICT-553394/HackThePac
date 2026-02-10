using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AnimatedSprite : MonoBehaviour
{
    public SpriteRenderer spriteRenderer { get; private set; }
    public Sprite[] sprites;
    public float animationTime = 0.25f;
    public int animationFrame { get; private set; }
    public bool loop = true;

    public bool IsFinished { get; private set; } = false;

    private void Awake()
    {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        InvokeRepeating(nameof(Advance), this.animationTime, this.animationTime);
    }

    private void Advance()
    {
        if (this.spriteRenderer == null) {
            this.spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (this.spriteRenderer == null || !this.spriteRenderer.enabled) return;
        
        this.animationFrame++;

        if (this.animationFrame >= this.sprites.Length)
        {
            if (loop)
            {
                this.animationFrame = 0;
                IsFinished = false;
            }
            else
            {
                this.animationFrame = sprites.Length - 1;
                IsFinished = true; 
            }
        }

        if (this.animationFrame >= 0 && this.animationFrame < this.sprites.Length)
        {
            this.spriteRenderer.sprite = this.sprites[this.animationFrame];
        }
    }

    public void Restart()
    {
        this.animationFrame = -1;
        IsFinished = false; // <-- reset when restarting
        Advance();
    }
}