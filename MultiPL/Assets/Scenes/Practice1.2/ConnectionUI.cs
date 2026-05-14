using FishNet;
using FishNet.Transporting;
using TMPro;
using UnityEngine;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private GameObject _menuPanel;

    public static string PlayerNickname { get; private set; } = "Player";

    private void Start()
    {
        if (InstanceFinder.NetworkManager != null)
        {
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
        }

        ShowMenu();
    }

    private void OnDestroy()
    {
        if (InstanceFinder.NetworkManager != null)
        {
            InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        }
    }

    public void StartAsHost()
    {
        SaveNickname();

        InstanceFinder.ServerManager.StartConnection();
        InstanceFinder.ClientManager.StartConnection();
    }

    public void StartAsClient()
    {
        SaveNickname();
        InstanceFinder.ClientManager.StartConnection();
    }

    private void SaveNickname()
    {
        string rawValue = _nicknameInput != null ? _nicknameInput.text : string.Empty;
        PlayerNickname = string.IsNullOrWhiteSpace(rawValue) ? "Player" : rawValue.Trim();
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        switch (args.ConnectionState)
        {
            case LocalConnectionState.Started:
                HideMenu();
                Debug.Log($"[ConnectionUI] Connected as {PlayerNickname}, hiding menu");
                break;

            case LocalConnectionState.Stopped:
                ShowMenu();
                Debug.Log("[ConnectionUI] Disconnected, showing menu");
                break;
        }
    }

    private void HideMenu()
    {
        if (_menuPanel != null)
            _menuPanel.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    private void ShowMenu()
    {
        if (_menuPanel != null)
            _menuPanel.SetActive(true);
        else
            gameObject.SetActive(true);
    }
}