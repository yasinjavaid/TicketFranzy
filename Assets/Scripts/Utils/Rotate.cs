using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private float x, y, z;
    [SerializeField] private bool isRotate;
    // Update is called once per frame
   
    void Update()
    {
        if (isRotate)
        {
            transform.Rotate(x  *Time.deltaTime,y  *Time.deltaTime,z  *Time.deltaTime);
        }
    }
}
