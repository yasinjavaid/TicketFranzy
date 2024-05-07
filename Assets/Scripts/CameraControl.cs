using Cinemachine;

using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControl : SingletonMB<CameraControl>
{
    [SerializeField] Vector3 zoomInMax;
    [SerializeField] Vector3 zoomOutMax;
    [SerializeField] float zoomInputMultiplier;
    [SerializeField] float zoomMaxSpeed;
    [SerializeField] float zoomSmoothTime;
    [SerializeField] CinemachineVirtualCamera virtualCamera;

    CinemachineTransposer cameraTransposer;
    protected float currentZoom;
    protected float targetZoom;
    protected float currentZoomVelocity;

    private void Start()
    {
        cameraTransposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        currentZoom = Mathf.Clamp((cameraTransposer.m_FollowOffset.y - zoomInMax.y) / (zoomOutMax.y - zoomInMax.y), 0, 1);
    }

    protected virtual void OnEnable() => GameInput.Register("CameraZoom", GameInput.ReferencePriorities.Character, OnZoomInput);

    protected virtual void OnDisable() => GameInput.Deregister("CameraZoom", GameInput.ReferencePriorities.Character, OnZoomInput);

    protected virtual bool OnZoomInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            targetZoom = Mathf.Clamp(currentZoom + (ctx.ReadValue<float>() * zoomInputMultiplier), 0, 1);
        return true;
    }

    private void Update()
    {
        if (currentZoom != targetZoom)
            SetCameraZoom(currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref currentZoomVelocity, zoomSmoothTime, zoomMaxSpeed, Time.deltaTime));
    }

    protected virtual void SetCameraZoom(float zoom) => cameraTransposer.m_FollowOffset = Vector3.Lerp(zoomInMax, zoomOutMax, zoom);
}
