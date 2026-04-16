using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float _speed = 15f;
    [SerializeField] private int _damage = 25;
    [SerializeField] private float _lifeTime = 3f;

    private void Start()
    {
        if (IsServer) Destroy(gameObject, _lifeTime); 
    }

    void Update()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent(out PlayerNetwork target))
        {
            if (target.OwnerClientId == OwnerClientId) return;

            if (target.IsAlive.Value)
            {
                target.HP.Value = Mathf.Max(0, target.HP.Value - _damage);
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}