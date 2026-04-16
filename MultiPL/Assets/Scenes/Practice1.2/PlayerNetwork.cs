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

    // -------------------- INIT --------------------

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
    }

    public override void OnNetworkDespawn()
    {
        HP.OnValueChanged -= OnHpChanged;
    }

    // -------------------- NICKNAME --------------------

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        string safeValue = string.IsNullOrWhiteSpace(nickname)
            ? $"Player_{OwnerClientId}"
            : nickname.Trim();

        Nickname.Value = safeValue;
    }

    // -------------------- DAMAGE --------------------

    public void TakeDamage(int damage)
    {
        if (!IsServer) return;
        if (!IsAlive.Value) return;

        HP.Value = Mathf.Max(0, HP.Value - damage);
    }

    // -------------------- HP CHANGE --------------------

    private void OnHpChanged(int prev, int next)
    {
        if (!IsServer) return;

        if (next <= 0 && IsAlive.Value)
        {
            IsAlive.Value = false;
            StartCoroutine(RespawnRoutine());
        }
    }

    // -------------------- RESPAWN --------------------

    private IEnumerator RespawnRoutine()
    {
        // выключаем визуал
        playerBody.SetActive(false);

        yield return new WaitForSeconds(3f);

        // выбираем точку респавна
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int idx = Random.Range(0, spawnPoints.Length);
            transform.position = spawnPoints[idx].position;
        }
        else
        {
            transform.position = Vector3.zero;
        }

        // сброс состояния
        HP.Value = 100;
        IsAlive.Value = true;

        playerBody.SetActive(true);
    }
}