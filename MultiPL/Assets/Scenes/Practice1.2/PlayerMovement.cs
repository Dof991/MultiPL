using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
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

    private void Awake()
    {
        _control = new ActionMaps();
        _control.Enable();
        _control.Player.Move.started += ctx => OnMovement();
        _cc = GetComponent<CharacterController>();
        _player = GetComponent<PlayerNetwork>();
    }

    private void OnDisable()
    {
        _control.Disable();
    }

    private void FixedUpdate()
    {
        OnMovement();
    }

    private void OnMovement()
    {
        if (IsOwner && _player.IsAlive.Value)
        {

            _inputDir_XZ = _control.Player.Move.ReadValue<Vector2>();

            _movingDir = new Vector3(_inputDir_XZ.y * transform.forward.x, 0, _inputDir_XZ.y * transform.forward.z);

            Vector3 move = _movingDir.normalized * speed;

            _verticalVelocity += gravity * Time.deltaTime;
            move.y = _verticalVelocity;

            transform.eulerAngles += new Vector3(0, _inputDir_XZ.x * rotSpeed, 0);

            _cc.Move(move * Time.deltaTime);

            if (_cc.isGrounded) _verticalVelocity = 0f;
        }
    }
}