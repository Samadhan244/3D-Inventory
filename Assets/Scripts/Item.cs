using UnityEngine;

public class Item : MonoBehaviour
{
    GlobalReferences global => GlobalReferences.Instance;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Rigidbody rb;
    [SerializeField] Collider collid;
    public ScriptableItem scriptableItem;
    [SerializeField] bool isInsideInventory, isInsideHand;  // Disables gravity when the item is inside the inventory or in hand
    float cooldown;  // Prevents dropping spam due to animation timing
    static readonly int DropHash = Animator.StringToHash("Drop");

    void Update()
    {
        if (cooldown > 0) cooldown -= Time.deltaTime;
        if (!isInsideInventory && !isInsideHand) rb.AddForce(Vector3.down * scriptableItem.weight, ForceMode.Acceleration);  // Applies gravity to the item based on its weight
    }

    // Plays a sound when the item hits something with sufficient force
    void OnCollisionEnter(Collision collision) { if (collision.relativeVelocity.magnitude > 8f) audioSource.PlayOneShot(scriptableItem.sound); }

    // Adds the item to the chest when the held item is moved into it
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

    float distance;
    void OnMouseDrag()
    {
        if (isInsideInventory) return;

        if (!isInsideHand) distance = Vector3.Distance(transform.position, global.cam.transform.position);
        if (distance > 5) return;  // Prevent picking up the item if it's too far away

        isInsideHand = true;
        distance = Mathf.Clamp(distance + Input.GetAxis("Mouse ScrollWheel") * 3, 1f, 4f);  // Adjusts the distance using mouse scroll
        Vector3 point = global.cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance));  // Move the item based on mouse position (left-right and up-down)
        point.y = Mathf.Max(point.y, 0f);  // Prevent moving the item below the ground (assuming ground is at Y = 0)
        rb.MovePosition(point);
    }

    void OnMouseUp()
    {
        if (isInsideInventory) return;
        isInsideHand = false;  // Stop dragging and re-enable gravity for the item
    }

    // Called by the player when the left mouse button is released to drop the item from the inventory. This approach is better than using OnMouseUp in the item itself
    public void Drop()
    {
        if (cooldown > 0) return;  // Prevents spamming
        else cooldown = 0.3f;

        if (isInsideInventory)
        {
            GetComponentInParent<Animator>().Play(DropHash);  // Plays the drop animation first, then removes the item
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