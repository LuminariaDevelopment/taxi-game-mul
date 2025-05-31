// TaxiSpawner.cs
using UnityEngine;
using Unity.Netcode;

public class TaxiSpawner : MonoBehaviour
{
    [Tooltip("Drag your Taxi prefab here (the same prefab that has NetworkObject & NetworkTransform).")]
    public GameObject taxiPrefab;

    [Tooltip("One or more Transforms to pick spawn positions from.")]
    public Transform[] spawnPoints;

    private void Start()
    {
        // Make sure a NetworkManager exists in this scene:
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("TaxiSpawner: No NetworkManager.Singleton found! Please add a NetworkManager to the scene.");
            return;
        }

        // Prevent Netcode from auto-spawning its PlayerPrefab on connect:
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = null;

        // Register our callback for when a client joins:
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnTaxiForClient;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnTaxiForClient;
        }
    }

    private void SpawnTaxiForClient(ulong clientId)
    {
        // Only the server runs this logic:
        if (!NetworkManager.Singleton.IsServer)
            return;

        // Pick a random spawn point (or default to the world origin if none):
        Transform chosenSpawn = null;
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            chosenSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        Vector3 spawnPosition = chosenSpawn != null ? chosenSpawn.position : Vector3.zero;
        Quaternion spawnRotation = chosenSpawn != null ? chosenSpawn.rotation : Quaternion.identity;

        // Make sure taxiPrefab is assigned:
        if (taxiPrefab == null)
        {
            Debug.LogError("TaxiSpawner: taxiPrefab is not assigned!");
            return;
        }

        // Instantiate the prefab:
        GameObject taxiInstance = Instantiate(taxiPrefab, spawnPosition, spawnRotation);
        NetworkObject netObj = taxiInstance.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("TaxiSpawner: taxiPrefab must have a NetworkObject component!");
            Destroy(taxiInstance);
            return;
        }

        // This hands ownership of this taxi to the connected client:
        netObj.SpawnAsPlayerObject(clientId);
    }
}
