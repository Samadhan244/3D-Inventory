using UnityEngine;

public class Item : MonoBehaviour
{
    GlobalReferences global => GlobalReferences.Instance;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Rigidbody rb;
    [SerializeField] Collider collid;
    public ScriptableItem scriptableItem;
    [SerializeField] bool isInsideInventory, isInsideHand;  // When it's inside inventory or hand, the gravity will be disabled
    float cooldown;  // To prevent spamming adding and dropping
    static readonly int DropHash = Animator.StringToHash("Drop");

    void Update()
    {
        if (cooldown > 0) cooldown -= Time.deltaTime;
        if (!isInsideInventory && !isInsideHand) rb.AddForce(Vector3.down * scriptableItem.weight, ForceMode.Acceleration);  // Item's gravity depending on its mass
    }

    // Play sound when the item hits something with enough force
    void OnCollisionEnter(Collision collision) { if (collision.relativeVelocity.magnitude > 8f) audioSource.PlayOneShot(scriptableItem.sound); }  // Play sound when the item hits something with enough force

    // When we move held item to the chest, it is added inside
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Chest") && isInsideHand)
        {
            global.inventory.AddItem(this);
            isInsideInventory = true;
            rb.isKinematic = true;
            collid.isTrigger = true;
        }
    }

    float distanceBetweenItemAndCamera = 2;
    void OnMouseDrag()
    {
        if (isInsideInventory) return;
        isInsideHand = true;
        distanceBetweenItemAndCamera = Mathf.Clamp(distanceBetweenItemAndCamera + Input.GetAxis("Mouse ScrollWheel"), 1f, 3f);  // Adjust distance with mouse scroll
        Vector3 point = global.cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceBetweenItemAndCamera));
        rb.MovePosition(point);
    }

    void OnMouseUp()
    {
        if (isInsideInventory) return;
        isInsideHand = false;
        distanceBetweenItemAndCamera = 2;
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
                isInsideHand = false;
                rb.isKinematic = false;
                collid.isTrigger = false;
            });
        }
    }
}