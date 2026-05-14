using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Component.Transforming;

[RequireComponent(typeof(CharacterController))]
public class PlayerNetwork : NetworkBehaviour
{
    [Header("Player Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private GameObject characterModel;

    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 5f;

    // Сетевые переменные
    public readonly SyncVar<bool> IsAlive = new SyncVar<bool>(true);
    public readonly SyncVar<int> HP = new SyncVar<int>(100);
    public readonly SyncVar<string> Nickname = new SyncVar<string>("Player");

    private bool _isRespawning;
    private PlayerMovement _movement;

    private void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        _movement = GetComponent<PlayerMovement>();
    }
        public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
    }
        
    [ServerRpc(RequireOwnership = false)]
    public void SubmitNicknameServerRpc(string nickname)
    {
        int id = Owner != null ? Owner.ClientId : -1;

        Nickname.Value = string.IsNullOrWhiteSpace(nickname)
            ? $"Player_{id}"
            : nickname.Trim();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        IsAlive.Value = true;
        HP.Value = 100;
    }
    
    [Server]
    public void TakeDamage(int damage)
    {
        if (!IsAlive.Value)
            return;

        HP.Value -= damage;

        if (HP.Value <= 0)
        {
            HP.Value = 0;
            Die();
        }
    }

    [Server]
    private void Die()
    {
        if (_isRespawning)
            return;

        IsAlive.Value = false;
        _isRespawning = true;

        RpcHandleDeath();

        StartCoroutine(RespawnCoroutine());
    }

    [ObserversRpc]
    private void RpcHandleDeath()
    {

        characterController.enabled = false;
        characterModel.SetActive(false);
    }

    [Server]
    private IEnumerator RespawnCoroutine()
    {
        float timer = respawnDelay;

        while (timer > 0f)
        {
            RpcRespawnCountdown(Mathf.CeilToInt(timer));
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }

        RespawnPlayer();
    }

    [ObserversRpc]
    private void RpcRespawnCountdown(int secondsLeft)
    {
        Debug.Log($"Respawn через {secondsLeft}...");
    }

   [Server]
private void RespawnPlayer()
{
    Transform spawnPoint = GetRandomRespawnPoint();
    if (spawnPoint == null)
        return;

    CharacterController cc = characterController;

    if (cc != null)
        cc.enabled = false;

    transform.SetPositionAndRotation(
        spawnPoint.position,
        spawnPoint.rotation
    );

    HP.Value = 100;
    IsAlive.Value = true;

    RpcHandleRespawn(
        spawnPoint.position,
        spawnPoint.rotation
    );

    _isRespawning = false;
}
[ObserversRpc]
private void RpcHandleRespawn(Vector3 position, Quaternion rotation)
{
    if (characterController != null)
        characterController.enabled = false;

    transform.SetPositionAndRotation(position, rotation);


    if (characterModel != null)
        characterModel.SetActive(true);

    if (characterController != null)
        characterController.enabled = true;
}

    [Server]
    private Transform GetRandomRespawnPoint()
    {
        PlayerSpawnPoint [] points = FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None);

        if (points == null || points.Length == 0)
        {
            Debug.LogWarning("Точки респавна не найдены!");
            return null;
        }

        int index = Random.Range(0, points.Length);
        return points[index].transform;
    }
    
    [ServerRpc]
    public void SetNickname(string newNickname)
    {
        Nickname.Value = newNickname;
    }
}