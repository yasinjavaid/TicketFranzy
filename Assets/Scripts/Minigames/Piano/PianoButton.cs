using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PianoButton : MonoBehaviour
{
   public int buttonNo;
   [SerializeField] private float buttonDownTime = 0.2f;
   [SerializeField] private Vector3 buttonDownAngle;
   public Renderer buttonRenderer;
   public void ButtonDownAnim()
   {
      transform.DOLocalRotate(buttonDownAngle, buttonDownTime);
   }

   public void ButtonUpAnim(Action OnComplete = null)
   {
      transform.DOLocalRotate(Vector3.zero, buttonDownTime).OnComplete(() => OnComplete?.Invoke());
   }
}
