using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using EasyUnityInternals;
using System.Collections;

[Serializable]
public class EasyPool : IEnumerable
{
    [SerializeField] protected List<GameObject> list;

    /// <summary>
    /// The template of GameObject from which copies will be instantiated
    /// </summary>
    public GameObject Template { get; set; }

    /// <summary>
    /// The transform that will be used as the parent of the newly instantiated objects
    /// <para>Default: null</para>
    /// </summary>
    public Transform Parent { get; set; }

    /// <summary>
    /// If set, limits the maximum amount of objects in the pool.
    /// </summary>
    public int? CountLimit { get; set; }

    /// <summary>
    /// The current count of objects in the pool.
    /// </summary>
    public int Count => list.Count;

    /// <summary>
    /// Indicates whether the pool is currently full. (only applicable if a CountLimit is set)
    /// <para>see also: <seealso cref="CountLimit"/></para>
    /// </summary>
    public bool IsFull => CountLimit.HasValue && list.Count >= CountLimit;

    /// <summary>
    /// The criteria for when a pooled object is available to be reused.
    /// <code>Default: (GameObject go) => !t.activeSelf;</code>
    /// </summary>
    public Func<GameObject, bool> UsableCriteria { get; set; } = (GameObject go) => !go.activeSelf;

    /// <summary>
    /// The action invoked on objects when they fill their <see cref="ReturnCriteria"/>.
    /// <br>This is intented to be used to return the object to the pool (by turning the condition on Usable Criteria to true).</br>
    /// <code>Default: (GameObject go) => t.SetActive(false);</code>
    /// </summary>
    public Action<GameObject> ReturnAction { get; set; } = (GameObject go) => go.SetActive(false);

    /// <summary>
    /// If set, when this criteria first evaluates to true, <see cref="ReturnAction"/> is invoked.
    /// <br>This is intented to be used to automatically return the object to the pool.</br>
    /// <code>Default: null</code>
    /// </summary>
    public Func<GameObject, bool> ReturnCriteria { get; set; }

    /// <summary>
    /// Minimum amount of seconds before the ReturnCriteria is enabled
    /// <code>Default: 0</code>
    /// </summary>
    public float ReturnMinDelay { get; set; }

    /// <summary>
    /// Creates a new EasyPool with a template and a parent
    /// </summary>
    /// <param name="template">The template of T from which copies will be instantiated</param>
    /// <param name="parent">The transform that will be used as the parent of the newly instantiated objects</param>
    public EasyPool(GameObject template, Transform parent)
    {
        list = new List<GameObject>();
        Template = template;
        Parent = parent;
    }

    /// <summary>
    /// Gets the first object in the pool that satisfies the <see cref="UsableCriteria"/>.
    /// <br>If none is found, this instantiates a new one.</br>
    /// </summary>
    /// <returns></returns>
    public GameObject Get()
    {
        list.RemoveAll(a => !a);
        GameObject go = list.FirstOrDefault(a => UsableCriteria(a));
        if (!go && !IsFull) list.Add(go = Object.Instantiate(Template, Parent));
        if (go)
        {
            go.SetActive(true);
            if (ReturnCriteria != null) ReturnWhen(go, ReturnMinDelay, ReturnCriteria);
        }
        return go;
    }

    /// <summary>
    /// Invokes the ReturnAction on the object to return it to the pool
    /// </summary>
    /// <param name="go">The object which is being returned to the pool</param>
    public void Return(GameObject go) => ReturnAction(go);

    /// <summary>
    /// Automatically returns an object to the pool when the provided criteria first evaluates to true.
    /// <br>This is automatically set when you get an object and <see cref="ReturnCriteria"/> had already been set.</br>
    /// </summary>
    /// <param name="go">The object which will be returned to the pool.</param>
    /// <param name="criteria">The criteria that evaluates when the object is ready to be returned.</param>
    public void ReturnWhen(GameObject go, Func<GameObject, bool> criteria) => ReturnWhen(go, 0, () => criteria(go));

    /// <summary>
    /// Automatically returns an object to the pool when the provided criteria first evaluates to true.
    /// <br>This is automatically set when you get an object and <see cref="ReturnCriteria"/> had already been set.</br>
    /// </summary>
    /// <param name="go">The object which will be returned to the pool.</param>
    /// <param name="minDelay">Minimum amount of seconds before the criteria is enabled</param>
    /// <param name="criteria">The criteria that evaluates when the object is ready to be returned.</param>
    public void ReturnWhen(GameObject go, float minDelay, Func<GameObject, bool> criteria) => ReturnWhen(go, minDelay, () => criteria(go));

    /// <summary>
    /// Automatically returns an object to the pool when the provided criteria first evaluates to true.
    /// <br>This is automatically set when you get an object and <see cref="ReturnCriteria"/> had already been set.</br>
    /// </summary>
    /// <param name="go">The object which will be returned to the pool.</param>
    /// <param name="criteria">The criteria that evaluates when the object is ready to be returned.</param>
    public void ReturnWhen(GameObject go, Func<bool> criteria) => ReturnWhen(go, 0, criteria);

    /// <summary>
    /// Automatically returns an object to the pool when the provided criteria first evaluates to true.
    /// <br>This is automatically set when you get an object and <see cref="ReturnCriteria"/> had already been set.</br>
    /// </summary>
    /// <param name="go">The object which will be returned to the pool.</param>
    /// <param name="minDelay">Minimum amount of seconds before the criteria is enabled</param>
    /// <param name="criteria">The criteria that evaluates when the object is ready to be returned.</param>
    public void ReturnWhen(GameObject go, float minDelay, Func<bool> criteria) =>
        EasySingleton.GetInstance.InvokeDelayed(minDelay,
        () => EasySingleton.GetInstance.InvokeDelayed(new WaitUntil(criteria), () => Return(go)));

    public void ReturnAll() => list.ForEach(go => Return(go));

    public IEnumerator<GameObject> GetEnumerator() => list.Where(go => !UsableCriteria(go)).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => list.Where(go => !UsableCriteria(go)).GetEnumerator();
}