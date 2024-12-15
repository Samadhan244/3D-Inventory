using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] ItemPrefabs;
    [SerializeField] int droppedItemsCount, maxItemsOnSceneToDrop = 25;
    [SerializeField] float delaySpawn;

    // Update is called once per frame
    void Update()
    {
        delaySpawn -= Time.deltaTime;
        if (droppedItemsCount >= maxItemsOnSceneToDrop) return;

        if (delaySpawn <= 0)
        {
            //delaySpawn = Random.Range(3f, 6f);  // Random time of second item spawning
            delaySpawn = 2f;
            Vector3 randomPosition = new Vector3(Random.Range(-6f,6f), Random.Range(3f,7f), Random.Range(-6f,6f));
            Instantiate(ItemPrefabs[Random.Range(0, ItemPrefabs.Length)], transform.position + randomPosition, Quaternion.identity);
            droppedItemsCount += 1;
        }
    }
}