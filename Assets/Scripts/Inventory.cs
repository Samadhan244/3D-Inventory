using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Collections;

public class Inventory : MonoBehaviour
{
    GlobalReferences global => GlobalReferences.Instance;
    UnityEvent OnItemFolded, OnItemRetrieved; // Events triggered when an item is folded or retrieved
    [SerializeField] Text itemCountText, weightCountText;
    [SerializeField] Transform[] slots;  // Slots where items are stored
    [SerializeField] Image[] icons;  // Icons displayed when hovering over inventory
    [SerializeField] Animator animator;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip chestOpen, chestClose, itemPickup, itemDrop;
    bool isOnCd;
    [SerializeField] float weightCount;
    static readonly int IsOpenHash = Animator.StringToHash("IsOpen");

    [SerializeField] GameObject tooltip;

    void Start()
    {
        GridLayout3D();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) OpenOrCloseInventory();
    }

    public void OpenOrCloseInventory()  // Called when pressing a hotkey or clicking the 3D inventory
    {
        // Opening or closing the inventory has a cooldown of 1 second
        if (isOnCd) return;
        else { isOnCd = true; this.Wait(1f, () => isOnCd = false); }

        // Toggle inventory state
        global.isChestOpen = !global.isChestOpen;
        animator.SetBool(IsOpenHash, global.isChestOpen);
    }

    public void PlaySound(int index)
    {
        AudioClip[] sounds = { chestOpen, chestClose, itemPickup, itemDrop };
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
            slots[i].localPosition = new Vector3(column * spacing.x, row * spacing.y, 0);  // Assign position to each slot
        }
    }

    public void AddItem(Item item)
    {
        if (global.usedSlots >= global.totalSlots) { print("Inventory is full"); return; }

        StartCoroutine(SendInventoryRequest(item.scriptableItem.id.ToString(), "retrieve"));

        PlaySound(2);
        global.usedSlots += 1;
        global.droppedItemsCount -= 1;
        weightCount += item.scriptableItem.weight;
        itemCountText.text = global.usedSlots + " / " + global.totalSlots;
        weightCountText.text = weightCount.ToString();

        foreach (Transform slot in slots)
            if (slot.childCount == 0)
            {
                Transform itemTransform = item.transform;
                itemTransform.SetParent(slot, false);  // Ensure the child retains global transform settings
                itemTransform.localPosition = Vector3.zero;
                itemTransform.localRotation = Quaternion.Euler(270, 0, 0);
                break;
            }
    }

    public void RemoveItem(Item item)
    {
        StartCoroutine(SendInventoryRequest(item.scriptableItem.id.ToString(), "fold"));

        global.usedSlots -= 1;
        global.droppedItemsCount += 1;
        weightCount -= item.scriptableItem.weight;
        itemCountText.text = global.usedSlots + " / " + global.totalSlots;
        weightCountText.text = weightCount.ToString();

        item.transform.SetParent(null, false);  // Ensure the child retains global transform settings
        item.transform.position = transform.position + Vector3.up + Vector3.forward;  // Position the dropped item slightly above and forward
        SortSlots();  // Reorganize slots to fill empty spaces
    }

    IEnumerator SendInventoryRequest(string itemId, string action)  // Coroutine to send a POST request
    {
        string url = "https://wadahub.manerai.com/api/inventory/status";
        string authToken = "Bearer kPERnYcWAY46xaSy8CEzanosAgsWM84Nx7SKM4QBSqPq6c7StWfGxzhxPfDh8MaP";

        string jsonData = "{\"item_id\":\"" + itemId + "\", \"action\":\"" + action + "\"}";  // JSON data for the POST request

        // Create UnityWebRequest with JSON data
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", authToken);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();  // Send the request and wait for a response

        // Handle the response
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Request sent successfully: " + request.downloadHandler.text);
            // Trigger events based on the action
            if (action == "fold") OnItemFolded?.Invoke();
            else if (action == "retrieve") OnItemRetrieved?.Invoke();
        }
        else Debug.LogError("Error sending request: " + request.error);
    }

    void SortSlots()
    {
        for (int i = 0; i < slots.Length - 1; i++)  // Loop through all slots
            if (slots[i].childCount == 0)  // If the current slot is empty, find the next occupied slot
                for (int j = i + 1; j < slots.Length; j++)
                    if (slots[j].childCount > 0)
                    {
                        Transform child = slots[j].GetChild(0);
                        child.SetParent(slots[i], false);
                        child.localPosition = Vector3.zero;  // Reset position (child's position = parent's position)
                        break;
                    }
    }

    public void ShowUI(bool trueOrFalse)
    {
        tooltip.SetActive(trueOrFalse);
        if (trueOrFalse)
        {
            foreach (Image x in icons) x.gameObject.SetActive(false);
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].childCount > 0)
                {
                    icons[i].gameObject.SetActive(true);
                    icons[i].sprite = global.spriteAtlas.GetSprite(slots[i].GetChild(0).name.Replace("(Clone)", "").Trim());
                }
                else break;
            }
        }
    }
}