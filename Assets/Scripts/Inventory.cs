using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }
    [SerializeField] Text itemCountText, weightCountText;
    [SerializeField] GameObject inventoryInterface, itemsInside;
    [SerializeField] Transform[] slots;
    [SerializeField] Animator animator;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip chestOpen, chestClose, itemPickup;
    public bool isOpen, isOnCd;
    [SerializeField] int itemsCount, maxItemsCount = 20;
    [SerializeField] float weightCount;
    private static readonly int IsOpenHash = Animator.StringToHash("IsOpen");  // Performance-friendly

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
    }

    public void OpenOrCloseInventory()  // This method is called by pressing a hotkey or left-clicking the 3d inventory
    {
        // Opening or closing inventory has a cooldown of 1 second
        if (isOnCd) return;
        else { isOnCd = true; this.Wait(1f, () => isOnCd = false); }

        // Open if closed, close if open
        isOpen = !isOpen;
        animator.SetBool(IsOpenHash, isOpen);
        if (isOpen)
        {
            itemsInside.SetActive(true);
            this.Wait(1f, () => inventoryInterface.SetActive(true));
        }
        else
        {
            inventoryInterface.SetActive(false);
            this.Wait(1f, () => itemsInside.SetActive(false));  // Disable items when the chest is closed
        }
    }

    public void PlaySound(int index)
    {
        AudioClip[] sounds = { chestOpen, chestClose, itemPickup };
        audioSource.PlayOneShot(sounds[index]);
    }

    void GridLayout3D()
    {
        Vector3 spacing = new Vector3(0.1f, 0.1f, 0.1f);  // Spacing between slots (X, Y, Z)
        int columns = 5;  // Number of slots per row
        for (int i = 0; i < itemsInside.transform.childCount; i++)
        {
            int row = i / columns;
            int column = i % columns;
            slots[i].localPosition = new Vector3(column * spacing.x, 0, row * spacing.z);  // Assign the position to the slot
        }
    }

    public void AddItem(Item item)
    {
        if (itemsCount >= maxItemsCount) { print("Inventory is full"); return; }

        StartCoroutine(SendInventoryRequest(item.ScriptableItem.id.ToString(), "add"));
        item.isInsideInventory = true;
        itemsCount += 1;
        weightCount += item.ScriptableItem.weight;
        itemCountText.text = itemsCount + " / " + maxItemsCount;
        weightCountText.text = weightCount.ToString();

        foreach (Transform slot in slots)
            if (slot.childCount == 0)
            {
                Transform itemTransform = item.transform;
                itemTransform.SetParent(slot);
                itemTransform.localPosition = Vector3.zero;
                itemTransform.rotation = Quaternion.Euler(item.ScriptableItem.xRotation, 0, 0);
                itemTransform.localScale = new Vector3(item.ScriptableItem.scale * 3, item.ScriptableItem.scale * 3, item.ScriptableItem.scale * 3);
                break;
            }
    }

    private IEnumerator SendInventoryRequest(string itemId, string action)  // Coroutine to send the POST request
    {
        string url = "https://wadahub.manerai.com/api/inventory/status";
        string authToken = "Bearer kPERnYcWAY46xaSy8CEzanosAgsWM84Nx7SKM4QBSqPq6c7StWfGxzhxPfDh8MaP";

        // Create the data to send in the POST request
        string jsonData = "{\"item_id\":\"" + itemId + "\", \"action\":\"" + action + "\"}";

        // Create the UnityWebRequest with JSON data
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", authToken);
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request and wait for a response
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result == UnityWebRequest.Result.Success) Debug.Log("Request sent successfully: " + request.downloadHandler.text);
        else Debug.LogError("Error sending request: " + request.error);
    }
}