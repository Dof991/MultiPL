using TMPro;
using UnityEngine;
using FishNet.Object;

public class PlayerView : NetworkBehaviour
{
    private PlayerNetwork _playerNetwork;

    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private TMP_Text _hpText;

    private string _lastNick;
    private int _lastHp;

    private void Awake()
    {
        _playerNetwork = GetComponent<PlayerNetwork>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        RefreshUI();
    }

    private void Update()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (_playerNetwork == null)
            return;

        if (_playerNetwork.Nickname.Value != _lastNick)
        {
            _lastNick = _playerNetwork.Nickname.Value;
            _nicknameText.text = _lastNick;
        }

        if (_playerNetwork.HP.Value != _lastHp)
        {
            _lastHp = _playerNetwork.HP.Value;
            _hpText.text = $"HP: {_lastHp}";
        }
    }
}