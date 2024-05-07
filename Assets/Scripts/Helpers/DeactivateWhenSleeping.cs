using UnityEngine;

public class DeactivateWhenSleeping : MonoBehaviour
{
    public Rigidbody GetRigidbody => _rigidBody ? _rigidBody : (_rigidBody = GetComponentInParent<Rigidbody>());
    protected Rigidbody _rigidBody;

    private void FixedUpdate()
    {
        if (GetRigidbody.IsSleeping())
            gameObject.SetActive(false);
    }
}
