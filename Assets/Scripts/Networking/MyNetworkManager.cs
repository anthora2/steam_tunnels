using Mirror;
using UnityEngine;

public class MyNetworkManager : NetworkManager
{
    [Header("Spawn Settings")]
    public GameObject biblePrefab;
    public GameObject crossPrefab;
    public int numberOfItems = 5;
    
    [Header("Spawn Position")]
    public Vector3 planeCenter = new Vector3(92.99f, 0.24f, 456.6f); // Plane center position
    public Vector3 spawnAreaSize = new Vector3(20f, 0f, 20f); // Spawn area around plane center
    public float spawnHeightOffset = 1.5f; // Height above plane surface
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        
        // Spawn pickups when server starts
        SpawnInitialItems();
    }
    
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // Get a random spawn position on the plane
        Vector3 spawnPosition = GetRandomSpawnPosition();
        Quaternion spawnRotation = Quaternion.identity;
        
        // Instantiate player at the spawn position
        GameObject player = Instantiate(playerPrefab, spawnPosition, spawnRotation);
        
        // Name it for debugging
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        
        // Add player to connection
        NetworkServer.AddPlayerForConnection(conn, player);
    }
    
    void SpawnInitialItems()
    {
        for (int i = 0; i < numberOfItems; i++)
        {
            Vector3 position = GetRandomSpawnPosition();
            
            GameObject item = Instantiate(biblePrefab, position, Quaternion.identity);
            NetworkServer.Spawn(item);
            Debug.Log($"Spawned item at {position}");
        }

        for (int i = 0; i < numberOfItems; i++)
        {
            Vector3 position = GetRandomSpawnPosition();
            
            GameObject item = Instantiate(crossPrefab, position, Quaternion.identity);
            NetworkServer.Spawn(item);
            Debug.Log($"Spawned item at {position}");
        }
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        // Calculate random position within spawn area, centered on the plane
        float x = planeCenter.x + Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
        float y = planeCenter.y + spawnHeightOffset; // Spawn above the plane
        float z = planeCenter.z + Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f);
        
        return new Vector3(x, y, z);
    }
}