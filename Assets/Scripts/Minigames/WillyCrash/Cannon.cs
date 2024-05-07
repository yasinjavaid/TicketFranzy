using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class Cannon : MonoBehaviour
{
    [SerializeField] private Rigidbody ball;
    [SerializeField] private Transform cannonBarrel;
    [SerializeField] private Vector3 cannonBarrelRotateAngle = Vector3.zero;
    [SerializeField] private Vector3 shotDirection = Vector3.zero;
    [SerializeField] private float shotForce = Single.NaN;
    [SerializeField] private RagdollManage ragdollManage;


    private void OnEnable()
    {
        GameInput.Register("Launch",GameInput.ReferencePriorities.Character, Method);
    }

    public void Start()
    {
        StartBarrelRotate();
    }

    private void StartBarrelRotate()
    {
        cannonBarrel.DOLocalRotate(cannonBarrelRotateAngle, 2, RotateMode.Fast).SetLoops(-1, LoopType.Yoyo);
    }
    

    private bool Method(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            ShotBall();
            return true;
        }

        return false;
    }
    private void OnDisable()
    {
        GameInput.Deregister("Launch",GameInput.ReferencePriorities.Character, Method);
    }

    private void ShotBall()
    {
        ragdollManage.ShotPlayer(shotForce);
       //    ball.isKinematic = false;
       // ball.transform.gameObject.SetActive(true);
        //ball.transform.parent = null;
        //ball.AddRelativeForce (ball.transform.up * shotForce, ForceMode.Impulse);
     
    }
}
