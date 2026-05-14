using UnityEngine;
using FishNet.Object;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float speed = 18f;
    [SerializeField] private int damage = 20;
    [SerializeField] private float lifetime = 5f;

    private float _spawnTime;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _spawnTime = Time.time;
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (IsServerInitialized && Time.time > _spawnTime + lifetime)
        {
            Despawn(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized)
            return;

        if (!IsSpawned)
            return;

        PlayerNetwork target = other.GetComponent<PlayerNetwork>();
        if (target == null)
            return;

        if (target.Owner.ClientId == Owner.ClientId)
            return;

        target.TakeDamage(damage);

        Despawn(gameObject);
    }
}