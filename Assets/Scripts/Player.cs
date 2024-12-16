using UnityEngine;

public class Player : MonoBehaviour
{
    GlobalReferences global => GlobalReferences.Instance;
    [SerializeField] Rigidbody rb;
    [SerializeField] Animator animator;  // Handles animations for footstep sounds
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] footstepSounds;
    [SerializeField] float moveSpeed = 5f, rotationSpeed = 5f, pitch = 15f;
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");  // Optimized for performance

    void Update()
    {
        MoveAndRotation();
        DropItemsByMouseUp();
        DisplayInventoryByMouseDown();
    }

    public void Footsteps()  // Called from animation to play a random footstep sound
    {
        int randNum = Random.Range(0, footstepSounds.Length);
        audioSource.PlayOneShot(footstepSounds[randNum]);
    }

    void MoveAndRotation()
    {
        animator.SetBool(IsMoving, rb.velocity.magnitude > 0.3f);  // Triggers footstep sounds only for significant movement (>0.3 to ignore small impacts)

        if (global.isChestOpen) return;  // Prevent movement while the inventory is open
        Vector3 moveDirection = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
        rb.velocity = new Vector3(moveDirection.x, 0, moveDirection.z).normalized * moveSpeed;
        rb.rotation = Quaternion.Euler(pitch, rb.rotation.eulerAngles.y + Input.GetAxis("Rotation") * rotationSpeed * Time.deltaTime, 0);
    }

    void DropItemsByMouseUp()
    {
        if (Input.GetMouseButtonUp(0))  // Detects mouse button release
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Item"))
                hit.collider.GetComponent<Item>()?.Drop();  // Drops the item if the clicked object is an item
        }
    }

    bool isUiDisplayed;
    void DisplayInventoryByMouseDown()
    {
        if (Input.GetMouseButtonDown(0) && !isUiDisplayed)  // Detects mouse button press and toggles UI
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Chest"))
            {
                isUiDisplayed = true;
                global.inventory.ShowUI(true);
            }
        }

        if (Input.GetMouseButtonUp(0) && isUiDisplayed)  // Detects mouse button release to hide the inventory UI
        {
            isUiDisplayed = false;
            global.inventory.ShowUI(false);
        }
    }
}