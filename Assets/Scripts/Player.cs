using UnityEngine;

public class Player : MonoBehaviour
{
    GlobalReferences global => GlobalReferences.Instance;
    [Header("Use hotkeys W/A/S/D/Q/E to move the camera")]
    [SerializeField] Rigidbody rb;
    [SerializeField] Animator animator;  // Animation only for footstep sounds
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] footstepSounds;
    [SerializeField] float moveSpeed = 5, rotationSpeed = 5, pitch = 15;
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");  // Performance-friendly

    void Update()
    {
        MoveAndRotation();
        DropItemsByMouseUp();
    }

    public void Footsteps()  // This method is called from animation which plays random footstep sound
    {
        int randNum = Random.Range(0, footstepSounds.Length);
        audioSource.PlayOneShot(footstepSounds[randNum]);
    }

    void MoveAndRotation()
    {
        animator.SetBool(IsMoving, rb.velocity.magnitude > 0.1f);  // Animation only for footstep sounds

        if (global.isChestOpen) return;  // Can't move while inventory is open
        Vector3 moveDirection = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
        rb.velocity = new Vector3(moveDirection.x, 0, moveDirection.z).normalized * moveSpeed;
        rb.rotation = Quaternion.Euler(pitch, rb.rotation.eulerAngles.y + Input.GetAxis("Rotation") * rotationSpeed * Time.deltaTime, 0);
    }

    void DropItemsByMouseUp()
    {
        if (Input.GetMouseButtonUp(0))  // Mouse2 (Right Mouse Button)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Item"))  hit.collider.GetComponent<Item>()?.Drop();
        }
    }
}