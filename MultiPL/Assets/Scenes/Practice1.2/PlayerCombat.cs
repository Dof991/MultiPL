using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerCombat : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork player;
    [SerializeField] private int damage = 10;
    private PlayerNetwork _target;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized)
            return;

        if (!IsSpawned)
            return;

        _target = other.GetComponent<PlayerNetwork>();
        Debug.Log(_target.name);


        if (_target == null)
            return;
        // Не стреляем в самого себя
        if (_target.Owner.ClientId == Owner.ClientId)
            return;

        _target.TakeDamage(damage);
    }

    private void OnTriggerExit(Collider other)
    {
        _target = null;
    }



}