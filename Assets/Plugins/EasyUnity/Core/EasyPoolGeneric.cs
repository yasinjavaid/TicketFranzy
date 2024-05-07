using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using EasyUnityInternals;
using System.Collections;

[Serializable]
public class EasyPool<T> : IEnumerable<T> where T : Component
{
    [SerializeField] protected List<T> list;

    /// <summary>
    /// The template of T from which copies will be instantiated
    /// </summary>
    public T Template { get; set; }

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
    /// <code>Default: (T t) => !t.gameObject.activeSelf;</code>
    /// </summary>
    public Func<T, bool> UsableCriteria { get; set; } = (T t) => !t.gameObject.activeSelf;

    /// <summary>
    /// The action invoked on objects when they fill their <see cref="ReturnCriteria"/>.
    /// <br>This is intented to be used to return the object to the pool (by turning the condition on Usable Criteria to true).</br>
    /// <code>Default: (T t) => t.gameObject.SetActive(false);</code>
    /// </summary>
    public Action<T> ReturnAction { get; set; } = (T t) => t.gameObject.SetActive(false);

    /// <summary>
    /// If set, when this criteria first evaluates to true, <see cref="ReturnAction"/> is invoked.
    /// <br>This is intented to be used to automatically return the object to the pool.</br>
    /// <code>Default: null</code>
    /// </summary>
    public Func<T, bool> ReturnCriteria { get; set; }

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
    public EasyPool(T template, Transform parent)
    {
        list = new List<T>();
        Template = template;
        Parent = parent;
    }

    /// <summary>
    /// Gets the first object in the pool that satisfies the <see cref="UsableCriteria"/>.
    /// <br>If none is found, this instantiates a new one.</br>
    /// </summary>
    /// <returns></returns>
    public T Get()
    {
        list.RemoveAll(a => !a);
        T t = list.FirstOrDefault(a => UsableCriteria(a));
        if (!t && !IsFull) list.Add(t = Object.Instantiate(Template, Parent));
        if (t)
        {
            t.gameObject.SetActive(true);
            if (ReturnCriteria != null) ReturnWhen(t, ReturnMinDelay, ReturnCriteria);
        }
        return t;
    }

    /// <summary>
    /// Invokes the ReturnAction on the object to return it to the pool
    /// </summary>
    /// <param name="t">The object which is being returned to the pool</param>
    public void Return(T t) => ReturnAction(t);

    /// <summary>
    /// Automatically returns an object to the pool when the provided criteria first evaluates to true.
    /// <br>This is automatically set when you get an object and <see cref="ReturnCriteria"/> had already been set.</br>
    /// </summary>
    /// <param name="t">The object which will be returned to the pool.</param>
    /// <param name="criteria">The criteria that evaluates when the object is ready to be returned.</param>
    public void ReturnWhen(T t, Func<T, bool> criteria) => ReturnWhen(t, 0, () => criteria(t));

    /// <summary>
    /// Automatically returns an object to the pool when the provided criteria first evaluates to true.
    /// <br>This is automatically set when you get an object and <see cref="ReturnCriteria"/> had already been set.</br>
    /// </summary>
    /// <param name="t">The object which will be returned to the pool.</param>
    /// <param name="minDelay">Minimum amount of seconds before the criteria is enabled</param>
    /// <param name="criteria">The criteria that evaluates when the object is ready to be returned.</param>
    public void ReturnWhen(T t, float minDelay, Func<T, bool> criteria) => ReturnWhen(t, minDelay, () => criteria(t));

    /// <summary>
    /// Automatically returns an object to the pool when the provided criteria first evaluates to true.
    /// <br>This is automatically set when you get an object and <see cref="ReturnCriteria"/> had already been set.</br>
    /// </summary>
    /// <param name="t">The object which will be returned to the pool.</param>
    /// <param name="criteria">The criteria that evaluates when the object is ready to be returned.</param>
    public void ReturnWhen(T t, Func<bool> criteria) => ReturnWhen(t, 0, criteria);

    /// <summary>
    /// Automatically returns an object to the pool when the provided criteria first evaluates to true.
    /// <br>This is automatically set when you get an object and <see cref="ReturnCriteria"/> had already been set.</br>
    /// </summary>
    /// <param name="t">The object which will be returned to the pool.</param>
    /// <param name="minDelay">Minimum amount of seconds before the criteria is enabled</param>
    /// <param name="criteria">The criteria that evaluates when the object is ready to be returned.</param>
    public void ReturnWhen(T t, float minDelay, Func<bool> criteria) => 
        EasySingleton.GetInstance.InvokeDelayed(minDelay,
        () => EasySingleton.GetInstance.InvokeDelayed(new WaitUntil(criteria), () => Return(t)));

    public void ReturnAll() => list.ForEach(t => Return(t));

    public IEnumerator<T> GetEnumerator() => list.Where(t => !UsableCriteria(t)).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => list.Where(t => !UsableCriteria(t)).GetEnumerator();
}