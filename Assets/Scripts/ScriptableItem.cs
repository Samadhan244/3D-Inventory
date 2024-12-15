using UnityEngine;

[CreateAssetMenu(fileName = "Items")]
public class ScriptableItem : ScriptableObject
{
    [field: SerializeField] public int id { get; private set; }
    [field: SerializeField] public new string name { get; private set; }
    [field: SerializeField] public float weight { get; private set; }  // Higher the weight, stronger it falls
    [field: SerializeField] public AudioClip sound { get; private set; }  // Each type has its own dropping sound
    [field: SerializeField] public Type type { get; private set; }
    public enum Type { Food, Potion, Weapon }
}