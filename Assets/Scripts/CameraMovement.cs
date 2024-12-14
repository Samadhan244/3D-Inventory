using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Use hotkeys W/A/S/D/Q/E to move the camera")]
    [SerializeField] Rigidbody rb;
    [SerializeField] Animator animator;  // We need animation only for footstep sounds
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] footstepSounds;
    [SerializeField] float moveSpeed = 5, rotationSpeed = 5, pitch = 15;
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");  // Performance-friendly

    void Update()
    {
        animator.SetBool(IsMoving, rb.velocity.magnitude > 0.1f);

        if (Inventory.Instance.isOpen) return;  // Can't move while inventory is open
        Vector3 moveDirection = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
        rb.velocity = new Vector3(moveDirection.x, 0, moveDirection.z).normalized * moveSpeed;
        rb.rotation = Quaternion.Euler(pitch, rb.rotation.eulerAngles.y + Input.GetAxis("Rotation") * rotationSpeed * Time.deltaTime, 0);
    }

    public void Footsteps()  // This method is called from animation which plays random footstep sound
    {
        int randNum = Random.Range(0, footstepSounds.Length);
        audioSource.PlayOneShot(footstepSounds[randNum]);
    }
}