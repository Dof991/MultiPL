using Unity.Collections;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    // Ник должен быть виден всем клиентам, но менять его может только сервер.
    public NetworkVariable<FixedString32Bytes> Nickname = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // HP тоже читает каждый клиент, но изменяется только на сервере.
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

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        // Сервер нормализует ник и записывает итоговое значение в NetworkVariable.
        string safeValue = string.IsNullOrWhiteSpace(nickname) ? $"Player_{OwnerClientId}" : nickname.Trim();
        Nickname.Value = safeValue;
    }
    
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject playerBody;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Только владелец отправляет на сервер свой локально введенный ник.
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
        
        HP.OnValueChanged += OnHpChanged;
        // StartCoroutine(Death());
    }

    public override void OnNetworkDespawn()
    {
        HP.OnValueChanged -= OnHpChanged;
    }

    private void OnHpChanged(int prev, int next)
    {
        // Только сервер запускает цикл смерти
        if (!IsServer) return;
        if (next <= 0 && IsAlive.Value)
        {
            IsAlive.Value = false;
            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        playerBody.SetActive(false);
        yield return new WaitForSeconds(3f);

        // Выбрать случайную точку респавна
        // int idx = Random.Range(0, spawnPoints.Length);
        transform.position = Vector3.zero;

        HP.Value = 100;
        IsAlive.Value = true;
        playerBody.SetActive(true);
    }

    private IEnumerator Death()
    {
        for (int i = 0; i < 5; i++)
        {
            HP.Value -= 20;
            yield return new WaitForSeconds(1f);
        }
    }
}