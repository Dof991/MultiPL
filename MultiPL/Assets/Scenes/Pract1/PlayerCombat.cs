using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerCombat : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _attackRange = 5f;

    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, _attackRange))
        {
            PlayerNetwork targetNetwork = hit.collider.GetComponent<PlayerNetwork>();
            
            if (targetNetwork != null)
            {
                Debug.Log($"Нашел цель: {targetNetwork.name}"); 
                TryAttack(targetNetwork);
            }
            else
            {
                Debug.Log("Луч прошел мимо");
            }
        }
    }

    public void TryAttack(PlayerNetwork target)
    {
        if (!IsOwner || target == null) return;
        Debug.Log("TryAttack");
        DealDamageServerRpc(target.NetworkObjectId, _damage);
    }

    [ServerRpc]
    private void DealDamageServerRpc(ulong targetObjectId, int damage)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(targetObjectId, out NetworkObject targetObject))
            return;

        PlayerNetwork targetPlayer = targetObject.GetComponent<PlayerNetwork>();
        
        if (targetPlayer == null || targetPlayer == _playerNetwork) return;

        int nextHp = Mathf.Max(0, targetPlayer.HP.Value - damage);
        targetPlayer.HP.Value = nextHp;
    }
}