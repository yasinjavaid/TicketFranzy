using System.Collections.Generic;
using System.Linq;

using Cinemachine;

using MoreLinq;

using UnityEngine;
using UnityEngine.Playables;

public class CameraSystem : SingletonMB<CameraSystem>
{
    [SerializeField] protected CinemachineBrain brain;
    [SerializeField] protected List<CinemachineVirtualCamera> virtualCameras = new List<CinemachineVirtualCamera>();
    [SerializeField] protected List<PlayableDirector> directors = new List<PlayableDirector>();


    public Camera GetCamera => brain ? brain.OutputCamera : null;


    protected ICinemachineCamera ActiveVirtualCamera { get; set; }


    protected PlayableDirector playingDirector { get; set; }

    public float RemainingTransitionTime => (float)(playingDirector && playingDirector.state == PlayState.Playing ? playingDirector.duration - playingDirector.time : 0);

    public IEnumerable<string> GetVirtualCameraNames() => virtualCameras.Select(vc => vc.gameObject.name);
    public bool ContainsVirtualCamera(string name) => virtualCameras.Any(vc => vc.gameObject.name == name);

    public float FOV
    {
        get => (ActiveVirtualCamera as CinemachineVirtualCamera).m_Lens.FieldOfView;
        set => (ActiveVirtualCamera as CinemachineVirtualCamera).m_Lens.FieldOfView = value;
    }

    protected override void Awake()
    {
        base.Awake();
        ActiveVirtualCamera = virtualCameras.FirstOrDefault(vc => vc.gameObject.activeInHierarchy);
    }

    public virtual string GetVirtualCameraName() => (ActiveVirtualCamera != null && ActiveVirtualCamera?.VirtualCameraGameObject != null) ? ActiveVirtualCamera.VirtualCameraGameObject.name : null;

    /// <summary>
    /// Changes to a different virtual camera
    /// </summary>
    /// <param name="name">The name of the virtual camera to activate</param>
    public virtual void SetVirtualCamera(string name)
    {
        if (!virtualCameras.Any(vc => vc.gameObject.name == name)) return;
        if (playingDirector) playingDirector.Stop();
        virtualCameras.ForEach(vc => vc.gameObject.SetActive(vc.gameObject.name == name));
        ActiveVirtualCamera = virtualCameras.FirstOrDefault(vc => vc.gameObject.activeInHierarchy);
    }
    /// <summary>
    /// Changes to a different virtual camera
    /// </summary>
    /// <param name="name">The name of the virtual camera to activate</param>
    /// <returns>True if the camera exists, false otherwise</returns>
    public virtual bool TrySetVirtualCamera(string name)
    {
        if (!virtualCameras.Any(vc => vc.gameObject.name == name)) return false;
        if (playingDirector) playingDirector.Stop();
        virtualCameras.ForEach(vc => vc.gameObject.SetActive(vc.gameObject.name == name));
        ActiveVirtualCamera = virtualCameras.FirstOrDefault(vc => vc.gameObject.activeInHierarchy);
        return true;
    }

    public virtual void SetFollowTarget(Component component) => this.InvokeDelayed(0.01f, () => ActiveVirtualCamera.Follow = component ? component.transform : null);
    public virtual void SetFollowTarget(Transform transform) => this.InvokeDelayed(0.01f, () => ActiveVirtualCamera.Follow = transform);
    public virtual void SetLookAtTarget(Component component) => this.InvokeDelayed(0.01f, () => ActiveVirtualCamera.LookAt = component ? component.transform : null);
    public virtual void SetLookAtTarget(Transform transform) => this.InvokeDelayed(0.01f, () => ActiveVirtualCamera.LookAt = transform);

    public static bool TrySetFollowTarget(Component component)
    { if (Instance) Instance.SetFollowTarget(component); return Instance; }
    public static bool TrySetFollowTarget(Transform transform)
    { if (Instance) Instance.SetFollowTarget(transform); return Instance; }
    public static bool TrySetLookAtTarget(Component component)
    { if (Instance) Instance.SetLookAtTarget(component); return Instance; }
    public static bool TrySetLookAtTarget(Transform transform)
    { if (Instance) Instance.SetLookAtTarget(transform); return Instance; }

    public virtual float PlayTransition(string name)
    {
        if (playingDirector) playingDirector.Stop();
        playingDirector = directors.Where(d => d.name.StartsWith(name))?.RandomSubset(1).FirstOrDefault();
        if (playingDirector)
        {
            playingDirector.Play();
            return (float)playingDirector.duration;
        }
        return 0;
    }
}