using UnityEngine;

public class ArcadePatrolArea : SingletonMB<ArcadePatrolArea>
{
    public static Vector3 GetRandomPosition()
    {
        Vector3 myPos = Instance.transform.position;
        Vector3 scale = Instance.transform.localScale / 2;
        return new Vector3(myPos.x + Random.Range(-scale.x, +scale.x), 0f, myPos.z + Random.Range(-scale.z, +scale.z));
    }
}
