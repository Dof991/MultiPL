using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

public struct MoveData : IReplicateData
{

    private uint _tick;

    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
    public void Dispose() { }
}

public struct ReconcileData : IReconcileData
{
    public Vector3 Position;

    private uint _tick;

    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
    public void Dispose() { }
}

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementPredicted : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController _cc;
    private float _verticalVelocity;

    private ActionMaps _control;
    private Vector2 _inputDir_XZ;
    private Vector3 _movingDir;

    private PlayerNetwork _player;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _control = new ActionMaps();
        _control.Enable();
        _control.Player.Move.started += ctx => TimeManager_OnTick();
        _cc = GetComponent<CharacterController>();
        _player = GetComponent<PlayerNetwork>();
    }


    public override void CreateReconcile()
    {
        ReconcileData rd = new ReconcileData
        {
            Position = transform.position,
        };

        Reconcile(rd);
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        TimeManager.OnTick -= TimeManager_OnTick;
        TimeManager.OnPostTick -= TimeManager_OnPostTick;
    }

    private void TimeManager_OnTick()
    {
        if (!IsOwner && !IsServerInitialized)
            return;

        MoveData md = new MoveData();

        Replicate(md);
    }

    private void TimeManager_OnPostTick()
    {
        if (IsServerInitialized)
        {
            ReconcileData rd = new ReconcileData
            {
                Position = transform.position,
            };

            Reconcile(rd);
        }
    }

    [Replicate]
    private void Replicate(
        MoveData data,
        ReplicateState state = ReplicateState.Invalid,
        Channel channel = Channel.Unreliable)
    {
        if (_player != null && !_player.IsAlive.Value)
            return;

        if (_cc == null || !_cc.enabled)
            return;

        _inputDir_XZ = _control.Player.Move.ReadValue<Vector2>();

        _movingDir = new Vector3(_inputDir_XZ.y * transform.forward.x, 0, _inputDir_XZ.y * transform.forward.z);

        Vector3 move = _movingDir.normalized * speed;

        _verticalVelocity += gravity * Time.deltaTime;
        move.y = _verticalVelocity;

        transform.eulerAngles += new Vector3(0, _inputDir_XZ.x * rotSpeed, 0);

        _cc.Move(move * Time.deltaTime);

        if (_cc.isGrounded) _verticalVelocity = 0f;
    }

    [Reconcile]
    private void Reconcile(
        ReconcileData data,
        Channel channel = Channel.Unreliable)
    {
        if (_cc != null && _cc.enabled)
        {
            _cc.enabled = false;

            transform.position = data.Position;

            _cc.enabled = true;
        }
        else
        {
            transform.position = data.Position;
        }
    }

}