using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }
    [SerializeField] Text itemCountText, weightCountText;
    [SerializeField] Transform[] slots;  // Items are saved in these empty slots
    [SerializeField] Animator animator;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip chestOpen, chestClose, itemPickup;
    public bool isOpen, isOnCd;
    [SerializeField] int currentItems, maxItems = 20;
    [SerializeField] float weightCount;
    static readonly int IsOpenHash = Animator.StringToHash("IsOpen"), DropHash = Animator.StringToHash("Drop");  // Performance-friendly to play animation hashes, instead of string

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GridLayout3D();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) OpenOrCloseInventory();
        if (Input.GetKeyDown(KeyCode.P)) GridLayout3D();
    }

    public void OpenOrCloseInventory()  // This method is called by pressing a hotkey or left-clicking the 3d inventory
    {
        // Opening or closing inventory has a cooldown of 1 second
        if (isOnCd) return;
        else { isOnCd = true; this.Wait(1f, () => isOnCd = false); }

        // Open if closed, close if open
        isOpen = !isOpen;
        animator.SetBool(IsOpenHash, isOpen);
    }

    public void PlaySound(int index)
    {
        AudioClip[] sounds = { chestOpen, chestClose, itemPickup };
        audioSource.PlayOneShot(sounds[index]);
    }

    void GridLayout3D()
    {
        Vector3 spacing = new Vector3(1f, -1f, 1f);  // Spacing between slots (X, Y, Z)
        int columns = 5;  // Number of slots per row
        for (int i = 0; i < slots.Length; i++)
        {
            int row = i / columns;
            int column = i % columns;
            slots[i].localPosition = new Vector3(column * spacing.x, row * spacing.y, 0);  // Assign the position to the slot
        }
    }

    public void AddItem(Item item)
    {
        if (currentItems >= maxItems) { print("Inventory is full"); return; }

        item.AddOrRemove(true);
        StartCoroutine(SendInventoryRequest(item.scriptableItem.id.ToString(), "add"));

        PlaySound(2);
        currentItems += 1;
        weightCount += item.scriptableItem.weight;
        itemCountText.text = currentItems + " / " + maxItems;
        weightCountText.text = weightCount.ToString();

        foreach (Transform slot in slots)
            if (slot.childCount == 0)
            {
                Transform itemTransform = item.transform;
                itemTransform.SetParent(slot, false);  // 'false' ensures the child retains its global transform settings when changing its parent
                itemTransform.localPosition = Vector3.zero;
                itemTransform.localRotation = Quaternion.Euler(270, 0, 0);
                break;
            }
    }

    public void RemoveItem(Item item)
    {
        item.GetComponentInParent<Animator>().Play(DropHash);  // Do the dropping animation in inventory, before actually dropping it

        this.Wait(0.3f, () =>
        {
            item.AddOrRemove(false);
            StartCoroutine(SendInventoryRequest(item.scriptableItem.id.ToString(), "remove"));

            currentItems -= 1;
            weightCount -= item.scriptableItem.weight;
            itemCountText.text = currentItems + " / " + maxItems;
            weightCountText.text = weightCount.ToString();

            item.transform.SetParent(null, false);  // 'false' ensures the child retains its global transform settings when changing its parent
            item.transform.position = transform.position + Vector3.up + Vector3.forward;  // Teleport up and forward when dropping the item
            SortSlots();  // Reorganize slots to fill the empty ones
        });
    }

    IEnumerator SendInventoryRequest(string itemId, string action)  // Coroutine to send the POST request
    {
        string url = "https://wadahub.manerai.com/api/inventory/status";
        string authToken = "Bearer kPERnYcWAY46xaSy8CEzanosAgsWM84Nx7SKM4QBSqPq6c7StWfGxzhxPfDh8MaP";

        string jsonData = "{\"item_id\":\"" + itemId + "\", \"action\":\"" + action + "\"}";  // Create the data to send in the POST request

        // Create the UnityWebRequest with JSON data
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", authToken);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();  // Send the request and wait for a response

        // Check for errors
        if (request.result == UnityWebRequest.Result.Success) Debug.Log("Request sent successfully: " + request.downloadHandler.text);
        else Debug.LogError("Error sending request: " + request.error);
    }

    void SortSlots()
    {
        for (int i = 0; i < slots.Length - 1; i++)  // Loop through all slots
            if (slots[i].childCount == 0)  // If the current slot is empty, find the next occupied slot and move the item from the occupied slot to the empty slot
                for (int j = i + 1; j < slots.Length; j++)
                    if (slots[j].childCount > 0)
                    {
                        Transform child = slots[j].GetChild(0);
                        child.SetParent(slots[i], false);
                        child.localPosition = Vector3.zero;  // Reset position (child's position = parent's position)
                        break;
                    }
    }
}