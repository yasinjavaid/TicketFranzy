using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gear : MonoBehaviour
{
    float rotationSpeed = 45;
    public float ToothCount=15f;
    public float turnSpeed = 50f;
    public float rotationDisplay;
    public Vector3 rotationDisplay1;
    public Gear[] childGear;
    private float MainToothCount;

    Rigidbody rigidbody;
    public static float InspectorAngles(float angle)
    {
        angle %= 360;
        angle = angle > 180 ? angle - 360 : angle;
        return angle;
    }
    
    public void Rotate(float rotation, float toothCount)
    {
        MainToothCount = toothCount;
        float angles = InspectorAngles(transform.localRotation.eulerAngles.z);

        float clampRotation = Mathf.Clamp(rotation, -45 * MainToothCount / ToothCount, 45 * MainToothCount / ToothCount);


        this.transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, clampRotation);
        //Vector3 vector = new Vector3(0, 0, -rotation);
        //Quaternion deltaRotation = Quaternion.Euler(vector);
        //rigidbody = GetComponent<Rigidbody>();
        //rigidbody.MoveRotation(rigidbody.rotation * deltaRotation);


        //Vector3 vector = new Vector3(0, 0, -clampRotation);
        //this.transform.Rotate(vector);

        for (int i = 0; i < childGear.Length; i++)
        {
            childGear[i].Rotate(-rotation, MainToothCount);
        }
          //  childGear[i].Rotate(-rotation * ToothCount);
    }
}
