using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] Rigidbody rb;
    [SerializeField] Collider collid;
    public ScriptableItem scriptableItem;
    bool isInsideInventory;  // When it's inside inventory, the gravity will be disabled
    static bool inventoryIsOnCooldown;

    void Update()
    {
        if (!isInsideInventory) rb.AddForce(Vector3.down * scriptableItem.weight, ForceMode.Acceleration);  // Item's gravity depending on its mass
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 8f) audioSource.PlayOneShot(scriptableItem.sound);  // Play sound when the item hits something with enough force
    }

    void OnMouseDown()
    {
        // Prevent taking or dropping items too fast
        if (inventoryIsOnCooldown) return;
        inventoryIsOnCooldown = true;
        this.Wait(0.3f, () => inventoryIsOnCooldown = false);  // This is more perfromance-friendly than constant checking of cooldown += Time.deltatime; in update

        // Take, if not in inventory, or drop if in inventory
        if (!isInsideInventory) Inventory.Instance.AddItem(this);
        else Inventory.Instance.RemoveItem(this);
    }

    public void AddOrRemove(bool add)  // Inventory itself calls for this item's method when needed
    {
        if (add)
        {
            isInsideInventory = true;
            rb.isKinematic = true;  // Freeze rotation and position while inside inventory(animator will itself rotate it)
            collid.isTrigger = true;  // To not crash with anything inside inventory
        }
        else
        {
            isInsideInventory = false;
            rb.isKinematic = false;
            collid.isTrigger = false;
        }
    }
}