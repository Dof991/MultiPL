using UnityEngine;
using FishNet.Object;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private Vector3 _offset = new(0f, 8f, -6f);

    private Camera _cam;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        _cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (!enabled || _cam == null)
            return;

        _cam.transform.position = transform.position + _offset;
        _cam.transform.LookAt(transform.position);
    }
}