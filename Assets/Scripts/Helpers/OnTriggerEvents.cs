
using UnityEngine;
using UnityEngine.Events;

public class OnTriggerEvents : MonoBehaviour
{
    public UnityEvent<Collider> OnTriggerEnter_Other;
    public UnityEvent<Collider> OnTriggerStay_Other;
    public UnityEvent<Collider> OnTriggerExit_Other;

    public UnityEvent<OnTriggerEvents> OnTriggerEnter_This;
    public UnityEvent<OnTriggerEvents> OnTriggerStay_This;
    public UnityEvent<OnTriggerEvents> OnTriggerExit_This;

    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEnter_Other?.Invoke(other);
        OnTriggerEnter_This?.Invoke(this);
    }
    private void OnTriggerStay(Collider other)
    {
        OnTriggerStay_Other?.Invoke(other);
        OnTriggerStay_This?.Invoke(this);
    }
    private void OnTriggerExit(Collider other)
    {
        OnTriggerExit_Other?.Invoke(other);
        OnTriggerExit_This?.Invoke(this);
    }
}
