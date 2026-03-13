using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Ghost))]
public class GhostEatenBehavior : GhostBehavior{
 private bool _isReturningHome;

 public override void Enable(float duration)
 {
 enabled = true;
 _isReturningHome = true;
 ghost.movement.speedMultiplier =2f;
 }

 public override void Disable()
 {
 _isReturningHome = false;
 enabled = false;
 CancelInvoke();
 StopAllCoroutines();
 }

 private void Update()
 {
 if (!_isReturningHome || ghost.home == null || ghost.home.outside == null)
 return;

 float distanceToOutside = Vector2.Distance(transform.position, ghost.home.outside.position);

 if (distanceToOutside <0.25f)
 {
 _isReturningHome = false;
 ghost.movement.SetDirection(Vector2.zero, true);
 StartCoroutine(EnterHomeTransition());
 }
 }

 private void OnTriggerEnter2D(Collider2D other)
 {
 Node node = other.GetComponent<Node>();

 if (node != null && enabled && _isReturningHome && ghost.home != null && ghost.home.outside != null)
 {
 Vector2 direction = ghost.movement.direction;
 float minDistance = float.MaxValue;
 Vector3 targetPosition = ghost.home.outside.position;

 foreach (Vector2 availableDirection in node.availableDirections)
 {
 if (IsReverseDirection(availableDirection, node))
 continue;

 Vector3 newPosition = transform.position + (Vector3)availableDirection;
 float distance = (targetPosition - newPosition).sqrMagnitude;

 if (distance < minDistance)
 {
 direction = availableDirection;
 minDistance = distance;
 }
 }

 ghost.movement.SetDirection(direction);
 }
 }

 private IEnumerator EnterHomeTransition()
 {
 ghost.movement.SetDirection(Vector2.down, true);
 ghost.movement.rb.isKinematic = true;
 ghost.movement.enabled = false;

 Collider2D col = ghost.GetComponent<Collider2D>();
 if (col != null) col.enabled = false;

 Vector3 startPos = transform.position;
 float duration =0.5f;
 float elapsed =0f;

 while (elapsed < duration)
 {
 ghost.SetPosition(Vector3.Lerp(startPos, ghost.home.outside.position, elapsed / duration));
 elapsed += Time.deltaTime;
 yield return null;
 }

 ghost.SetPosition(ghost.home.outside.position);
 elapsed =0f;

 while (elapsed < duration)
 {
 ghost.SetPosition(Vector3.Lerp(ghost.home.outside.position, ghost.home.inside.position, elapsed / duration));
 elapsed += Time.deltaTime;
 yield return null;
 }

 ghost.SetPosition(ghost.home.inside.position);

 if (col != null) col.enabled = true;

 ghost.movement.rb.isKinematic = false;
 ghost.movement.enabled = true;
 ghost.movement.speedMultiplier =1f;

 if (ghost.frightened != null)
 {
 ghost.frightened.eaten = false;
 ghost.frightened.Disable();
 }

 Disable();
 ghost.home.Enable(ghost.home.duration);
 }
}
