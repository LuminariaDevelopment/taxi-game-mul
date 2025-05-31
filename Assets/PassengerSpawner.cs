using UnityEngine;
using System.Collections.Generic;

public class PassengerSpawner : MonoBehaviour
{
    [System.Serializable]
    public class PassengerSpawnData
    {
        public GameObject prefab;
        [Range(0f, 1f)] public float spawnWeight;
    }

    [System.Serializable]
    public class SpawnArea
    {
        public BoxCollider area;
        public int passengerLimit;
        public List<PassengerSpawnData> passengerSpawnOptions;
    }

    [Header("Spawn Areas")]
    public List<SpawnArea> spawnAreas;

    void Start()
    {
        foreach (var spawnArea in spawnAreas)
        {
            SpawnPassengersInArea(spawnArea);
        }
    }

    void SpawnPassengersInArea(SpawnArea spawnArea)
    {
        for (int i = 0; i < spawnArea.passengerLimit; i++)
        {
            Vector3 spawnPos = GetRandomPositionOnGround(spawnArea.area);
            GameObject prefab = GetWeightedRandomPassengerPrefab(spawnArea.passengerSpawnOptions);
            Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Instantiate(prefab, spawnPos, randomRotation);
        }
    }

    Vector3 GetRandomPositionOnGround(BoxCollider area)
    {
        Vector3 center = area.center + area.transform.position;
        Vector3 size = area.size;

        float x = Random.Range(-size.x / 2f, size.x / 2f);
        float z = Random.Range(-size.z / 2f, size.z / 2f);
        Vector3 rayOrigin = center + new Vector3(x, 0f, z);

        Ray ray = new Ray(rayOrigin, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 20f))
        {
            return hitInfo.point;
        }

        return center;
    }

    GameObject GetWeightedRandomPassengerPrefab(List<PassengerSpawnData> spawnOptions)
    {
        float totalWeight = 0f;
        foreach (var option in spawnOptions)
        {
            totalWeight += option.spawnWeight;
        }

        float randomValue = Random.value * totalWeight;
        float currentWeight = 0f;

        foreach (var option in spawnOptions)
        {
            currentWeight += option.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return option.prefab;
            }
        }

        return spawnOptions[0].prefab; // Fallback
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        foreach (var spawnArea in spawnAreas)
        {
            if (spawnArea.area != null)
            {
                Gizmos.DrawWireCube(spawnArea.area.center + spawnArea.area.transform.position, spawnArea.area.size);
            }
        }
    }
}
