using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableHashSet<T> : HashSet<T>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<T> items = new List<T>();

    public void OnBeforeSerialize()
    {
        items.Clear();
        foreach (T t in this)
            items.Add(t);
    }

    public void OnAfterDeserialize()
    {
        Clear();
        for (int i = 0; i < items.Count; i++)
            Add(items[i]);
    }
}
