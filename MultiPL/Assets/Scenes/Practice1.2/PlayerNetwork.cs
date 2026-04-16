using Unity.Collections;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> Nickname = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> HP = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> IsAlive = new(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject playerBody;

    private Vector3 _deathPosition;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }

        if (IsServer)
        {
            HP.Value = 100;
            IsAlive.Value = true;
        }

        HP.OnValueChanged += OnHpChanged;
        IsAlive.OnValueChanged += OnIsAliveChanged;

        ToggleVisual(IsAlive.Value);
    }

    public override void OnNetworkDespawn()
    {
        HP.OnValueChanged -= OnHpChanged;
        IsAlive.OnValueChanged -= OnIsAliveChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        string safeValue = string.IsNullOrWhiteSpace(nickname)
            ? $"Player_{OwnerClientId}"
            : nickname.Trim();

        Nickname.Value = safeValue;
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer) return;
        if (!IsAlive.Value) return;

        HP.Value = Mathf.Max(0, HP.Value - damage);
    }

    private void OnHpChanged(int prev, int next)
    {
        if (!IsServer) return;

        if (next <= 0 && IsAlive.Value)
        {
            _deathPosition = transform.position;

            IsAlive.Value = false;
            StartCoroutine(RespawnRoutine());
        }
    }

    private void OnIsAliveChanged(bool prev, bool next)
    {
        ToggleVisual(next);
    }

    private void ToggleVisual(bool isAlive)
    {
        if (playerBody != null)
            playerBody.SetActive(isAlive);

        var cc = GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = isAlive;
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(3f);

        Vector3 spawnPos = _deathPosition;

        HP.Value = 100;
        IsAlive.Value = true;

        RespawnClientRpc(spawnPos);
    }

    [ClientRpc]
    private void RespawnClientRpc(Vector3 newPos)
    {
        var cc = GetComponent<CharacterController>();

        if (cc != null) cc.enabled = false;

        transform.position = newPos;

        Physics.SyncTransforms();

        if (cc != null) cc.enabled = true;
    }
}