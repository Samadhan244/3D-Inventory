using UnityEngine;

public class GlobalReferences : MonoBehaviour
{
    public static GlobalReferences Instance { get; private set; }  // Singleton instance
    public Inventory inventory;
    public bool isChestOpen;
    public int usedSlots, totalSlots = 20;
    public int droppedItemsCount, maxItemsOnSceneToDrop = 10;

    void Awake()
    {
        if (!Instance) { Instance = this; DontDestroyOnLoad(gameObject); }  // Ensure only one instance exists. Optional: Prevents destruction on scene load
        else Destroy(gameObject);
    }
}