using UnityEngine;

public abstract class SingletonMB<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>
    /// The only instance of this class (might be null)
    /// </summary>
    public static T Instance { get; protected set; }

    public enum OnDuplicateBehavior { Reject, Override }
    public OnDuplicateBehavior OnDuplicate { get; set; }

    /// <summary>
    /// Registers new Instance or self-destructs if one already exists
    /// </summary>
    protected virtual void Awake()
    {
        if (typeof(T) == typeof(MonoBehaviour))
        {
            OnDestroyInstance();
        }
        else if (Instance == null)
        {
            Instance = GetComponent<T>();
        }
        else if (Instance != GetComponent<T>())
        {
            switch (OnDuplicate)
            {
                case OnDuplicateBehavior.Reject:
                    OnDestroyInstance();
                    break;
                case OnDuplicateBehavior.Override:
                    (Instance as SingletonMB<T>).OnDestroyInstance();
                    Instance = GetComponent<T>();
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Registers new Instance or self-destructs if one already exists
    /// </summary>
    protected virtual void AwakeSilently()
    {
        if (typeof(T) == typeof(MonoBehaviour))
            OnDestroyInstance();
        else if (Instance == null)
            Instance = GetComponent<T>();
        else if (Instance != GetComponent<T>())
            OnDestroyInstance();
    }

    /// <summary>
    /// Destroys the new instance
    /// </summary>
    protected virtual void OnDestroyInstance()
    {
        if (Application.isPlaying)
            Destroy(GetComponent<T>());
    }

    /// <summary>
    /// When instance is destroyed we should clear our reference to it
    /// </summary>
    protected virtual void OnDestroy()
    {
        //I'm not sure either will ever return yes, but wildly nullifying Instance would be worse
        if (Instance == GetComponent<T>() || Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Gets or Creates an instance as necessary.
    /// </summary>
    /// <returns>The only instance of this class (be it old or new)</returns>
    public static T GetOrCreateInstance() => Instance ? Instance : Instance = new GameObject(nameof(T)).AddComponent<T>();

    /// <summary>
    /// Gets or Finds an instance as necessary
    /// </summary>
    /// <returns>The only instance of this class (be it previously known or not)</returns>
    public static T GetOrFindInstance() => Instance ? Instance : Instance = FindObjectOfType<T>();

    public static bool TryGetInstance(out T t) => t = Instance;

    public static bool TryGetOrFindInstance(out T t) => t = GetOrFindInstance();
}
