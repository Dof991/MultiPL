using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float cooldown = 0.4f;
    [SerializeField] private int maxAmmo = 10;

    private float _lastShotTime;
    private int _currentAmmo;

    private PlayerNetwork _player;
    private ActionMaps _control;

    private void Awake()
    {
        _control = new ActionMaps();
    }

    public override void OnNetworkSpawn()
    {
        _currentAmmo = maxAmmo;
        _player = GetComponent<PlayerNetwork>();

        
        if (!IsOwner)
        {
            _control.Disable();
            return;
        }

        _control.Enable();

        // Подписка на стрельбу
        _control.Player.Shoot.started += OnShoot;
    }

    private void OnDestroy()
    {
        if (_control != null)
        {
            _control.Player.Shoot.started -= OnShoot;
        }
    }

    private void OnShoot(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        
        if (!IsOwner) return;

        // Можно добавить клиентскую проверку кулдауна (чтобы не спамить RPC)
        if (Time.time < _lastShotTime + cooldown) return;

        ShootServerRpc(firePoint.position, firePoint.forward);
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 pos, Vector3 dir, ServerRpcParams rpc = default)
    {
        

        if (_player == null || !_player.IsAlive.Value) return;
        if (_currentAmmo <= 0) return;
        if (Time.time < _lastShotTime + cooldown) return;

        _lastShotTime = Time.time;
        _currentAmmo--;

        var go = Instantiate(projectilePrefab, pos + dir * 1.2f, Quaternion.LookRotation(dir));

        var no = go.GetComponent<NetworkObject>();
        no.SpawnWithOwnership(rpc.Receive.SenderClientId);
    }
}