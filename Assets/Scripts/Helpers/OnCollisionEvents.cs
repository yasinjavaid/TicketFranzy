using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnCollisionEvents : MonoBehaviour
{
    public UnityEvent<Collision> OnCollisionEnter_Other;
    public UnityEvent<Collision> OnCollisionStay_Other;
    public UnityEvent<Collision> OnCollisionExit_Other;

    public UnityEvent<OnCollisionEvents> OnCollision_This;
    public UnityEvent<OnCollisionEvents> OnCollisionStay_This;
    public UnityEvent<OnCollisionEvents> OnCollisionExit_This;
    private void OnCollisionEnter(Collision other)
    {
        OnCollisionEnter_Other?.Invoke(other);
        OnCollision_This?.Invoke(this);
    }
    private void OnCollisionStay(Collision other)
    {
        OnCollisionStay_Other?.Invoke(other);
        OnCollisionStay_This?.Invoke(this);
    }
    private void OnCollisionExit(Collision other)
    {
        OnCollisionExit_Other?.Invoke(other);
        OnCollisionExit_This?.Invoke(this);
    }
}
