using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : NetworkBehaviour
{
    public readonly SyncVar<int> Ammo = new SyncVar<int>();
    
    [SerializeField] private int _maxAmmo = 30;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _cooldown = 0.5f;

    private float _lastShotTime;
    private PlayerNetwork _playerNetwork;

    public override void OnStartNetwork()
    {
        _playerNetwork = GetComponent<PlayerNetwork>();
        if (IsServerInitialized) Ammo.Value = _maxAmmo; 
    }

    private void Update()
    {
        if (!IsOwner || !_playerNetwork.IsAlive.Value) return;

        bool isShooting = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;

        if (isShooting && Ammo.Value > 0)
        {
            Shoot(_firePoint.position, transform.forward);
        }
    }

    [ServerRpc]
    private void Shoot(Vector3 pos, Vector3 dir)
    {
        if (!_playerNetwork.IsAlive.Value || Ammo.Value <= 0 || Time.time < _lastShotTime + _cooldown) return;

        _lastShotTime = Time.time;
        Ammo.Value--; 

        GameObject go = Instantiate(_projectilePrefab, pos, Quaternion.LookRotation(dir));
        var no = go.GetComponent<NetworkObject>();
        ServerManager.Spawn(go, Owner);
        
    }
}