using UnityEngine;

[RequireComponent(typeof(Ghost))]
public class GhostAISelector : MonoBehaviour{
 public enum GhostAIType {
 Blinky,
 Pinky,
 Inky,
 Clyde }

 [Header("Ghost Type")]
 [SerializeField] private GhostAIType aiType;

 [Header("Pinky")]
 [SerializeField] private int pinkyTilesAhead =4;

 [Header("Inky")]
 [SerializeField] private int inkyTilesAhead =2;

 [Header("Clyde")]
 [SerializeField] private float clydeChaseDistance =8f;
 [SerializeField] private Vector2 clydeScatterCorner = new Vector2(-100f, -100f);

 private void Awake()
 {
 DisableAllStrategies();
 EnableSelectedStrategy();
 }

 private void DisableAllStrategies()
 {
 GhostTargetStrategy[] strategies = GetComponents<GhostTargetStrategy>();

 foreach (GhostTargetStrategy strategy in strategies)
 {
 strategy.enabled = false;
 }
 }

 private void EnableSelectedStrategy()
 {
 switch (aiType)
 {
 case GhostAIType.Blinky:
 {
 GhostBlinkyTarget strategy = GetOrAdd<GhostBlinkyTarget>();
 strategy.enabled = true;
 break;
 }

 case GhostAIType.Pinky:
 {
 GhostPinkyTarget strategy = GetOrAdd<GhostPinkyTarget>();
 SetPrivateField(strategy, "tilesAhead", pinkyTilesAhead);
 strategy.enabled = true;
 break;
 }

 case GhostAIType.Inky:
 {
 GhostInkyTarget strategy = GetOrAdd<GhostInkyTarget>();
 SetPrivateField(strategy, "tilesAhead", inkyTilesAhead);
 strategy.enabled = true;
 break;
 }

 case GhostAIType.Clyde:
 {
 GhostClydeTarget strategy = GetOrAdd<GhostClydeTarget>();
 SetPrivateField(strategy, "chaseDistance", clydeChaseDistance);
 SetPrivateField(strategy, "scatterCorner", clydeScatterCorner);
 strategy.enabled = true;
 break;
 }
 }
 }

 private T GetOrAdd<T>() where T : Component {
 T component = GetComponent<T>();

 if (component == null)
 component = gameObject.AddComponent<T>();

 return component;
 }

 private void SetPrivateField<TComponent, TValue>(TComponent component, string fieldName, TValue value)
 where TComponent : Component {
 var field = typeof(TComponent).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

 if (field != null)
 field.SetValue(component, value);
 }
}
