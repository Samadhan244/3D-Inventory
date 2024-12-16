using UnityEngine;

public class Item : MonoBehaviour
{
    GlobalReferences global => GlobalReferences.Instance;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Rigidbody rb;
    [SerializeField] Collider collid;
    public ScriptableItem scriptableItem;
    bool isInsideInventory;  // When it's inside inventory, the gravity will be disabled
    static bool inventoryIsOnCooldown;
    static readonly int DropHash = Animator.StringToHash("Drop");

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

        if (!isInsideInventory)
        {
            global.inventory.AddItem(this);
            isInsideInventory = true;
            rb.isKinematic = true;
            collid.isTrigger = true;
        }
    }


    public void Drop()
    {
        if (isInsideInventory)  // Removing item
        {
            GetComponentInParent<Animator>().Play(DropHash);  // Do the animation first, then remove it
            this.Wait(0.3f, () =>
            {
                global.inventory.RemoveItem(this);
                isInsideInventory = false;
                rb.isKinematic = false;
                collid.isTrigger = false;
            });
        }
    }
}