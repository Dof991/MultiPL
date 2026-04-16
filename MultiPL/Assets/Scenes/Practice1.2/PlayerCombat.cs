using Unity.Netcode;
using UnityEngine;

public class PlayerCombat : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork player;
    [SerializeField] private int damage = 10;
    private PlayerNetwork _target;
    private ActionMaps _control;
    [SerializeField] private Rigidbody rb;

    
    private void Awake()
    {
        _control = new ActionMaps();
        _control.Enable();
        _control.Player.Attack.started += ctx => TryAttack();

    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collis");
        if (!IsServer) return;
        _target = other.gameObject.GetComponent<PlayerNetwork>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;
        _target = null;
    }
    

    public void TryAttack()
    {
        
        Debug.Log(_target);

        // Атаку инициирует только локальный владелец объекта.
        if (!IsOwner || _target == null || !player.IsAlive.Value)
            return;
        DealDamageServerRpc(_target.NetworkObjectId, damage);
    }

    [ServerRpc]
    private void DealDamageServerRpc(ulong targetObjectId, int inputDamage)
    {
        Debug.Log(targetObjectId);
        
        // Сервер проверяет, существует ли цель среди заспавненных сетевых объектов.
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(targetObjectId, out NetworkObject targetObject))
            return;
        

        // Запрещаем урон самому себе и удары по некорректной цели.
        if (_target == null || _target == player)
            return;

        // Итоговое значение HP ограничиваем снизу нулем.
        int nextHp = Mathf.Max(0, _target.HP.Value - inputDamage);
        _target.HP.Value = nextHp;

        // Jump();
    }

    private void OnDisable()
    {
        _control.Disable();
    }
}