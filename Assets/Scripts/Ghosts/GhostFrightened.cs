
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GhostFrightened : GhostBehavior{
 public SpriteRenderer body;
 public SpriteRenderer eyes;
 public SpriteRenderer blue;
 public SpriteRenderer white;

 public bool eaten;

 public override void Enable(float duration)
 {
 base.Enable(duration);

 body.enabled = false;
 eyes.enabled = false;
 blue.enabled = true;
 white.enabled = false;

 Invoke(nameof(Flash), duration /2f);
 }

 public override void Disable()
 {
 base.Disable();

 body.enabled = true;
 eyes.enabled = true;
 blue.enabled = false;
 white.enabled = false;
 }

 public void Eaten()
 {
 eaten = true;

 body.enabled = false;
 eyes.enabled = true;
 blue.enabled = false;
 white.enabled = false;

 enabled = false;
 CancelInvoke();

 if (ghost.chase != null) ghost.chase.Disable();
 if (ghost.scatter != null) ghost.scatter.Disable();

 if (ghost.eatenBehavior != null)
 {
 ghost.eatenBehavior.Enable(0f);
 }
 else if (ghost.home != null)
 {
 ghost.SetPosition(ghost.home.inside.position);
 ghost.home.Enable(ghost.home.duration);
 }
 }

 private void OnEnable()
 {
 blue.GetComponent<AnimatedSprite>().Restart();

 if (ghost.movement != null)
 ghost.movement.speedMultiplier =0.5f;

 eaten = false;
 }

 private void OnDisable()
 {
 if (ghost.movement != null)
 ghost.movement.speedMultiplier =1f;

 eaten = false;
 }

 private void Flash()
 {
 if (!eaten)
 {
 blue.enabled = false;
 white.enabled = true;
 white.GetComponent<AnimatedSprite>().Restart();
 }
 }

 private void OnTriggerEnter2D(Collider2D other)
 {
 Node node = other.GetComponent<Node>();

 if (node != null && enabled)
 {
 Vector2 direction = ghost.movement.direction;
 float maxDistance = float.MinValue;

 foreach (Vector2 availableDirection in node.availableDirections)
 {
 if (IsReverseDirection(availableDirection, node))
 continue;

 Vector3 newPosition = transform.position + (Vector3)availableDirection;
 float distance = (ghost.target.position - newPosition).sqrMagnitude;

 if (distance > maxDistance)
 {
 direction = availableDirection;
 maxDistance = distance;
 }
 }

 ghost.movement.SetDirection(direction);
 }
 }

 private void OnCollisionEnter2D(Collision2D collision)
 {
 if (collision.gameObject.layer == LayerMask.NameToLayer("Pacman"))
 {
 if (enabled)
 {
 Eaten();
 }
 }
 }
}
