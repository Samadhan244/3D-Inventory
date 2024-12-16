using UnityEngine;

public class Item : MonoBehaviour
{
    GlobalReferences global => GlobalReferences.Instance;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Rigidbody rb;
    [SerializeField] Collider collid;
    public ScriptableItem scriptableItem;
    bool isInsideInventory;  // When it's inside inventory, the gravity will be disabled
    float cooldown;  // To prevent spamming adding and dropping
    static readonly int DropHash = Animator.StringToHash("Drop");

    void Update()
    {
        if (cooldown > 0) cooldown -= Time.deltaTime;
        if (!isInsideInventory) rb.AddForce(Vector3.down * scriptableItem.weight, ForceMode.Acceleration);  // Item's gravity depending on its mass
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 8f) audioSource.PlayOneShot(scriptableItem.sound);  // Play sound when the item hits something with enough force
    }

    void OnMouseDown()
    {
        if (global.isChestOpen) return;  // can't pick up items when checking inside chest
        if (cooldown > 0) return;  // Prevent spamming
        else cooldown = 0.3f;

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
        if (cooldown > 0) return;  // Prevent spamming
        else cooldown = 0.3f;

        if (isInsideInventory)  // Removing item
        {
            GetComponentInParent<Animator>().Play(DropHash);  // Do the animation first, then remove it
            global.inventory.PlaySound(3);
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