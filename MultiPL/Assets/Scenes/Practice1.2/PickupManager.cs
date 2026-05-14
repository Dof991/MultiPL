using UnityEngine;
using System.Collections;
using FishNet;
using FishNet.Managing.Server;
using FishNet.Transporting;
using FishNet.Object;

public class PickupManager : MonoBehaviour
{
    [SerializeField] private GameObject _healthPickupPrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _respawnDelay = 10f;

    private bool _isInitialized = false;

    private void Start()
    {
        if (InstanceFinder.NetworkManager != null)
        {
            InstanceFinder.ServerManager.OnServerConnectionState += OnServerState;

            if (InstanceFinder.ServerManager.Started)
                OnServerReady();
        }
        else
        {
            Debug.LogError("[PickupManager] NetworkManager not found!");
        }
    }

    private void OnServerState(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
            OnServerReady();
    }

    private void OnServerReady()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;

        Debug.Log("[PickupManager] Server started, spawning pickups...");
        SpawnAll();
    }

    private void SpawnAll()
    {
        foreach (Transform point in _spawnPoints)
        {
            if (point != null)
                SpawnPickup(point.position);
        }
    }

    public void OnPickedUp(Vector3 position)
    {
        if (!InstanceFinder.IsServer)
            return;

        StartCoroutine(RespawnAfterDelay(position));
    }

    private IEnumerator RespawnAfterDelay(Vector3 position)
    {
        yield return new WaitForSeconds(_respawnDelay);

        if (InstanceFinder.IsServer)
            SpawnPickup(position);
    }

    private void SpawnPickup(Vector3 position)
    {
        if (!InstanceFinder.IsServer)
        {
            Debug.LogError("[PickupManager] Cannot spawn - not server");
            return;
        }

        GameObject go = Instantiate(_healthPickupPrefab, position, Quaternion.identity);

        var pickup = go.GetComponent<HealthPickup>();
        var netObj = go.GetComponent<NetworkObject>();

        if (pickup != null && netObj != null)
        {
            pickup.Init(this);

            InstanceFinder.ServerManager.Spawn(netObj);
        }
        else
        {
            Debug.LogError("[PickupManager] Missing components");
            Destroy(go);
        }
    }

    private void OnDestroy()
    {
        if (InstanceFinder.ServerManager != null)
        {
            InstanceFinder.ServerManager.OnServerConnectionState -= OnServerState;
        }
    }
}