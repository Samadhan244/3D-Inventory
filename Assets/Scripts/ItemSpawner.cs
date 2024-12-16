using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    GlobalReferences global => GlobalReferences.Instance;
    [SerializeField] GameObject[] ItemPrefabs;
    [SerializeField] float spawnCD;

    // Update is called once per frame
    void Update()
    {
        spawnCD -= Time.deltaTime;
        if (global.droppedItemsCount >= global.maxItemsOnSceneToDrop) return;

        if (spawnCD <= 0)
        {
            spawnCD = Random.Range(2f, 5f);  // Random time of second item spawning
            Vector3 randomPosition = new Vector3(Random.Range(-6f,6f), Random.Range(3f,7f), Random.Range(-6f,6f));
            Instantiate(ItemPrefabs[Random.Range(0, ItemPrefabs.Length)], transform.position + randomPosition, Quaternion.identity);
            global.droppedItemsCount += 1;
        }
    }
}