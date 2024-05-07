using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinLights : MonoBehaviour
{
    [SerializeField] private int points;
    Material mat;


    private void Awake()
    {
        mat = GetComponent<Renderer>().material;
    }

    public Material GetMaterial()
    {
        return mat;
    }

    public void EnableEmission()
    {
        mat.EnableKeyword("_EMISSION");
    }

    public void DisableEmission()
    {
        mat.DisableKeyword("_EMISSION");
    }

    public int getPoints()
    {
        return points;
    }

    public void blinkLight()
    {
        if (mat.IsKeywordEnabled("_EMISSION"))
        {
            DisableEmission();
        }
        else
        {
            EnableEmission();
        }
    }

}
