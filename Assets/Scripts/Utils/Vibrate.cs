using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Vibrate : MonoBehaviour
{
  [SerializeField] private float duration;
  [SerializeField] private Vector3 shake;
  [SerializeField] private int vibrato;
  [SerializeField] private float randomness;
  public void VibrateRotation()
  {
    transform.DOShakeRotation(duration, shake, vibrato, randomness, true).OnComplete(() =>
    {
      transform.DOLocalRotate(Vector3.zero, 0);
    });
  }
}
