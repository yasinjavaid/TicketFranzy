using System.IO;

using UnityEngine;

[ExecuteAlways]
public class Ball : MonoBehaviour
{
    [SerializeField] protected bool loadPosition;
    [SerializeField] protected bool savePosition;

    public Rigidbody GetRigidbody => _rigidBody ? _rigidBody : (_rigidBody = GetComponentInParent<Rigidbody>());
    protected Rigidbody _rigidBody;

    public SphereCollider GetCollider => _collider ? _collider : (_collider = GetComponent<SphereCollider>());
    protected SphereCollider _collider;

    public MeshRenderer GetMeshRenderer => _meshRenderer ? _meshRenderer : (_meshRenderer = GetComponent<MeshRenderer>());
    protected MeshRenderer _meshRenderer;

    public TrailRenderer GetTrailRenderer => _trailRenderer ? _trailRenderer : (_trailRenderer = GetComponent<TrailRenderer>());
    protected TrailRenderer _trailRenderer;

    public int Id = 0;
    public float Radius => GetCollider.radius * transform.lossyScale.y;
    public float Drag => GetRigidbody.drag;

    public Vector3 Position
    {
        get => GetRigidbody.position;
        set => GetRigidbody.position = value;
    }

    public Quaternion Rotation
    {
        get => transform.rotation;
        set => transform.rotation = value;
    }
    public Vector3 Velocity
    {
        get => GetRigidbody.velocity;
        set => GetRigidbody.velocity = value;
    }

    private void Update()
    {
        if (loadPosition)
        {
            loadPosition = false;
            Load();
        }

        if (savePosition)
        {
            savePosition = false;
            Save();
        }
    }

    private void Load()
    {
        string path = Path.Combine(Path.GetTempPath(), Application.companyName, Application.productName, this.GetFullName());
        if (File.Exists(path))
        {
            string[] lines = File.ReadAllLines(path);
            transform.position = new Vector3(float.Parse(lines[0]), float.Parse(lines[1]), float.Parse(lines[2]));
        }
    }

    private void Save()
    {
        string path = Path.Combine(Path.GetTempPath(), Application.companyName, Application.productName, this.GetFullName());
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Application.companyName));
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Application.companyName, Application.productName));
        File.WriteAllText(path, Position.x + "\n" + Position.y + "\n" + Position.z);
    }
}