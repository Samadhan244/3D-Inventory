using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] Rigidbody rb;
    [SerializeField] ScriptableItem item;
    public bool isInsideInventory;  // When it's inside inventory, the gravity will be disabled
    public ScriptableItem ScriptableItem => item;  // Optional getter for other scripts

    void Update()
    {
        if (!isInsideInventory) rb.AddForce(Vector3.down * item.weight, ForceMode.Acceleration);  // Item's gravity depending on its mass
        else rb.velocity = Vector3.zero;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 9f) audioSource.PlayOneShot(item.sound);  // Play sound when the item hits something with enough force
    }

    void OnMouseDown()
    {
        Inventory.Instance.AddItem(this);
    }
}