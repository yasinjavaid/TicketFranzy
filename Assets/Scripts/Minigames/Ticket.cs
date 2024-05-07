
using UnityEngine;

public class Ticket : MonoBehaviour
{
    public Rigidbody GetRigidbody => _rigidBody ? _rigidBody : (_rigidBody = GetComponent<Rigidbody>());
    [SerializeField] protected Rigidbody _rigidBody;

    public MeshFilter GetMeshFilter => _meshFilter ? _meshFilter : (_meshFilter = GetComponent<MeshFilter>());
    [SerializeField] protected MeshFilter _meshFilter;

    public MeshRenderer GetMeshRenderer => _meshRenderer ? _meshRenderer : (_meshRenderer = GetComponent<MeshRenderer>());
    [SerializeField] protected MeshRenderer _meshRenderer;

    public HingeJoint GetHingeJoint => _hingeJoint ? _hingeJoint : (_hingeJoint = GetComponent<HingeJoint>());
    [SerializeField] protected HingeJoint _hingeJoint;
}