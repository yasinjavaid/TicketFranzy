using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Disables this game object whenever this component is enabled
/// </summary>
public class DisableOnEnable : MonoBehaviour
{
    private void OnEnable()
    {
        gameObject.SetActive(false);
    }
}
