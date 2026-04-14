using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float cooldown = 0.4f;
    [SerializeField] private int maxAmmo = 10;

    private float _lastShotTime;
    private int _currentAmmo;

    // IsAlive и связь с PlayerNetwork настраивается студентом
    private PlayerNetwork _player;

    private Action _control;
    
    private void Awake()
    {
        _control = new Action();
        _control.Enable();
        _control.Player.Shoot.started += ctx => ShootServerRpc(firePoint.position, firePoint.forward);

    }

    public override void OnNetworkSpawn()
    {
        _currentAmmo = maxAmmo;
        _player = GetComponent<PlayerNetwork>();
    }
    

    [ServerRpc]
    private void ShootServerRpc(Vector3 pos, Vector3 dir, ServerRpcParams rpc = default)
    {
        if (!IsOwner || !_player.IsAlive.Value || _currentAmmo <= 0 || Time.time < _lastShotTime + cooldown) return;

        _lastShotTime = Time.time;
        _currentAmmo--;

        var go = Instantiate(projectilePrefab, pos + dir * 1.2f, Quaternion.LookRotation(dir));
        var no = go.GetComponent<NetworkObject>();
        no.SpawnWithOwnership(rpc.Receive.SenderClientId);
    }
}
